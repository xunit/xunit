using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

[CleanEnvironment("COMPUTERNAME", "HOSTNAME", "NAME", "HOST", "USERNAME", "LOGNAME", "USER")]
public class TrxResultWriterMessageHandlerTests
{
	static readonly XNamespace ns = XNamespace.Get("http://microsoft.com/schemas/VisualStudio/TeamTest/2010");

	[Theory]
	// Windows
	[InlineData("COMPUTERNAME", "USERNAME")]
	// Linux
	[InlineData("HOSTNAME", "LOGNAME")]
	[InlineData("HOSTNAME", "USER")]
	[InlineData("NAME", "LOGNAME")]
	[InlineData("NAME", "USER")]
	// macOS
	[InlineData("HOST", "LOGNAME")]
	[InlineData("HOST", "USER")]
	public async ValueTask TestRunElement(
		string computerEnvName,
		string userEnvName)
	{
		Environment.SetEnvironmentVariable(computerEnvName, "expected-computer");
		Environment.SetEnvironmentVariable(userEnvName, "expected-user");

		await using var handler = TestableTrxResultWriterMessageHandler.Create();

		var testRunElement = await handler.TestRunElement();
		Guid.Parse(testRunElement.Attribute("id")!.Value);  // Just ensure it's a GUID, it's randomized every time
		Assert.Equal("expected-user@expected-computer 0001-01-01T00:00:00.0000000+00:00", testRunElement.Attribute("name")?.Value);
		Assert.Equal("expected-user", testRunElement.Attribute("runUser")?.Value);

		var timesElement = testRunElement.Element(ns + "Times");
		Assert.NotNull(timesElement);
		Assert.Equal("0001-01-01T00:00:00.0000000+00:00", timesElement.Attribute("creation")?.Value);
		Assert.Equal("0001-01-01T00:00:00.0000000+00:00", timesElement.Attribute("queuing")?.Value);
		Assert.Equal("0001-01-01T00:00:00.0000000+00:00", timesElement.Attribute("start")?.Value);
		Assert.Equal("0001-01-01T00:00:00.0000000+00:00", timesElement.Attribute("finish")?.Value);

		var testSettingsElement = testRunElement.Element(ns + "TestSettings");
		Assert.NotNull(testSettingsElement);
		Assert.Equal("default", testSettingsElement.Attribute("name")?.Value);
		Assert.Equal("6c4d5628-128d-4c3b-a1a4-ab366a4594ad", testSettingsElement.Attribute("id")?.Value);

		var resultsElement = testRunElement.Element(ns + "Results");
		Assert.NotNull(resultsElement);
		Assert.Empty(resultsElement.Elements());

		var testDefinitionsElement = testRunElement.Element(ns + "TestDefinitions");
		Assert.NotNull(testDefinitionsElement);
		Assert.Empty(testDefinitionsElement.Elements());

		var testEntriesElement = testRunElement.Element(ns + "TestEntries");
		Assert.NotNull(testEntriesElement);
		Assert.Empty(testEntriesElement.Elements());

		var testListsElement = testRunElement.Element(ns + "TestLists");
		Assert.NotNull(testListsElement);
		Assert.Collection(
			testListsElement.Elements().OrderBy(e => e.Attribute("name")?.Value),
			element =>
			{
				Assert.Equal("All Loaded Results", element.Attribute("name")?.Value);
				Assert.Equal("19431567-8539-422a-85d7-44ee4e166bda", element.Attribute("id")?.Value);
			},
			element =>
			{
				Assert.Equal("Results Not in a List", element.Attribute("name")?.Value);
				Assert.Equal("8c84fa94-04c1-424b-9868-57a2d4851a1d", element.Attribute("id")?.Value);
			}
		);

		VerifyResultSummary(testRunElement, "Completed");
	}

