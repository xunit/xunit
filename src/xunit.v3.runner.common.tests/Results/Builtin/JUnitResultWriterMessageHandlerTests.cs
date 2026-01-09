using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

[CleanEnvironment("COMPUTERNAME", "HOSTNAME", "NAME", "HOST")]
public class JUnitResultWriterMessageHandlerTests
{
	[CulturedFactDefault]
	public async ValueTask TestSuitesElement()
	{
		await using var handler = TestableJUnitResultWriterMessageHandler.Create();

		var testSuitesElement = await handler.TestSuitesElement();
		Assert.Equal("Test results", testSuitesElement.Attribute("name")?.Value);
		Assert.Equal("0", testSuitesElement.Attribute("tests")?.Value);
		Assert.Equal("0", testSuitesElement.Attribute("failures")?.Value);
		Assert.Equal("0", testSuitesElement.Attribute("errors")?.Value);
		Assert.Equal("0", testSuitesElement.Attribute("disabled")?.Value);
		Assert.Null(testSuitesElement.Attribute("skipped")?.Value);  // No "skipped" at the top level
		Assert.Equal("0.000000", testSuitesElement.Attribute("time")?.Value);
		Assert.Equal("0001-01-01T00:00:00", testSuitesElement.Attribute("timestamp")?.Value);

		Assert.Empty(testSuitesElement.Elements("testsuite"));
	}

	[CulturedTheoryDefault]
	// Windows
	[InlineData("COMPUTERNAME")]
	// Linux
	[InlineData("HOSTNAME")]
	[InlineData("NAME")]
	// macOS
	[InlineData("HOST")]
	public async ValueTask TestAssemblies(string computerEnvName)
	{
		var assembly1Starting = TestData.TestAssemblyStarting(assemblyUniqueID: "asm1", assemblyPath: "asm1.dll");
		var assembly2Starting = TestData.TestAssemblyStarting(assemblyUniqueID: "asm2", assemblyPath: "asm2.dll", startTime: TestData.DefaultStartTime.AddSeconds(1));
		var assembly1Finished = TestData.TestAssemblyFinished(
			assemblyUniqueID: "asm1",
			executionTime: 123.4567M,
			testsFailed: 42,
			testsNotRun: 3,
			testsSkipped: 6,
			testsTotal: 2112
		);
		var assembly2Finished = TestData.TestAssemblyFinished(
			assemblyUniqueID: "asm2",
			executionTime: 1.23M,
			testsFailed: 0,
			testsNotRun: 0,
			testsSkipped: 0,
			testsTotal: 5
		);
		await using var handler = TestableJUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assembly1Starting);
		handler.OnMessage(assembly2Starting);
		Environment.SetEnvironmentVariable(computerEnvName, "expected-computer");
		handler.OnMessage(assembly1Finished);
		Environment.SetEnvironmentVariable(computerEnvName, null);
		handler.OnMessage(assembly2Finished);

		var testSuitesElement = await handler.TestSuitesElement();
		Assert.Equal("2117", testSuitesElement.Attribute("tests")?.Value);
		Assert.Equal("42", testSuitesElement.Attribute("failures")?.Value);
		Assert.Equal("3", testSuitesElement.Attribute("disabled")?.Value);
		Assert.Equal("124.686700", testSuitesElement.Attribute("time")?.Value);
		Assert.Equal("2024-07-04T21:12:08", testSuitesElement.Attribute("timestamp")?.Value);

