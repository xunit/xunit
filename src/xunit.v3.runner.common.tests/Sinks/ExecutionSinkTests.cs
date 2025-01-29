#pragma warning disable xUnit1051  // The TestableExecutionSink factory function does not always need a cancellation token

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class ExecutionSinkTests
{
	public class Cancellation
	{
		[Fact]
		public void ReturnsFalseWhenCancellationTokenCancellationRequested()
		{
			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.Cancel();
			using var sink = TestableExecutionSink.Create(cancellationToken: cancellationTokenSource.Token);

			var result = sink.OnMessage(TestData.DiagnosticMessage());

			Assert.False(result);
		}

		[Fact]
		public void ReturnsTrueWhenCancellationTokenCancellationHasNotBeenRequested()
		{
			var cancellationTokenSource = new CancellationTokenSource();
			using var sink = TestableExecutionSink.Create(cancellationToken: cancellationTokenSource.Token);

			var result = sink.OnMessage(TestData.DiagnosticMessage());

			Assert.True(result);
		}
	}

	public class DiscoveryMessageConversion
	{
		[Theory]
		[InlineData(AppDomainOption.Enabled, true)]
		[InlineData(AppDomainOption.Disabled, false)]
		public void ConvertsDiscoveryStarting(
			AppDomainOption appDomain,
			bool shadowCopy)
		{
			var assembly = TestData.XunitProjectAssembly<ExecutionSinkTests>();
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions(culture: "fr-FR");
			using var sink = TestableExecutionSink.Create(assembly: assembly, discoveryOptions: discoveryOptions, appDomainOption: appDomain, shadowCopy: shadowCopy);
			var testMessage = TestData.DiscoveryStarting();

			sink.OnMessage(testMessage);

			var result = Assert.Single(sink.InnerSink.Messages.OfType<TestAssemblyDiscoveryStarting>());
			Assert.Same(assembly, result.Assembly);
			Assert.Equal(appDomain, result.AppDomain);
			Assert.Same(discoveryOptions, result.DiscoveryOptions);
			Assert.Equal(shadowCopy, result.ShadowCopy);
		}

		[Fact]
		public void ConvertsDiscoveryComplete()
		{
			var assembly = TestData.XunitProjectAssembly<ExecutionSinkTests>();
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions(culture: "fr-FR");
			using var sink = TestableExecutionSink.Create(assembly: assembly, discoveryOptions: discoveryOptions);
			var testMessage = TestData.DiscoveryComplete(testCasesToRun: 42);

			sink.OnMessage(testMessage);

			var result = Assert.Single(sink.InnerSink.Messages.OfType<TestAssemblyDiscoveryFinished>());
			Assert.Same(assembly, result.Assembly);
			Assert.Same(discoveryOptions, result.DiscoveryOptions);
			Assert.Equal(42, result.TestCasesToRun);
		}
	}

	public class ExecutionMessageConversion
	{
		[Fact]
		public void ConvertsTestAssemblyStarting()
		{
			var assembly = TestData.XunitProjectAssembly<ExecutionSinkTests>();
			var executionOptions = TestData.TestFrameworkExecutionOptions(culture: "fr-FR");
			using var sink = TestableExecutionSink.Create(assembly: assembly, executionOptions: executionOptions);
			var testMessage = TestData.TestAssemblyStarting();

			sink.OnMessage(testMessage);

			var result = Assert.Single(sink.InnerSink.Messages.OfType<TestAssemblyExecutionStarting>());
			Assert.Same(assembly, result.Assembly);
			Assert.Same(executionOptions, result.ExecutionOptions);
		}

		[Fact]
		public void ConvertsTestAssemblyFinished()
		{
			var assembly = TestData.XunitProjectAssembly<ExecutionSinkTests>();
			var executionOptions = TestData.TestFrameworkExecutionOptions(culture: "fr-FR");
			using var sink = TestableExecutionSink.Create(assembly: assembly, executionOptions: executionOptions);
			var testMessage = TestData.TestAssemblyFinished();

			sink.OnMessage(testMessage);

			var result = Assert.Single(sink.InnerSink.Messages.OfType<TestAssemblyExecutionFinished>());
			Assert.Same(assembly, result.Assembly);
			Assert.Same(executionOptions, result.ExecutionOptions);
			Assert.Same(sink.ExecutionSummary, result.ExecutionSummary);
		}

		[Fact]
		public void CountsErrors()
		{
			var assembly = TestData.XunitProjectAssembly<ExecutionSinkTests>();
			var executionOptions = TestData.TestFrameworkExecutionOptions(culture: "fr-FR");
			using var sink = TestableExecutionSink.Create(assembly: assembly, executionOptions: executionOptions);
			var error = TestData.ErrorMessage();
			var finished = TestData.TestAssemblyFinished();  // Need finished message to finalized the error count

			sink.OnMessage(error);
			sink.OnMessage(finished);

			Assert.Equal(1, sink.ExecutionSummary.Errors);
		}
	}

	public class FailSkips
	{
		[Fact]
		public void OnTestSkipped_TransformsToTestFailed()
		{
			var startingMessage = TestData.TestStarting();
			var skippedMessage = TestData.TestSkipped(reason: "The skip reason");
			using var sink = TestableExecutionSink.Create(failSkips: true);

			sink.OnMessage(startingMessage);
			sink.OnMessage(skippedMessage);

			var outputMessage = Assert.Single(sink.InnerSink.Messages.OfType<ITestFailed>());
			Assert.Equal(FailureCause.Other, outputMessage.Cause);
			Assert.Equal(0M, outputMessage.ExecutionTime);
			Assert.Empty(outputMessage.Output);
			Assert.Equal("FAIL_SKIP", outputMessage.ExceptionTypes.Single());
			Assert.Equal("The skip reason", outputMessage.Messages.Single());
			var stackTrace = Assert.Single(outputMessage.StackTraces);
			Assert.Equal("", stackTrace);
		}

		public static TheoryData<IMessageSinkMessage> FinishedMessages = new()
		{
			TestData.TestCaseFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
			TestData.TestMethodFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
			TestData.TestClassFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
			TestData.TestCollectionFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
			TestData.TestAssemblyFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
		};

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(FinishedMessages))]
		public void OnFinished_CountsSkipsAsFails(IMessageSinkMessage finishedMessage)
		{
			var inputSummary = (IExecutionSummaryMetadata)finishedMessage;
			using var sink = TestableExecutionSink.Create(failSkips: true);

			sink.OnMessage(finishedMessage);

			var outputSummary = Assert.Single(sink.InnerSink.Messages.OfType<IExecutionSummaryMetadata>());
			Assert.Equal(inputSummary.TestsTotal, outputSummary.TestsTotal);
			Assert.Equal(inputSummary.TestsFailed + inputSummary.TestsSkipped, outputSummary.TestsFailed);
			Assert.Equal(inputSummary.TestsNotRun, outputSummary.TestsNotRun);
			Assert.Equal(0, outputSummary.TestsSkipped);
		}
	}

	public class FailWarn
	{
		[Fact]
		public void OnTestPassed_WithWarnings_TransformsToTestFailed()
		{
			var startingMessage = TestData.TestStarting();
			var passedMessage = TestData.TestPassed(warnings: ["warning"]);
			using var sink = TestableExecutionSink.Create(failWarn: true);

			sink.OnMessage(startingMessage);
			sink.OnMessage(passedMessage);

			var outputMessage = Assert.Single(sink.InnerSink.Messages.OfType<ITestFailed>());
			Assert.Equal(FailureCause.Other, outputMessage.Cause);
			Assert.Equal("FAIL_WARN", outputMessage.ExceptionTypes.Single());
			Assert.Equal("This test failed due to one or more warnings", outputMessage.Messages.Single());
			var stackTrace = Assert.Single(outputMessage.StackTraces);
			Assert.Equal("", stackTrace);
			Assert.NotNull(outputMessage.Warnings);
			var warning = Assert.Single(outputMessage.Warnings);
			Assert.Equal("warning", warning);
		}

		public static TheoryData<ITestResultMessage> OtherWarningMessages = new()
		{
			TestData.TestPassed(warnings: null),
			TestData.TestFailed(warnings: null),
			TestData.TestFailed(warnings: ["warning"]),
			TestData.TestSkipped(warnings: null),
			TestData.TestSkipped(warnings: ["warning"]),
			TestData.TestNotRun(warnings: null),
			TestData.TestNotRun(warnings: ["warning"]),
		};

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(OtherWarningMessages))]
		public void OtherResultMessages_PassesThrough(ITestResultMessage inputResult)
		{
			var startingMessage = TestData.TestStarting();
			using var sink = TestableExecutionSink.Create(failWarn: true);

			sink.OnMessage(startingMessage);
			sink.OnMessage(inputResult);

			var outputResult = Assert.Single(sink.InnerSink.Messages.OfType<ITestResultMessage>());
			Assert.Same(inputResult, outputResult);
		}

		public static TheoryData<IMessageSinkMessage> FinishedMessages = new()
		{
			TestData.TestCaseFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
			TestData.TestMethodFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
			TestData.TestClassFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
			TestData.TestCollectionFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
			TestData.TestAssemblyFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
		};

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(FinishedMessages))]
		public void OnFinished_CountsWarnsAsFails(IMessageSinkMessage finishedMessage)
		{
			var startingMessage = TestData.TestStarting();
			var passedMessage = TestData.TestPassed(warnings: ["warning"]);
			var inputSummary = (IExecutionSummaryMetadata)finishedMessage;
			using var sink = TestableExecutionSink.Create(failWarn: true);

			sink.OnMessage(startingMessage);
			sink.OnMessage(passedMessage);
			sink.OnMessage(finishedMessage);

			var outputSummary = Assert.Single(sink.InnerSink.Messages.OfType<IExecutionSummaryMetadata>());
			Assert.Equal(inputSummary.TestsTotal, outputSummary.TestsTotal);
			Assert.Equal(inputSummary.TestsFailed + 1, outputSummary.TestsFailed);
			Assert.Equal(inputSummary.TestsNotRun, outputSummary.TestsNotRun);
			Assert.Equal(inputSummary.TestsSkipped, outputSummary.TestsSkipped);
		}
	}

	public class LongRunningTestDetection
	{
		[Fact]
		public async ValueTask ShortRunningTests_NoMessages()
		{
			var events = new List<LongRunningTestsSummary>();
			using var sink = TestableExecutionSink.Create(longRunningSeconds: 1, longRunningTestCallback: events.Add);

			sink.OnMessage(TestData.TestAssemblyStarting());
			sink.OnMessage(TestData.TestCaseStarting());
			await sink.AdvanceClockAsync(100);
			sink.OnMessage(TestData.TestCaseFinished());
			sink.OnMessage(TestData.TestAssemblyFinished());

			Assert.Empty(events);
		}

		[Fact(Skip = "Flaky, need to determine why manual timing is no long effective")]
		public async ValueTask LongRunningTest_ReportedOnce()
		{
			var events = new List<LongRunningTestsSummary>();
			using var sink = TestableExecutionSink.Create(longRunningSeconds: 1, longRunningTestCallback: events.Add);
			var testCaseStarting = TestData.TestCaseStarting(testCaseDisplayName: "My test display name");

			sink.OnMessage(TestData.TestAssemblyStarting());
			sink.OnMessage(testCaseStarting);
			await sink.AdvanceClockAsync(1500);
			sink.OnMessage(TestData.TestCaseFinished());
			sink.OnMessage(TestData.TestAssemblyFinished());

			var @event = Assert.Single(events);
			Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
			var receivedTestCasePair = Assert.Single(@event.TestCases);
			Assert.Same(testCaseStarting, receivedTestCasePair.Key);
			Assert.Equal(TimeSpan.FromMilliseconds(1500), receivedTestCasePair.Value);

			var diagMessage = Assert.Single(sink.DiagnosticMessageSink.Messages.OfType<IDiagnosticMessage>());
			Assert.Equal("[Long Running Test] 'My test display name', Elapsed: 00:00:01", diagMessage.Message);
		}

		[Fact(Skip = "Flaky, need to determine why manual timing is no long effective")]
		public async ValueTask LongRunningTest_ReportedTwice()
		{
			var events = new List<LongRunningTestsSummary>();
			using var sink = TestableExecutionSink.Create(longRunningSeconds: 1, longRunningTestCallback: events.Add);
			var testCaseStarting = TestData.TestCaseStarting(testCaseDisplayName: "My test display name");

			sink.OnMessage(TestData.TestAssemblyStarting());
			sink.OnMessage(testCaseStarting);
			await sink.AdvanceClockAsync(1000);
			await sink.AdvanceClockAsync(500);
			await sink.AdvanceClockAsync(500);
			sink.OnMessage(TestData.TestCaseFinished());
			sink.OnMessage(TestData.TestAssemblyFinished());

			Assert.Collection(events,
				@event =>
				{
					Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
					var receivedTestCasePair = Assert.Single(@event.TestCases);
					Assert.Same(testCaseStarting, receivedTestCasePair.Key);
					Assert.Equal(TimeSpan.FromSeconds(1), receivedTestCasePair.Value);
				},
				@event =>
				{
					Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
					var receivedTestCasePair = Assert.Single(@event.TestCases);
					Assert.Same(testCaseStarting, receivedTestCasePair.Key);
					Assert.Equal(TimeSpan.FromSeconds(2), receivedTestCasePair.Value);
				}
			);

			Assert.Collection(sink.DiagnosticMessageSink.Messages.OfType<IDiagnosticMessage>(),
				diagMessage => Assert.Equal("[Long Running Test] 'My test display name', Elapsed: 00:00:01", diagMessage.Message),
				diagMessage => Assert.Equal("[Long Running Test] 'My test display name', Elapsed: 00:00:02", diagMessage.Message)
			);
		}

		[Fact(Skip = "Flaky, need to determine why manual timing is no long effective")]
		public async ValueTask OnlyIncludesLongRunningTests()
		{
			var events = new List<LongRunningTestsSummary>();
			using var sink = TestableExecutionSink.Create(longRunningSeconds: 1, longRunningTestCallback: events.Add);
			var testCase1Starting = TestData.TestCaseStarting(testCaseUniqueID: "1");
			var testCase2Starting = TestData.TestCaseStarting(testCaseUniqueID: "2");

			sink.OnMessage(TestData.TestAssemblyStarting());
			sink.OnMessage(testCase1Starting);
			await sink.AdvanceClockAsync(500);
			sink.OnMessage(testCase2Starting);  // Started later, hasn't run long enough
			await sink.AdvanceClockAsync(500);
			sink.OnMessage(TestData.TestCaseFinished(testCaseUniqueID: "1"));
			sink.OnMessage(TestData.TestCaseFinished(testCaseUniqueID: "2"));
			sink.OnMessage(TestData.TestAssemblyFinished());

			var @event = Assert.Single(events);
			Assert.Equal(TimeSpan.FromSeconds(1), @event.ConfiguredLongRunningTime);
			var receivedTestCasePair = Assert.Single(@event.TestCases);
			Assert.Same(testCase1Starting, receivedTestCasePair.Key);
			Assert.Equal(TimeSpan.FromSeconds(1), receivedTestCasePair.Value);
		}
	}

	public class MessageTiming
	{
		[Fact]
		public void EnsureInnerHandlerIsCalledBeforeFinishedIsSet()
		{
			TestableExecutionSink? sink = default;
			bool? isFinishedDuringDispatch = default;

			try
			{
				sink = TestableExecutionSink.Create(
					innerSinkCallback: (msg) =>
					{
						if (sink is null)
							throw new InvalidOperationException("Sink didn't exist in the callback");
						if (msg is ITestAssemblyFinished)
							isFinishedDuringDispatch = sink.Finished.WaitOne(0);
						return true;
					});

				sink.OnMessage(TestData.TestAssemblyFinished());
				var isFinishedAfterDispatch = sink.Finished.WaitOne(0);

				Assert.False(isFinishedDuringDispatch);
				Assert.True(isFinishedAfterDispatch);
			}
			finally
			{
				sink?.Dispose();
			}
		}
	}

	public class XmlCreation
	{
		[Fact]
		public void AddsAssemblyStartingInformationToXml()
		{
			var assemblyStarting = TestData.TestAssemblyStarting(
				assemblyPath: "/path/to/assembly.dll",
				assemblyUniqueID: "assembly-id",
				configFilePath: "config",
				startTime: new DateTimeOffset(2013, 7, 6, 16, 24, 32, TimeSpan.Zero),
				targetFramework: "MentalFloss,Version=v21.12",
				testEnvironment: "256-bit MentalFloss",
				testFrameworkDisplayName: "xUnit.net v14.42"
			);

			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(assemblyStarting);

			Assert.Equal("/path/to/assembly.dll", assemblyElement.Attribute("name")!.Value);
			Assert.Equal("MentalFloss,Version=v21.12", assemblyElement.Attribute("target-framework")!.Value);
			Assert.Equal("256-bit MentalFloss", assemblyElement.Attribute("environment")!.Value);
			Assert.Equal("xUnit.net v14.42", assemblyElement.Attribute("test-framework")!.Value);
			Assert.Equal("config", assemblyElement.Attribute("config-file")!.Value);
			Assert.Equal("2013-07-06", assemblyElement.Attribute("run-date")!.Value);
			Assert.Equal("16:24:32", assemblyElement.Attribute("run-time")!.Value);
		}

		[Fact]
		public void AssemblyStartingDoesNotIncludeNullValues()
		{
			var assemblyStarting = TestData.TestAssemblyStarting(assemblyPath: "/path/to/assembly.dll", configFilePath: null, targetFramework: null);
			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(assemblyStarting);

			Assert.Null(assemblyElement.Attribute("config-file"));
			Assert.Null(assemblyElement.Attribute("target-framework"));
		}

		[CulturedFact]
		public void AddsAssemblyFinishedInformationToXml()
		{
			var assemblyFinished = TestData.TestAssemblyFinished(
				testsTotal: 2112,
				testsFailed: 42,
				testsSkipped: 6,
				testsNotRun: 3,
				executionTime: 123.4567M
			);
			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);
			var errorMessage = TestData.ErrorMessage(
				exceptionParentIndices: [-1],
				exceptionTypes: ["ExceptionType"],
				messages: ["Message"],
				stackTraces: ["Stack"]
			);

			sink.OnMessage(errorMessage);
			sink.OnMessage(assemblyFinished);

			Assert.Equal("2112", assemblyElement.Attribute("total")!.Value);
			Assert.Equal("2061", assemblyElement.Attribute("passed")!.Value);
			Assert.Equal("42", assemblyElement.Attribute("failed")!.Value);
			Assert.Equal("6", assemblyElement.Attribute("skipped")!.Value);
			Assert.Equal("3", assemblyElement.Attribute("not-run")!.Value);
			Assert.Equal(123.457M.ToString(CultureInfo.InvariantCulture), assemblyElement.Attribute("time")!.Value);
			Assert.Equal("1", assemblyElement.Attribute("errors")!.Value);
		}

		[CulturedFact]
		public void AddsTestCollectionElementsToXml()
		{
			var assemblyFinished = TestData.TestAssemblyFinished();
			var testCollectionStarted = TestData.TestCollectionStarting(testCollectionDisplayName: "Collection Name", testCollectionUniqueID: "abc123");
			var testCollectionFinished = TestData.TestCollectionFinished(testsTotal: 2112, testsFailed: 42, testsSkipped: 6, testsNotRun: 3, executionTime: 123.4567m, testCollectionUniqueID: "abc123");

			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(testCollectionStarted);
			sink.OnMessage(testCollectionFinished);
			sink.OnMessage(assemblyFinished);

			var collectionElement = Assert.Single(assemblyElement.Elements("collection"));
			Assert.Equal("Collection Name", collectionElement.Attribute("name")!.Value);
			Assert.Equal("2112", collectionElement.Attribute("total")!.Value);
			Assert.Equal("2061", collectionElement.Attribute("passed")!.Value);
			Assert.Equal("42", collectionElement.Attribute("failed")!.Value);
			Assert.Equal("6", collectionElement.Attribute("skipped")!.Value);
			Assert.Equal("3", collectionElement.Attribute("not-run")!.Value);
			Assert.Equal(123.457M.ToString(CultureInfo.InvariantCulture), collectionElement.Attribute("time")!.Value);
		}

		[CulturedFact]
		public void AddsPassingTestElementToXml()
		{
			var assemblyFinished = TestData.TestAssemblyFinished();
			var assemblyStarting = TestData.TestAssemblyStarting();
			var collectionStarting = TestData.TestCollectionStarting();
			var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
			var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
			var caseStarting = TestData.TestCaseStarting(traits: TestData.EmptyTraits);
			var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
			var testPassed = TestData.TestPassed(executionTime: 123.4567809m, output: "test output");

			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(assemblyStarting);
			sink.OnMessage(collectionStarting);
			sink.OnMessage(classStarting);
			sink.OnMessage(methodStarting);
			sink.OnMessage(caseStarting);
			sink.OnMessage(testStarting);
			sink.OnMessage(testPassed);
			sink.OnMessage(assemblyFinished);

			var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
			Assert.Equal("Test Display Name", testElement.Attribute("name")!.Value);
			Assert.Equal("ExecutionSinkTests+XmlCreation+ClassUnderTest", testElement.Attribute("type")!.Value);
			Assert.Equal("TestMethod", testElement.Attribute("method")!.Value);
			Assert.Equal("Pass", testElement.Attribute("result")!.Value);
			Assert.Equal(123.4567809M.ToString(CultureInfo.InvariantCulture), testElement.Attribute("time")!.Value);
			Assert.Equal("test output", testElement.Element("output")!.Value);
			Assert.Null(testElement.Attribute("source-file"));
			Assert.Null(testElement.Attribute("source-line"));
			Assert.Empty(testElement.Elements("traits"));
			Assert.Empty(testElement.Elements("failure"));
			Assert.Empty(testElement.Elements("reason"));
		}

		[CulturedFact]
		public void EmptyOutputStringDoesNotShowUpInResultingXml()
		{
			var assemblyFinished = TestData.TestAssemblyFinished();
			var assemblyStarting = TestData.TestAssemblyStarting();
			var collectionStarting = TestData.TestCollectionStarting();
			var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
			var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
			var caseStarting = TestData.TestCaseStarting(traits: TestData.EmptyTraits);
			var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
			var testPassed = TestData.TestPassed(executionTime: 123.4567809m, output: "");

			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(assemblyStarting);
			sink.OnMessage(collectionStarting);
			sink.OnMessage(classStarting);
			sink.OnMessage(methodStarting);
			sink.OnMessage(caseStarting);
			sink.OnMessage(testStarting);
			sink.OnMessage(testPassed);
			sink.OnMessage(assemblyFinished);

			var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
			Assert.Equal("Test Display Name", testElement.Attribute("name")!.Value);
			Assert.Equal("ExecutionSinkTests+XmlCreation+ClassUnderTest", testElement.Attribute("type")!.Value);
			Assert.Equal("TestMethod", testElement.Attribute("method")!.Value);
			Assert.Equal("Pass", testElement.Attribute("result")!.Value);
			Assert.Equal(123.4567809M.ToString(CultureInfo.InvariantCulture), testElement.Attribute("time")!.Value);
			Assert.Null(testElement.Attribute("output"));
			Assert.Null(testElement.Attribute("source-file"));
			Assert.Null(testElement.Attribute("source-line"));
			Assert.Empty(testElement.Elements("traits"));
			Assert.Empty(testElement.Elements("failure"));
			Assert.Empty(testElement.Elements("reason"));
		}

		[CulturedFact]
		public void OutputStringStripsANSIInResultingXml()
		{
			var assemblyFinished = TestData.TestAssemblyFinished();
			var assemblyStarting = TestData.TestAssemblyStarting();
			var collectionStarting = TestData.TestCollectionStarting();
			var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
			var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
			var caseStarting = TestData.TestCaseStarting(traits: TestData.EmptyTraits);
			var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
			var testPassed = TestData.TestPassed(executionTime: 123.4567809m, output: "\u001B[31mtest output");

			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(assemblyStarting);
			sink.OnMessage(collectionStarting);
			sink.OnMessage(classStarting);
			sink.OnMessage(methodStarting);
			sink.OnMessage(caseStarting);
			sink.OnMessage(testStarting);
			sink.OnMessage(testPassed);
			sink.OnMessage(assemblyFinished);

			var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
			Assert.Equal("Test Display Name", testElement.Attribute("name")!.Value);
			Assert.Equal("ExecutionSinkTests+XmlCreation+ClassUnderTest", testElement.Attribute("type")!.Value);
			Assert.Equal("TestMethod", testElement.Attribute("method")!.Value);
			Assert.Equal("Pass", testElement.Attribute("result")!.Value);
			Assert.Equal(123.4567809M.ToString(CultureInfo.InvariantCulture), testElement.Attribute("time")!.Value);
			Assert.Equal("test output", testElement.Element("output")!.Value);
			Assert.Null(testElement.Attribute("source-file"));
			Assert.Null(testElement.Attribute("source-line"));
			Assert.Empty(testElement.Elements("traits"));
			Assert.Empty(testElement.Elements("failure"));
			Assert.Empty(testElement.Elements("reason"));
		}

		[CulturedFact]
		public void AddsFailingTestElementToXml()
		{
			var assemblyFinished = TestData.TestAssemblyFinished();
			var assemblyStarting = TestData.TestAssemblyStarting();
			var collectionStarting = TestData.TestCollectionStarting();
			var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
			var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
			var caseStarting = TestData.TestCaseStarting();
			var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
			var testFailed = TestData.TestFailed(
				exceptionParentIndices: [-1],
				exceptionTypes: ["Exception Type"],
				executionTime: 123.4567809m,
				messages: ["Exception Message"],
				output: "test output",
				stackTraces: ["Exception Stack Trace"]
			);

			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(assemblyStarting);
			sink.OnMessage(collectionStarting);
			sink.OnMessage(classStarting);
			sink.OnMessage(methodStarting);
			sink.OnMessage(caseStarting);
			sink.OnMessage(testStarting);
			sink.OnMessage(testFailed);
			sink.OnMessage(assemblyFinished);

			var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
			Assert.Equal("Test Display Name", testElement.Attribute("name")!.Value);
			Assert.Equal("ExecutionSinkTests+XmlCreation+ClassUnderTest", testElement.Attribute("type")!.Value);
			Assert.Equal("TestMethod", testElement.Attribute("method")!.Value);
			Assert.Equal("Fail", testElement.Attribute("result")!.Value);
			Assert.Equal(123.4567809M.ToString(CultureInfo.InvariantCulture), testElement.Attribute("time")!.Value);
			Assert.Equal("test output", testElement.Element("output")!.Value);
			var failureElement = Assert.Single(testElement.Elements("failure"));
			Assert.Equal("Exception Type", failureElement.Attribute("exception-type")!.Value);
			Assert.Equal("Exception Type : Exception Message", failureElement.Elements("message").Single().Value);
			Assert.Equal("Exception Stack Trace", failureElement.Elements("stack-trace").Single().Value);
			Assert.Empty(testElement.Elements("reason"));
		}

		[Fact]
		public void NullStackTraceInFailedTestResultsInEmptyStackTraceXmlElement()
		{
			var assemblyFinished = TestData.TestAssemblyFinished();
			var assemblyStarting = TestData.TestAssemblyStarting();
			var collectionStarting = TestData.TestCollectionStarting();
			var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
			var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
			var caseStarting = TestData.TestCaseStarting();
			var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
			var testFailed = TestData.TestFailed(
				exceptionParentIndices: [-1],
				exceptionTypes: ["Exception Type"],
				executionTime: 123.4567809m,
				messages: ["Exception Message"],
				output: "test output",
				stackTraces: [default(string)]
			);

			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(assemblyStarting);
			sink.OnMessage(collectionStarting);
			sink.OnMessage(classStarting);
			sink.OnMessage(methodStarting);
			sink.OnMessage(caseStarting);
			sink.OnMessage(testStarting);
			sink.OnMessage(testFailed);
			sink.OnMessage(assemblyFinished);

			var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
			var failureElement = Assert.Single(testElement.Elements("failure"));
			Assert.Empty(failureElement.Elements("stack-trace").Single().Value);
		}

		[CulturedFact]
		public void AddsSkippedTestElementToXml()
		{
			var assemblyFinished = TestData.TestAssemblyFinished();
			var assemblyStarting = TestData.TestAssemblyStarting();
			var collectionStarting = TestData.TestCollectionStarting();
			var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
			var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
			var caseStarting = TestData.TestCaseStarting();
			var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
			var testSkipped = TestData.TestSkipped(reason: "Skip Reason");

			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(assemblyStarting);
			sink.OnMessage(collectionStarting);
			sink.OnMessage(classStarting);
			sink.OnMessage(methodStarting);
			sink.OnMessage(caseStarting);
			sink.OnMessage(testStarting);
			sink.OnMessage(testSkipped);
			sink.OnMessage(assemblyFinished);

			var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
			Assert.Equal("Test Display Name", testElement.Attribute("name")!.Value);
			Assert.Equal("ExecutionSinkTests+XmlCreation+ClassUnderTest", testElement.Attribute("type")!.Value);
			Assert.Equal("TestMethod", testElement.Attribute("method")!.Value);
			Assert.Equal("Skip", testElement.Attribute("result")!.Value);
			Assert.Equal(0m.ToString(CultureInfo.InvariantCulture), testElement.Attribute("time")!.Value);
			var reasonElement = Assert.Single(testElement.Elements("reason"));
			Assert.Equal("Skip Reason", reasonElement.Value);
			Assert.Empty(testElement.Elements("failure"));
		}

		[CulturedFact]
		public void AddsNotRunTestElementToXml()
		{
			var assemblyFinished = TestData.TestAssemblyFinished();
			var assemblyStarting = TestData.TestAssemblyStarting();
			var collectionStarting = TestData.TestCollectionStarting();
			var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
			var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
			var caseStarting = TestData.TestCaseStarting();
			var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
			var testNotRun = TestData.TestNotRun();

			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(assemblyStarting);
			sink.OnMessage(collectionStarting);
			sink.OnMessage(classStarting);
			sink.OnMessage(methodStarting);
			sink.OnMessage(caseStarting);
			sink.OnMessage(testStarting);
			sink.OnMessage(testNotRun);
			sink.OnMessage(assemblyFinished);

			var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
			Assert.Equal("Test Display Name", testElement.Attribute("name")!.Value);
			Assert.Equal("ExecutionSinkTests+XmlCreation+ClassUnderTest", testElement.Attribute("type")!.Value);
			Assert.Equal("TestMethod", testElement.Attribute("method")!.Value);
			Assert.Equal("NotRun", testElement.Attribute("result")!.Value);
			Assert.Equal(0m.ToString(CultureInfo.InvariantCulture), testElement.Attribute("time")!.Value);
			Assert.Empty(testElement.Elements("failure"));
		}

		[Fact]
		public void TestElementSourceInfoIsPlacedInXmlWhenPresent()
		{
			var assemblyFinished = TestData.TestAssemblyFinished();
			var assemblyStarting = TestData.TestAssemblyStarting();
			var collectionStarting = TestData.TestCollectionStarting();
			var classStarting = TestData.TestClassStarting();
			var methodStarting = TestData.TestMethodStarting();
			var caseStarting = TestData.TestCaseStarting(sourceFilePath: "source file", sourceLineNumber: 42);
			var testStarting = TestData.TestStarting();
			var testPassed = TestData.TestPassed();

			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(assemblyStarting);
			sink.OnMessage(collectionStarting);
			sink.OnMessage(classStarting);
			sink.OnMessage(methodStarting);
			sink.OnMessage(caseStarting);
			sink.OnMessage(testStarting);
			sink.OnMessage(testPassed);
			sink.OnMessage(assemblyFinished);

			var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
			Assert.Equal("source file", testElement.Attribute("source-file")!.Value);
			Assert.Equal("42", testElement.Attribute("source-line")!.Value);
		}

		[Fact]
		public void TestElementTraitsArePlacedInXmlWhenPresent()
		{
			var traits = new Dictionary<string, IReadOnlyCollection<string>>
			{
				{ "name1", new List<string> { "value1" }},
				{ "name2", new List<string> { "value2" }}
			};
			var assemblyFinished = TestData.TestAssemblyFinished();
			var assemblyStarting = TestData.TestAssemblyStarting();
			var collectionStarting = TestData.TestCollectionStarting();
			var classStarting = TestData.TestClassStarting();
			var methodStarting = TestData.TestMethodStarting();
			var caseStarting = TestData.TestCaseStarting(traits: traits);
			var testStarting = TestData.TestStarting();
			var testPassed = TestData.TestPassed();

			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(assemblyStarting);
			sink.OnMessage(collectionStarting);
			sink.OnMessage(classStarting);
			sink.OnMessage(methodStarting);
			sink.OnMessage(caseStarting);
			sink.OnMessage(testStarting);
			sink.OnMessage(testPassed);
			sink.OnMessage(assemblyFinished);

			var traitsElements = assemblyElement.Elements("collection").Single().Elements("test").Single().Elements("traits").Single().Elements("trait");
			var name1Element = Assert.Single(traitsElements, e => e.Attribute("name")?.Value == "name1");
			Assert.Equal("value1", name1Element.Attribute("value")!.Value);
			var name2Element = Assert.Single(traitsElements, e => e.Attribute("name")?.Value == "name2");
			Assert.Equal("value2", name2Element.Attribute("value")!.Value);
		}

		public static IEnumerable<object[]> IllegalXmlTestData()
		{
			yield return new object[]
			{
				new string(Enumerable.Range(0, 32).Select(x => (char)x).ToArray()),
				@"\0\x01\x02\x03\x04\x05\x06\a\b\t\n\v\f\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f"
			};
			// Invalid surrogate characters should be added as \x----, where ---- is the hex value of the char
			yield return new object[]
			{
				"\xd800 Hello.World \xdc00",
				@"\xd800 Hello.World \xdc00"
			};
			// ...but valid ones should be outputted like normal
			yield return new object[]
			{
				"\xd800\xdfff This.Is.Valid \xda00\xdd00",
				"\xd800\xdfff This.Is.Valid \xda00\xdd00" // note: no @
			};
		}

		[Theory]
		[MemberData(nameof(IllegalXmlTestData))]
		public void IllegalXmlAcceptanceTest(
			string inputName,
			string outputName)
		{
			var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
			var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
			var testStarting = TestData.TestStarting(testDisplayName: inputName);
			var testSkipped = TestData.TestSkipped(reason: "Bad\0\r\nString");

			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(TestData.TestAssemblyStarting());
			sink.OnMessage(TestData.TestCollectionStarting());
			sink.OnMessage(classStarting);
			sink.OnMessage(methodStarting);
			sink.OnMessage(TestData.TestCaseStarting());
			sink.OnMessage(testStarting);
			sink.OnMessage(testSkipped);
			sink.OnMessage(TestData.TestFinished());
			sink.OnMessage(TestData.TestCaseFinished());
			sink.OnMessage(TestData.TestMethodFinished());
			sink.OnMessage(TestData.TestClassFinished());
			sink.OnMessage(TestData.TestCollectionFinished());
			sink.OnMessage(TestData.TestAssemblyFinished());

			using var writer = new StringWriter();
			assemblyElement.Save(writer, SaveOptions.DisableFormatting);
			var outputXml = writer.ToString();

			var parsedXml = XDocument.Parse(outputXml);
			var testElement = parsedXml.XPathSelectElement("/assembly/collection/test");
			Assert.NotNull(testElement);
			Assert.Equal(outputName, testElement.Attribute("name")?.Value);
			var reasonElement = testElement.XPathSelectElement("reason");
			Assert.NotNull(reasonElement);
			Assert.Equal("Bad\\0\nString", reasonElement.Value);
		}

		class ClassUnderTest
		{
			[Fact]
			public void TestMethod() { }
		}

		readonly string assemblyID = "assembly-id";
		readonly string classID = "test-class-id";
		readonly string collectionID = "test-collection-id";
		readonly int[] exceptionParentIndices = [-1];
		readonly string[] exceptionTypes = ["ExceptionType"];
		readonly string[] messages = ["This is\t\r\nmy message"];
		readonly string methodID = "test-method-id";
		readonly string[] stackTraces = ["Line 1\r\nLine 2\r\nLine 3"];
		readonly string testCaseID = "test-case-id";
		readonly string testID = "test-id";

		[Fact]
		public void ErrorMessage()
		{
			var errorMessage = TestData.ErrorMessage(
				exceptionParentIndices: exceptionParentIndices,
				exceptionTypes: exceptionTypes,
				messages: messages,
				stackTraces: stackTraces
			);
			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(errorMessage);

			AssertFailureElement(assemblyElement, "fatal", null);
		}

		[Fact]
		public void TestAssemblyCleanupFailure()
		{
			var collectionStarting = TestData.TestAssemblyStarting(
				assemblyUniqueID: assemblyID,
				assemblyName: "assembly-name",
				assemblyPath: "assembly-file-path",
				configFilePath: "config-file-path",
				startTime: DateTimeOffset.UtcNow,
				targetFramework: "target-framework",
				testEnvironment: "test-environment",
				testFrameworkDisplayName: "test-framework"
			);
			var collectionCleanupFailure = TestData.TestAssemblyCleanupFailure(
				assemblyUniqueID: assemblyID,
				exceptionParentIndices: exceptionParentIndices,
				exceptionTypes: exceptionTypes,
				messages: messages,
				stackTraces: stackTraces
			);
			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(collectionStarting);
			sink.OnMessage(collectionCleanupFailure);

			AssertFailureElement(assemblyElement, "assembly-cleanup", "assembly-file-path");
		}

		[Fact]
		public void TestCaseCleanupFailure()
		{
			var caseStarting = TestData.TestCaseStarting(
				assemblyUniqueID: assemblyID,
				testCaseUniqueID: testCaseID,
				testCaseDisplayName: "MyTestCase",
				testClassUniqueID: classID,
				testCollectionUniqueID: collectionID,
				testMethodUniqueID: methodID
			);
			var caseCleanupFailure = TestData.TestCaseCleanupFailure(
				assemblyUniqueID: assemblyID,
				exceptionParentIndices: exceptionParentIndices,
				exceptionTypes: exceptionTypes,
				messages: messages,
				stackTraces: stackTraces,
				testCaseUniqueID: testCaseID,
				testCollectionUniqueID: collectionID,
				testClassUniqueID: classID,
				testMethodUniqueID: methodID
			);
			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(caseStarting);
			sink.OnMessage(caseCleanupFailure);

			AssertFailureElement(assemblyElement, "test-case-cleanup", "MyTestCase");
		}

		[Fact]
		public void TestClassCleanupFailure()
		{
			var classStarting = TestData.TestClassStarting(
				assemblyUniqueID: assemblyID,
				testClassName: "MyType",
				testClassUniqueID: classID,
				testCollectionUniqueID: collectionID
			);
			var classCleanupFailure = TestData.TestClassCleanupFailure(
				assemblyUniqueID: assemblyID,
				exceptionParentIndices: exceptionParentIndices,
				exceptionTypes: exceptionTypes,
				messages: messages,
				testClassUniqueID: classID,
				testCollectionUniqueID: collectionID,
				stackTraces: stackTraces
			);
			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(classStarting);
			sink.OnMessage(classCleanupFailure);

			AssertFailureElement(assemblyElement, "test-class-cleanup", "MyType");
		}

		[Fact]
		public void TestCleanupFailure()
		{
			var testStarting = TestData.TestStarting(
				assemblyUniqueID: assemblyID,
				testCaseUniqueID: testCaseID,
				testClassUniqueID: classID,
				testCollectionUniqueID: collectionID,
				testDisplayName: "MyTest",
				testMethodUniqueID: methodID,
				testUniqueID: testID
			);
			var testCleanupFailure = TestData.TestCleanupFailure(
				assemblyUniqueID: assemblyID,
				exceptionParentIndices: exceptionParentIndices,
				exceptionTypes: exceptionTypes,
				messages: messages,
				stackTraces: stackTraces,
				testCaseUniqueID: testCaseID,
				testCollectionUniqueID: collectionID,
				testClassUniqueID: classID,
				testMethodUniqueID: methodID,
				testUniqueID: testID
			);
			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(testStarting);
			sink.OnMessage(testCleanupFailure);

			AssertFailureElement(assemblyElement, "test-cleanup", "MyTest");
		}

		[Fact]
		public void TestCollectionCleanupFailure()
		{
			var collectionStarting = TestData.TestCollectionStarting(
				assemblyUniqueID: assemblyID,
				testCollectionDisplayName: "FooBar",
				testCollectionUniqueID: collectionID
			);
			var collectionCleanupFailure = TestData.TestCollectionCleanupFailure(
				assemblyUniqueID: assemblyID,
				exceptionParentIndices: exceptionParentIndices,
				exceptionTypes: exceptionTypes,
				messages: messages,
				stackTraces: stackTraces,
				testCollectionUniqueID: collectionID
			);
			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(collectionStarting);
			sink.OnMessage(collectionCleanupFailure);

			AssertFailureElement(assemblyElement, "test-collection-cleanup", "FooBar");
		}

		[Fact]
		public void TestMethodCleanupFailure()
		{
			var methodStarting = TestData.TestMethodStarting(
				assemblyUniqueID: assemblyID,
				testClassUniqueID: classID,
				testCollectionUniqueID: collectionID,
				methodName: "MyMethod",
				testMethodUniqueID: methodID
			);
			var methodCleanupFailure = TestData.TestMethodCleanupFailure(
				assemblyUniqueID: assemblyID,
				exceptionParentIndices: exceptionParentIndices,
				exceptionTypes: exceptionTypes,
				messages: messages,
				stackTraces: stackTraces,
				testCollectionUniqueID: collectionID,
				testClassUniqueID: classID,
				testMethodUniqueID: methodID
			);
			var assemblyElement = new XElement("assembly");
			using var sink = TestableExecutionSink.Create(assemblyElement: assemblyElement);

			sink.OnMessage(methodStarting);
			sink.OnMessage(methodCleanupFailure);

			AssertFailureElement(assemblyElement, "test-method-cleanup", "MyMethod");
		}

		static void AssertFailureElement(
			XElement assemblyElement,
			string messageType,
			string? name)
		{
			var errorElement = Assert.Single(assemblyElement.Element("errors")!.Elements());
			Assert.Equal(messageType, errorElement.Attribute("type")!.Value);

			if (name is null)
				Assert.Null(errorElement.Attribute("name"));
			else
				Assert.Equal(name, errorElement.Attribute("name")!.Value);

			var failureElement = Assert.Single(errorElement.Elements("failure"));
			Assert.Equal("ExceptionType", failureElement.Attribute("exception-type")!.Value);
			Assert.Equal("ExceptionType : This is\\t\r\nmy message", failureElement.Elements("message").Single().Value);
			Assert.Equal("Line 1\r\nLine 2\r\nLine 3", failureElement.Elements("stack-trace").Single().Value);
		}
	}

	class TestableExecutionSink : ExecutionSink
	{
		volatile bool stop = false;
		volatile int stopEventTriggerCount;
		DateTimeOffset utcNow = DateTimeOffset.UtcNow;
		readonly AutoResetEvent workEvent = new(initialState: false);

		public TestableExecutionSink(
			XunitProjectAssembly assembly,
			ITestFrameworkDiscoveryOptions discoveryOptions,
			ITestFrameworkExecutionOptions executionOptions,
			AppDomainOption appDomainOption,
			bool shadowCopy,
			SpyMessageSink innerSink,
			SpyMessageSink diagnosticMessageSink,
			ExecutionSinkOptions options) :
				base(assembly, discoveryOptions, executionOptions, appDomainOption, shadowCopy, innerSink, options)
		{
			InnerSink = innerSink;
			DiagnosticMessageSink = diagnosticMessageSink;
			Options = options;
		}

		public SpyMessageSink DiagnosticMessageSink { get; }

		public SpyMessageSink InnerSink { get; }

		public ExecutionSinkOptions Options { get; }

		protected override DateTimeOffset UtcNow => utcNow;

		public async Task AdvanceClockAsync(int milliseconds)
		{
			utcNow += TimeSpan.FromMilliseconds(milliseconds);

			var currentCount = stopEventTriggerCount;
			workEvent.Set();

			var stopTime = DateTime.UtcNow.AddSeconds(60);

			while (stopTime > DateTime.UtcNow)
			{
				await Task.Delay(25, TestContext.Current.CancellationToken);
				if (currentCount != stopEventTriggerCount)
					return;
			}

			throw new InvalidOperationException("After AdvanceClock, next work run never happened.");
		}

		public override void Dispose()
		{
			try
			{
				// Ensure we properly clean up the worker thread if we're waiting for long-running tests
				if (Options.LongRunningTestTime > TimeSpan.Zero)
				{
					stop = true;
					workEvent.Set();

					var stopTime = DateTime.UtcNow.AddSeconds(60);

					while (stopTime > DateTime.UtcNow)
					{
						Thread.Sleep(25);
						if (stopEventTriggerCount == -1)
						{
							workEvent.Dispose();
							return;
						}
					}

					throw new InvalidOperationException("Worker thread did not shut down within 60 seconds.");
				}
			}
			finally
			{
				base.Dispose();
			}
		}

		protected override bool WaitForStopEvent(int millionsecondsDelay)
		{
			Interlocked.Increment(ref stopEventTriggerCount);

			workEvent.WaitOne();

			if (stop)
			{
				stopEventTriggerCount = -1;
				return true;
			}

			return false;
		}

		public static TestableExecutionSink Create(
			XunitProjectAssembly? assembly = null,
			ITestFrameworkDiscoveryOptions? discoveryOptions = null,
			ITestFrameworkExecutionOptions? executionOptions = null,
			AppDomainOption? appDomainOption = null,
			bool shadowCopy = false,
			XElement? assemblyElement = null,
			CancellationToken cancellationToken = default,
			Action<ExecutionSummary>? finishedCallback = null,
			bool failSkips = false,
			bool failWarn = false,
			Action<LongRunningTestsSummary>? longRunningTestCallback = null,
			long longRunningSeconds = 0L,
			Func<IMessageSinkMessage, bool>? innerSinkCallback = null)
		{
			var diagnosticMessageSink = SpyMessageSink.Capture();

			return new(
				assembly ?? TestData.XunitProjectAssembly<ExecutionSinkTests>(),
				discoveryOptions ?? TestData.TestFrameworkDiscoveryOptions(),
				executionOptions ?? TestData.TestFrameworkExecutionOptions(),
				appDomainOption ?? AppDomainOption.Disabled,
				shadowCopy,
				SpyMessageSink.Capture(innerSinkCallback),
				diagnosticMessageSink,
				new ExecutionSinkOptions
				{
					AssemblyElement = assemblyElement,
					CancelThunk = () => cancellationToken.IsCancellationRequested,
					DiagnosticMessageSink = diagnosticMessageSink,
					FinishedCallback = finishedCallback,
					FailSkips = failSkips,
					FailWarn = failWarn,
					LongRunningTestCallback = longRunningTestCallback,
					LongRunningTestTime = TimeSpan.FromSeconds(longRunningSeconds),
				}
			);
		}
	}
}