	[CulturedFactDefault]
	public async ValueTask TestAssemblyStartingAndFinished_RecordsTimesAndTotals()
	{
		var assemblyStarting = TestData.TestAssemblyStarting(
			startTime: new DateTimeOffset(2013, 7, 6, 16, 24, 32, TimeSpan.Zero)
		);
		var assemblyFinished = TestData.TestAssemblyFinished(
			executionTime: 123.4567M,
			finishTime: new DateTimeOffset(2013, 7, 6, 17, 32, 48, TimeSpan.Zero),
			testsFailed: 42,
			testsNotRun: 3,
			testsSkipped: 6,
			testsTotal: 2112
		);
		await using var handler = TestableTrxResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		Assert.Equal("unknown@unknown 2013-07-06T16:24:32.0000000+00:00", testRunElement.Attribute("name")?.Value);

		var timesElement = testRunElement.Element(ns + "Times");
		Assert.NotNull(timesElement);
		Assert.Equal("2013-07-06T16:24:32.0000000+00:00", timesElement.Attribute("creation")?.Value);
		Assert.Equal("2013-07-06T16:24:32.0000000+00:00", timesElement.Attribute("queuing")?.Value);
		Assert.Equal("2013-07-06T16:24:32.0000000+00:00", timesElement.Attribute("start")?.Value);
		Assert.Equal("2013-07-06T17:32:48.0000000+00:00", timesElement.Attribute("finish")?.Value);

		VerifyResultSummary(testRunElement, "Failed", failed: 42, notRun: 3, skipped: 6, passed: 2061);
	}

	[CulturedFactDefault]
	public async ValueTask TestPassed()
	{
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting(traits: TestData.EmptyTraits);
		var testStarting = TestData.TestStarting(testUniqueID: "test-id", testDisplayName: "Test Display Name");
		var testPassed = TestData.TestPassed(executionTime: 123.4567809m, output: "test output");
		await using var handler = TestableTrxResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		VerifyResultSummary(testRunElement, "Completed", passed: 1);
		var testID = VerifyTestDefinition(testRunElement);
		VerifyTestEntry(testRunElement, testID);
		VerifyResult(testRunElement, testID, "Passed");
	}

