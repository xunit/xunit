#pragma warning disable xUnit1051  // The TestableExecutionSink factory function does not always need a cancellation token

using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class ExecutionSinkTests
{
	public static class Cancellation
	{
		[Fact]
		public static void ReturnsFalseWhenCancellationTokenCancellationRequested()
		{
			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.Cancel();
			using var sink = TestableExecutionSink.Create(cancellationToken: cancellationTokenSource.Token);

			var result = sink.OnMessage(TestData.DiagnosticMessage());

			Assert.False(result);
		}

		[Fact]
		public static void ReturnsTrueWhenCancellationTokenCancellationHasNotBeenRequested()
		{
			var cancellationTokenSource = new CancellationTokenSource();
			using var sink = TestableExecutionSink.Create(cancellationToken: cancellationTokenSource.Token);

			var result = sink.OnMessage(TestData.DiagnosticMessage());

			Assert.True(result);
		}
	}

	public static class DiscoveryMessageConversion
	{
		[Theory]
		[InlineData(AppDomainOption.Enabled, true)]
		[InlineData(AppDomainOption.Disabled, false)]
		public static void ConvertsDiscoveryStarting(
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
		public static void ConvertsDiscoveryComplete()
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

	public static class ExecutionMessageConversion
	{
		[Fact]
		public static void ConvertsTestAssemblyStarting()
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
		public static void ConvertsTestAssemblyFinished()
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
		public static void CountsErrors()
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

	public static class FailSkips
	{
		[Fact]
		public static void OnTestSkipped_TransformsToTestFailed()
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
		public static void OnFinished_CountsSkipsAsFails(IMessageSinkMessage finishedMessage)
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

	public static class FailWarn
	{
		[Fact]
		public static void OnTestPassed_WithWarnings_TransformsToTestFailed()
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
		public static void OtherResultMessages_PassesThrough(ITestResultMessage inputResult)
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
		public static void OnFinished_CountsWarnsAsFails(IMessageSinkMessage finishedMessage)
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

	public static class MessageTiming
	{
		[Fact]
		public static void EnsureInnerHandlerIsCalledBeforeFinishedIsSet()
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

	class TestableExecutionSink(
		XunitProjectAssembly assembly,
		ITestFrameworkDiscoveryOptions discoveryOptions,
		ITestFrameworkExecutionOptions executionOptions,
		AppDomainOption appDomainOption,
		bool shadowCopy,
		SpyMessageSink innerSink,
		SpyMessageSink diagnosticMessageSink,
		ExecutionSinkOptions options) :
			ExecutionSink(assembly, discoveryOptions, executionOptions, appDomainOption, shadowCopy, innerSink, options)
	{
		volatile bool stop = false;
		volatile int stopEventTriggerCount;
		DateTimeOffset utcNow = DateTimeOffset.UtcNow;
		readonly AutoResetEvent workEvent = new(initialState: false);

		public SpyMessageSink DiagnosticMessageSink { get; } = diagnosticMessageSink;

		public SpyMessageSink InnerSink { get; } = innerSink;

		public ExecutionSinkOptions Options { get; } = options;

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
			Action<ExecutionSummary>? finishedCallback = null,
			bool failSkips = false,
			bool failWarn = false,
			Action<LongRunningTestsSummary>? longRunningTestCallback = null,
			long longRunningSeconds = 0L,
			Func<IMessageSinkMessage, bool>? innerSinkCallback = null,
			CancellationToken cancellationToken = default)
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
					CancelThunk = () => cancellationToken.IsCancellationRequested,
					DiagnosticMessageSink = diagnosticMessageSink,
					FinishedCallback = finishedCallback,
					FailSkips = failSkips,
					FailWarn = failWarn,
					LongRunningTestCallback = longRunningTestCallback,
					LongRunningTestTime = TimeSpan.FromSeconds(longRunningSeconds),
					ResultWriterMessageHandlers = [],
				}
			);
		}
	}
}