		Assert.Collection(
			testSuitesElement.Elements("testsuite").OrderBy(e => e.Attribute("name")?.Value),
			testSuiteElement => VerifyTestSuiteElement(testSuiteElement, assembly1Starting, assembly1Finished, "expected-computer"),
			testSuiteElement => VerifyTestSuiteElement(testSuiteElement, assembly2Starting, assembly2Finished, "localhost")
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

	[CulturedTheoryDefault(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(ErrorsData))]
	public async ValueTask TestAssembly_WithError(IMessageSinkMessage errorMessage)
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var assemblyFinished = TestData.TestAssemblyFinished();
		await using var handler = TestableJUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(errorMessage);
		handler.OnMessage(assemblyFinished);

		var testSuitesElement = await handler.TestSuitesElement();
		Assert.Equal("1", testSuitesElement.Attribute("errors")?.Value);
		var testSuiteElement = Assert.Single(testSuitesElement.Elements("testsuite"));
		Assert.Equal("1", testSuiteElement.Attribute("errors")?.Value);
		Assert.Equal("""
			One or more exceptions occurred during cleanup:

			System.DivideByZeroException : Attempted to divide by zero. Did you really think that was going to work?
			/path/file.cs(42,0): at SomeInnerCall()
			/path/otherFile.cs(2112,0): at SomeOuterMethod
			""", testSuiteElement.Element("system-err")?.Value, ignoreLineEndingDifferences: true);
	}

	[CulturedFactDefault]
	public async ValueTask TestPassed()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var testStarting = TestData.TestStarting();
		var testClassStarting = TestData.TestClassStarting();
		var testPassed = TestData.TestPassed(executionTime: 123.4567809m, output: "");
		var testClassFinished = TestData.TestClassFinished();
		var testFinished = TestData.TestFinished();
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableJUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testClassStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(testFinished);
		handler.OnMessage(testClassFinished);
		handler.OnMessage(assemblyFinished);

		var testSuitesElement = await handler.TestSuitesElement();
		var testSuiteElement = Assert.Single(testSuitesElement.Elements("testsuite"));
		VerifyTestSuiteElement(testSuiteElement, assemblyStarting, assemblyFinished);
		var testCaseElement = Assert.Single(testSuiteElement.Elements("testcase"));
		Assert.Equal("test-display-name", testCaseElement.Attribute("name")?.Value);
		Assert.Equal("123.456781", testCaseElement.Attribute("time")?.Value);
		Assert.Equal("test-class-name", testCaseElement.Attribute("classname")?.Value);

		Assert.Null(testCaseElement.Element("skipped"));

		Assert.Null(testCaseElement.Element("failure"));