	[CulturedFactDefault]
	public async ValueTask TestFailed()
	{
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
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
		await using var handler = TestableTrxResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testFailed);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		VerifyResultSummary(testRunElement, "Failed", failed: 1);
		var testID = VerifyTestDefinition(testRunElement);
		VerifyTestEntry(testRunElement, testID);
		var resultElement = VerifyResult(testRunElement, testID, "Failed");
		var outputElement = Assert.Single(resultElement.Elements(ns + "Output"));
		var errorInfoElement = Assert.Single(outputElement.Elements(ns + "ErrorInfo"));
		var errorMessageElement = Assert.Single(errorInfoElement.Elements(ns + "Message"));
		Assert.Equal("Exception Type : Exception Message", errorMessageElement.Value);
		var errorStackTraceElement = Assert.Single(errorInfoElement.Elements(ns + "StackTrace"));
		Assert.Equal("Exception Stack Trace", errorStackTraceElement.Value);
		var messageElements = outputElement.Element(ns + "TextMessages")?.Elements(ns + "Message").CastOrToArray() ?? [];
		var messageElement = Assert.Single(messageElements);
		Assert.Equal("test output", messageElement.Value);
	}

	[Fact]
	public async ValueTask TestFailed_NullStackTrace()
	{
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
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
			stackTraces: [default]
		);
		await using var handler = TestableTrxResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testFailed);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var testID = VerifyTestDefinition(testRunElement);
		VerifyTestEntry(testRunElement, testID);
		var resultElement = VerifyResult(testRunElement, testID, "Failed");
		var outputElement = Assert.Single(resultElement.Elements(ns + "Output"));
		var errorInfoElement = Assert.Single(outputElement.Elements(ns + "ErrorInfo"));
		Assert.Empty(errorInfoElement.Elements(ns + "StackTrace"));
	}

	[CulturedFactDefault]
	public async ValueTask TestSkipped()
	{
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 1, testsTotal: 1);
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testSkipped = TestData.TestSkipped(reason: "Skip Reason", output: "test output");
		await using var handler = TestableTrxResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testSkipped);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		VerifyResultSummary(testRunElement, "Completed", skipped: 1);
		var testID = VerifyTestDefinition(testRunElement);
		VerifyTestEntry(testRunElement, testID);
		var resultElement = VerifyResult(testRunElement, testID, "NotExecuted", expectedDuration: "00:00:00.0000000");
		var outputElement = Assert.Single(resultElement.Elements(ns + "Output"));
		var stdOutElement = Assert.Single(outputElement.Elements(ns + "StdOut"));
		Assert.Equal("Skip Reason", stdOutElement.Value);
		var messageElements = outputElement.Element(ns + "TextMessages")?.Elements(ns + "Message").CastOrToArray() ?? [];
		var messageElement = Assert.Single(messageElements);
		Assert.Equal("test output", messageElement.Value);
	}

	[CulturedFactDefault]
	public async ValueTask TestNotRun()
	{
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 1, testsSkipped: 0, testsTotal: 1);
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testNotRun = TestData.TestNotRun();
		await using var handler = TestableTrxResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);

		handler.OnMessage(testNotRun);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		VerifyResultSummary(testRunElement, "Completed", notRun: 1);
		var testID = VerifyTestDefinition(testRunElement);
		VerifyTestEntry(testRunElement, testID);
		VerifyResult(testRunElement, testID, "NotRunnable", expectedDuration: "00:00:00.0000000");
	}

	[CulturedFactDefault]
	public async ValueTask TestResult_EmptyOutput()
	{
		var assemblyFinished = TestData.TestAssemblyFinished();
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting(traits: TestData.EmptyTraits);
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testPassed = TestData.TestPassed(executionTime: 123.4567809m, output: "");
		await using var handler = TestableTrxResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var testID = VerifyTestDefinition(testRunElement);
		VerifyTestEntry(testRunElement, testID);
		var resultElement = VerifyResult(testRunElement, testID, "Passed");
		Assert.Null(resultElement.Element(ns + "Output"));
	}

	[CulturedFactDefault]
	public async ValueTask TestResult_OutputIsSplitOnCRLFs()
	{
		var assemblyFinished = TestData.TestAssemblyFinished();
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting(traits: TestData.EmptyTraits);
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testPassed = TestData.TestPassed(executionTime: 123.4567809m, output: "Line1\r\nLine2\r\nLine3\r\n");
		await using var handler = TestableTrxResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var testID = VerifyTestDefinition(testRunElement);
		VerifyTestEntry(testRunElement, testID);
		var resultElement = VerifyResult(testRunElement, testID, "Passed");
		var messageElements = resultElement.Element(ns + "Output")?.Element(ns + "TextMessages")?.Elements(ns + "Message").CastOrToArray() ?? [];
		Assert.Collection(messageElements,
			element => Assert.Equal("Line1", element.Value),
			element => Assert.Equal("Line2", element.Value),
			element => Assert.Equal("Line3", element.Value)
		);
	}

	[Fact]
	public async ValueTask TestResult_WithAttachments()
	{
		var attachments = new Dictionary<string, TestAttachment>
		{
			{ "hello", TestAttachment.Create("world") },
			{ "bytes", TestAttachment.Create([1, 2, 3], "application/octet-stream") },
		};
		var assemblyFinished = TestData.TestAssemblyFinished();
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting(traits: TestData.EmptyTraits);
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testPassed = TestData.TestPassed(executionTime: 123.4567809m);
		var testFinished = TestData.TestFinished(attachments: attachments);
		await using var handler = TestableTrxResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(testFinished);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var testID = VerifyTestDefinition(testRunElement);
		VerifyTestEntry(testRunElement, testID);
		var resultElement = VerifyResult(testRunElement, testID, "Passed");
		var resultFilesElement = Assert.Single(resultElement.Elements(ns + "ResultFiles"));
		var resultFileElements = resultFilesElement.Elements(ns + "ResultFile");
		Assert.Collection(
			resultFileElements.Select(e => e.Attribute("path")?.Value).OrderBy(x => x),
			path =>
			{
				Assert.NotNull(path);
				Assert.True(handler.FileSystem.Exists(path));
				Assert.Equal("bytes.bin", Path.GetFileName(path));
				Assert.Equal([1, 2, 3], handler.FileSystem.ReadAllBytes(path));
			},
			path =>
			{
				Assert.NotNull(path);
				Assert.True(handler.FileSystem.Exists(path));
				Assert.Equal("hello.txt", Path.GetFileName(path));
				Assert.Equal("world", handler.FileSystem.ReadAllText(path));
			}
		);
	}

	public static IEnumerable<TheoryDataRow<IMessageSinkMessage>> ErrorsData =
	[
		new(TestData.ErrorMessage()),
		new(TestData.TestAssemblyCleanupFailure()),
		new(TestData.TestCaseCleanupFailure()),
		new(TestData.TestClassCleanupFailure()),
		new(TestData.TestCleanupFailure()),
		new(TestData.TestCollectionCleanupFailure()),
		new(TestData.TestMethodCleanupFailure()),
	];

	[Theory(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(ErrorsData))]
	public async ValueTask ErrorMessage(IMessageSinkMessage errorMessage)
	{
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 0);
		var assemblyStarting = TestData.TestAssemblyStarting();
		await using var handler = TestableTrxResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(errorMessage);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		VerifyResultSummary(testRunElement, "Failed", errors: 1);
	}

	static XElement VerifyResult(
		XElement testRunElement,
		string testID,
		string outcome,
		string? expectedDuration = null)
	{
		var resultsElement = Assert.Single(testRunElement.Elements(ns + "Results"));
		var resultElement = Assert.Single(resultsElement.Elements(ns + "UnitTestResult"));

		expectedDuration ??= 123.4567809m.ToTimespanRtf();

		Assert.Equal("Test Display Name", resultElement.Attribute("testName")?.Value);
		Assert.Equal(outcome, resultElement.Attribute("outcome")?.Value);
		Assert.Equal("13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b", resultElement.Attribute("testType")?.Value);
		Assert.Equal("8c84fa94-04c1-424b-9868-57a2d4851a1d", resultElement.Attribute("testListId")?.Value);
		Assert.Equal(testID, resultElement.Attribute("testId")?.Value);
		Assert.Equal(testID, resultElement.Attribute("executionId")?.Value);
		Assert.Equal("unknown", resultElement.Attribute("computerName")?.Value);
		Assert.Equal(expectedDuration, resultElement.Attribute("duration")?.Value);
		Assert.Equal("2024-07-04T21:12:08.0000000+00:00", resultElement.Attribute("startTime")?.Value);
		Assert.Equal("2024-07-04T21:12:28.0000000+00:00", resultElement.Attribute("endTime")?.Value);

		return resultElement;
	}

	static void VerifyResultSummary(
		XElement testRunElement,
		string outcome,
		int errors = 0,
		int failed = 0,
		int notRun = 0,
		int passed = 0,
		int skipped = 0)
	{
		var resultSummaryElement = Assert.Single(testRunElement.Elements(ns + "ResultSummary"));
		Assert.Equal(outcome, resultSummaryElement.Attribute("outcome")?.Value);

		var total = failed + notRun + passed + skipped;
		var executed = failed + passed;
		var counters = resultSummaryElement.Element(ns + "Counters");
		Assert.NotNull(counters);
		Assert.Equal(total.ToString(), counters.Attribute("total")?.Value);
		Assert.Equal(executed.ToString(), counters.Attribute("executed")?.Value);
		Assert.Equal(passed.ToString(), counters.Attribute("passed")?.Value);
		Assert.Equal(failed.ToString(), counters.Attribute("failed")?.Value);
		Assert.Equal(errors.ToString(), counters.Attribute("error")?.Value);
		Assert.Equal("0", counters.Attribute("timeout")?.Value);
		Assert.Equal("0", counters.Attribute("aborted")?.Value);
		Assert.Equal("0", counters.Attribute("inconclusive")?.Value);
		Assert.Equal("0", counters.Attribute("passedButRunAborted")?.Value);
		Assert.Equal(notRun.ToString(), counters.Attribute("notRunnable")?.Value);
		Assert.Equal(skipped.ToString(), counters.Attribute("notExecuted")?.Value);
		Assert.Equal("0", counters.Attribute("disconnected")?.Value);
		Assert.Equal("0", counters.Attribute("warning")?.Value);
		Assert.Equal("0", counters.Attribute("completed")?.Value);
		Assert.Equal("0", counters.Attribute("inProgress")?.Value);
		Assert.Equal("0", counters.Attribute("pending")?.Value);
	}

	static string VerifyTestDefinition(XElement testRunElement)
	{
		var testDefinitionsElement = Assert.Single(testRunElement.Elements(ns + "TestDefinitions"));
		var unitTestElement = Assert.Single(testDefinitionsElement.Elements(ns + "UnitTest"));
		var testID = unitTestElement.Attribute("id")?.Value;

		Assert.NotNull(testID);
		Guid.Parse(testID);

		Assert.Equal("Test Display Name", unitTestElement.Attribute("name")?.Value);
		Assert.Equal("./test-assembly.dll", unitTestElement.Attribute("storage")?.Value);

		var executionElement = Assert.Single(unitTestElement.Elements(ns + "Execution"));
		Assert.Equal(testID, executionElement.Attribute("id")?.Value);

		var testMethodElement = Assert.Single(unitTestElement.Elements(ns + "TestMethod"));
		Assert.Equal("./test-assembly.dll", testMethodElement.Attribute("codeBase")?.Value);
		Assert.Equal(typeof(ClassUnderTest).FullName, testMethodElement.Attribute("className")?.Value);
		Assert.Equal(nameof(ClassUnderTest.TestMethod), testMethodElement.Attribute("name")?.Value);
		Assert.Matches(@"^executor://[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/xunit\.v3/\d+\.\d+\.\d+$", testMethodElement.Attribute("adapterTypeName")?.Value);

		return testID;
	}

	static void VerifyTestEntry(
		XElement testRunElement,
		string testID)
	{
		var testEntriesElement = Assert.Single(testRunElement.Elements(ns + "TestEntries"));
		var testEntryElement = Assert.Single(testEntriesElement.Elements(ns + "TestEntry"));

		Assert.Equal("8c84fa94-04c1-424b-9868-57a2d4851a1d", testEntryElement.Attribute("testListId")?.Value);
		Assert.Equal(testID, testEntryElement.Attribute("testId")?.Value);
		Assert.Equal(testID, testEntryElement.Attribute("executionId")?.Value);
	}

	class ClassUnderTest
	{
		[Fact]
		public async ValueTask TestMethod() { }
	}

	class TestableTrxResultWriterMessageHandler : TrxResultWriterMessageHandler
	{
		private readonly StringWriter stringWriter;

		public TestableTrxResultWriterMessageHandler(
			StringWriter stringWriter,
			XmlWriter xmlWriter,
			IFileSystem fileSystem) :
				base(xmlWriter, fileSystem)
		{
			this.stringWriter = stringWriter;
			FileSystem = fileSystem;
		}

		public IFileSystem FileSystem { get; }

		public async ValueTask<XElement> TestRunElement()
		{
			await DisposeAsync();

			var document = XDocument.Parse(stringWriter.ToString());
			var assemblyElement = document.Element(ns + "TestRun");
			Assert.NotNull(assemblyElement);

			return assemblyElement;
		}

		public static TestableTrxResultWriterMessageHandler Create()
		{
			var stringWriter = new StringWriter();
			var textWriter = new XmlTextWriter(stringWriter);

			return new TestableTrxResultWriterMessageHandler(stringWriter, textWriter, new SpyFileSystem());
		}
	}
}