		Assert.Null(testCaseElement.Element("system-out"));
	}

	[CulturedFactDefault]
	public async ValueTask TestFailed()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var testStarting = TestData.TestStarting();
		var testClassStarting = TestData.TestClassStarting();
		var testFailed = TestData.TestFailed(executionTime: 123.4567809m, output: "");
		var testClassFinished = TestData.TestClassFinished();
		var testFinished = TestData.TestFinished();
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableJUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testClassStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testFailed);
		handler.OnMessage(testFinished);
		handler.OnMessage(testClassFinished);
		handler.OnMessage(assemblyFinished);

		var testSuitesElement = await handler.TestSuitesElement();
		var testSuiteElement = Assert.Single(testSuitesElement.Elements("testsuite"));
		VerifyTestSuiteElement(testSuiteElement, assemblyStarting, assemblyFinished);
		var testCaseElement = Assert.Single(testSuiteElement.Elements("testcase"));
		Assert.Equal("test-display-name", testCaseElement.Attribute("name")?.Value);
		Assert.Equal("123.456781", testCaseElement.Attribute("time")?.Value);
		Assert.Equal("test-class-name", testCaseElement.Attribute("classname")?.Value);

		Assert.Null(testCaseElement.Element("skipped"));

		var failureElement = Assert.Single(testCaseElement.Elements("failure"));
		Assert.Equal("System.DivideByZeroException : Attempted to divide by zero. Did you really think that was going to work?", failureElement.Attribute("message")?.Value);
		Assert.Equal("""
			/path/file.cs(42,0): at SomeInnerCall()
			/path/otherFile.cs(2112,0): at SomeOuterMethod
			""", failureElement.Value, ignoreLineEndingDifferences: true);

		Assert.Null(testCaseElement.Element("system-out"));
	}

	[CulturedFactDefault]
	public async ValueTask TestFailed_NullStackTrace()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var testStarting = TestData.TestStarting();
		var testClassStarting = TestData.TestClassStarting();
		var testFailed = TestData.TestFailed(executionTime: 123.4567809m, output: "", stackTraces: [default]);
		var testClassFinished = TestData.TestClassFinished();
		var testFinished = TestData.TestFinished();
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableJUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testClassStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testFailed);
		handler.OnMessage(testFinished);
		handler.OnMessage(testClassFinished);
		handler.OnMessage(assemblyFinished);

		var testSuitesElement = await handler.TestSuitesElement();
		var testSuiteElement = Assert.Single(testSuitesElement.Elements("testsuite"));
		VerifyTestSuiteElement(testSuiteElement, assemblyStarting, assemblyFinished);
		var testCaseElement = Assert.Single(testSuiteElement.Elements("testcase"));
		Assert.Equal("test-display-name", testCaseElement.Attribute("name")?.Value);
		Assert.Equal("123.456781", testCaseElement.Attribute("time")?.Value);
		Assert.Equal("test-class-name", testCaseElement.Attribute("classname")?.Value);

		Assert.Null(testCaseElement.Element("skipped"));

		var failureElement = Assert.Single(testCaseElement.Elements("failure"));
		Assert.Equal("System.DivideByZeroException : Attempted to divide by zero. Did you really think that was going to work?", failureElement.Attribute("message")?.Value);

		Assert.Null(testCaseElement.Element("system-out"));
	}

	[CulturedFactDefault]
	public async ValueTask TestSkipped()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var testStarting = TestData.TestStarting();
		var testClassStarting = TestData.TestClassStarting();
		var testSkipped = TestData.TestSkipped(reason: "I don't want to run", output: "");
		var testClassFinished = TestData.TestClassFinished();
		var testFinished = TestData.TestFinished();
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 1, testsTotal: 1);
		await using var handler = TestableJUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testClassStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testSkipped);
		handler.OnMessage(testFinished);
		handler.OnMessage(testClassFinished);
		handler.OnMessage(assemblyFinished);

		var testSuitesElement = await handler.TestSuitesElement();
		var testSuiteElement = Assert.Single(testSuitesElement.Elements("testsuite"));
		VerifyTestSuiteElement(testSuiteElement, assemblyStarting, assemblyFinished);
		var testCaseElement = Assert.Single(testSuiteElement.Elements("testcase"));
		Assert.Equal("test-display-name", testCaseElement.Attribute("name")?.Value);
		Assert.Equal("0.000000", testCaseElement.Attribute("time")?.Value);
		Assert.Equal("test-class-name", testCaseElement.Attribute("classname")?.Value);

		Assert.Equal("I don't want to run", testCaseElement.Element("skipped")?.Value);

		Assert.Null(testCaseElement.Element("failure"));

		Assert.Null(testCaseElement.Element("system-out"));
	}

	[CulturedFactDefault]
	public async ValueTask TestNotRun()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var testStarting = TestData.TestStarting();
		var testClassStarting = TestData.TestClassStarting();
		var testNotRun = TestData.TestNotRun();
		var testClassFinished = TestData.TestClassFinished();
		var testFinished = TestData.TestFinished();
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 1, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableJUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testClassStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testNotRun);
		handler.OnMessage(testFinished);
		handler.OnMessage(testClassFinished);
		handler.OnMessage(assemblyFinished);

		var testSuitesElement = await handler.TestSuitesElement();
		var testSuiteElement = Assert.Single(testSuitesElement.Elements("testsuite"));
		VerifyTestSuiteElement(testSuiteElement, assemblyStarting, assemblyFinished);
		Assert.Null(testSuiteElement.Element("testcase"));
	}

	[Fact]
	public async ValueTask TestResult_WithOutput()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var testStarting = TestData.TestStarting();
		var testClassStarting = TestData.TestClassStarting();
		var testPassed = TestData.TestPassed(output: "This is the output text");
		var testClassFinished = TestData.TestClassFinished();
		var testFinished = TestData.TestFinished();
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableJUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testClassStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(testFinished);
		handler.OnMessage(testClassFinished);
		handler.OnMessage(assemblyFinished);

		var testSuitesElement = await handler.TestSuitesElement();
		var testCaseElement = Assert.Single(testSuitesElement.XPathSelectElements("testsuite/testcase"));
		Assert.Equal("""
			<<< Test output >>>

			This is the output text
			""", testCaseElement.Element("system-out")?.Value);
	}

	[Fact]
	public async ValueTask TestResult_WithWarnings()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var testStarting = TestData.TestStarting();
		var testClassStarting = TestData.TestClassStarting();
		var testPassed = TestData.TestPassed(output: "", warnings: ["warning 1", "warning 2\r\nmulti-line"]);
		var testClassFinished = TestData.TestClassFinished();
		var testFinished = TestData.TestFinished();
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableJUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testClassStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(testFinished);
		handler.OnMessage(testClassFinished);
		handler.OnMessage(assemblyFinished);

		var testSuitesElement = await handler.TestSuitesElement();
		var testCaseElement = Assert.Single(testSuitesElement.XPathSelectElements("testsuite/testcase"));
		Assert.Equal("""
			<<< Warnings >>>

			1. warning 1

			2. warning 2
			multi-line
			""", testCaseElement.Element("system-out")?.Value);
	}

	[Fact]
	public async ValueTask TestResult_WithOutputAndWarnings()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var testStarting = TestData.TestStarting();
		var testClassStarting = TestData.TestClassStarting();
		var testPassed = TestData.TestPassed(output: "This is the output text\r\n", warnings: ["warning 1", "warning 2\r\nmulti-line"]);
		var testClassFinished = TestData.TestClassFinished();
		var testFinished = TestData.TestFinished();
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableJUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testClassStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(testFinished);
		handler.OnMessage(testClassFinished);
		handler.OnMessage(assemblyFinished);

		var testSuitesElement = await handler.TestSuitesElement();
		var testCaseElement = Assert.Single(testSuitesElement.XPathSelectElements("testsuite/testcase"));
		Assert.Equal("""
			<<< Test output >>>

			This is the output text

			<<< Warnings >>>

			1. warning 1

			2. warning 2
			multi-line
			""", testCaseElement.Element("system-out")?.Value);
	}

	static void VerifyTestSuiteElement(
		XElement testSuiteElement,
		ITestAssemblyStarting starting,
		ITestAssemblyFinished finished,
		string expectedHostName = "localhost")
	{
		Assert.Equal(starting.AssemblyPath, testSuiteElement.Attribute("name")?.Value);
		Assert.Equal(finished.TestsTotal.ToString(CultureInfo.InvariantCulture), testSuiteElement.Attribute("tests")?.Value);
		Assert.Equal(finished.TestsFailed.ToString(CultureInfo.InvariantCulture), testSuiteElement.Attribute("failures")?.Value);
		Assert.Equal("0", testSuiteElement.Attribute("errors")?.Value);
		Assert.Equal(finished.TestsNotRun.ToString(CultureInfo.InvariantCulture), testSuiteElement.Attribute("disabled")?.Value);
		Assert.Equal(finished.TestsSkipped.ToString(CultureInfo.InvariantCulture), testSuiteElement.Attribute("skipped")?.Value);
		Assert.Equal(finished.ExecutionTime.ToString("0.000000", CultureInfo.InvariantCulture), testSuiteElement.Attribute("time")?.Value);
		Assert.Equal(starting.StartTime.ToString("s", CultureInfo.InvariantCulture), testSuiteElement.Attribute("timestamp")?.Value);
		Assert.Equal(expectedHostName, testSuiteElement.Attribute("hostname")?.Value);
	}

	class TestableJUnitResultWriterMessageHandler(
		StringWriter stringWriter,
		XmlWriter xmlWriter) :
			JUnitResultWriterMessageHandler(xmlWriter)
	{
		public async ValueTask<XElement> TestSuitesElement()
		{
			await DisposeAsync();

			var document = XDocument.Parse(stringWriter.ToString());
			var assemblyElement = document.Element("testsuites");
			Assert.NotNull(assemblyElement);

			return assemblyElement;
		}

		public static TestableJUnitResultWriterMessageHandler Create()
		{
			var stringWriter = new StringWriter();
			var textWriter = new XmlTextWriter(stringWriter);

			return new(stringWriter, textWriter);
		}
	}
}
