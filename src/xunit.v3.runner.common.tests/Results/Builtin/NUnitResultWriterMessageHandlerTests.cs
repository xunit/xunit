using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

[CleanEnvironment("COMPUTERNAME", "HOSTNAME", "NAME", "HOST", "USERNAME", "LOGNAME", "USER", "USERDOMAIN")]
public class NUnitResultWriterMessageHandlerTests
{
	[CulturedFactDefault]
	public async ValueTask TestRunElement()
	{
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		var testRunElement = await handler.TestRunElement();
		Assert.Equal("0", testRunElement.Attribute("id")?.Value);
		Assert.Equal("Runnable", testRunElement.Attribute("runstate")?.Value);
		Assert.Equal("0", testRunElement.Attribute("testcasecount")?.Value);
		Assert.Equal("Passed", testRunElement.Attribute("result")?.Value);
		Assert.Equal("0", testRunElement.Attribute("total")?.Value);
		Assert.Equal("0", testRunElement.Attribute("passed")?.Value);
		Assert.Equal("0", testRunElement.Attribute("failed")?.Value);
		Assert.Equal("0", testRunElement.Attribute("warnings")?.Value);
		Assert.Equal("0", testRunElement.Attribute("inconclusive")?.Value);
		Assert.Equal("0", testRunElement.Attribute("skipped")?.Value);
		Assert.Equal("0", testRunElement.Attribute("asserts")?.Value);
		Assert.Equal(ThisAssembly.AssemblyFileVersion, testRunElement.Attribute("engine-version")?.Value);
		Assert.Equal("4.0.30319", testRunElement.Attribute("clr-version")?.Value);
		Assert.Equal("0001-01-01 00:00:00Z", testRunElement.Attribute("start-time")?.Value);
		Assert.Equal("0001-01-01 00:00:00Z", testRunElement.Attribute("end-time")?.Value);
		Assert.Equal("0.000000", testRunElement.Attribute("duration")?.Value);

		var commandLineElement = Assert.Single(testRunElement.Elements("command-line"));
		Assert.Equal(string.Empty, commandLineElement.Value);

		Assert.Empty(testRunElement.Elements("test-suite"));
	}

	[CulturedTheoryDefault]
	// Windows
	[InlineData("COMPUTERNAME", "USERNAME", "USERDOMAIN")]
	// Linux
	[InlineData("HOSTNAME", "LOGNAME", null)]
	[InlineData("HOSTNAME", "USER", null)]
	[InlineData("NAME", "LOGNAME", null)]
	[InlineData("NAME", "USER", null)]
	// macOS
	[InlineData("HOST", "LOGNAME", null)]
	[InlineData("HOST", "USER", null)]
	public async ValueTask TestAssemblies(
		string computerEnvName,
		string userEnvName,
		string? domainEnvName)
	{
		Environment.SetEnvironmentVariable(computerEnvName, "expected-computer");
		Environment.SetEnvironmentVariable(userEnvName, "expected-user");
		var expectedDomain = domainEnvName is not null ? "expected-domain" : "expected-computer";
		if (domainEnvName is not null)
			Environment.SetEnvironmentVariable(domainEnvName, expectedDomain);

		var assembly1Starting = TestData.TestAssemblyStarting(
			assemblyUniqueID: "asm1",
			assemblyPath: "/path/to/assembly.dll",
			targetFramework: ".NETFramework,Version=v4.7.2"
		);
		var assembly2Starting = TestData.TestAssemblyStarting(
			assemblyUniqueID: "asm2",
			assemblyPath: "/path/to/assembly.dll",
			startTime: TestData.DefaultStartTime.AddSeconds(1),
			targetFramework: ".NETCoreApp,Version=v8.0"
		);
		var assembly1Finished = TestData.TestAssemblyFinished(
			assemblyUniqueID: "asm1",
			executionTime: 123.4567M,
			finishTime: TestData.DefaultFinishTime.AddSeconds(-1),
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
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assembly1Starting);
		handler.OnMessage(assembly2Starting);
		handler.OnMessage(assembly1Finished);
		handler.OnMessage(assembly2Finished);

		var testRunElement = await handler.TestRunElement();
		Assert.Equal("0", testRunElement.Attribute("id")?.Value);
		Assert.Equal("Runnable", testRunElement.Attribute("runstate")?.Value);
		Assert.Equal("2117", testRunElement.Attribute("testcasecount")?.Value);
		Assert.Equal("Failed", testRunElement.Attribute("result")?.Value);
		Assert.Equal("2117", testRunElement.Attribute("total")?.Value);
		Assert.Equal("2066", testRunElement.Attribute("passed")?.Value);
		Assert.Equal("42", testRunElement.Attribute("failed")?.Value);
		Assert.Equal("9", testRunElement.Attribute("skipped")?.Value);
		Assert.Equal("2024-07-04 21:12:08Z", testRunElement.Attribute("start-time")?.Value);
		Assert.Equal("2024-07-04 21:12:28Z", testRunElement.Attribute("end-time")?.Value);
		Assert.Equal("124.686700", testRunElement.Attribute("duration")?.Value);

		Assert.Collection(
			testRunElement.Elements("test-suite").OrderBy(e => e.Attribute("id")?.Value),
			assemblyElement => verifyAssemblyTestSuiteElement(assemblyElement, "1-1000", "net-4.7.2", assembly1Starting, assembly1Finished, expectedEndTime: "2024-07-04T21:12:27.0000000Z"),
			assemblyElement => verifyAssemblyTestSuiteElement(assemblyElement, "2-1000", "netcore-8.0", assembly2Starting, assembly2Finished, expectedStartTime: "2024-07-04T21:12:09.0000000Z")
		);

		void verifyAssemblyTestSuiteElement(
			XElement testSuiteElement,
			string id,
			string targetRuntimeFramework,
			ITestAssemblyStarting starting,
			ITestAssemblyFinished finished,
			string expectedStartTime = "2024-07-04T21:12:08.0000000Z",
			string expectedEndTime = "2024-07-04T21:12:28.0000000Z")
		{
			VerifyTestSuite(testSuiteElement, "Assembly", id, finished, Path.GetFileName(starting.AssemblyPath), starting.AssemblyPath, 0, 0, null, expectedStartTime, expectedEndTime);

			var environmentElement = Assert.Single(testSuiteElement.Elements("environment"));
			Assert.Equal(ThisAssembly.AssemblyFileVersion, environmentElement.Attribute("framework-version")?.Value);
			Assert.Equal(targetRuntimeFramework.Substring(targetRuntimeFramework.IndexOf('-') + 1), environmentElement.Attribute("clr-version")?.Value);
			Assert.Equal(RuntimeInformation.OSDescription.Trim(), environmentElement.Attribute("os-version")?.Value);
			Assert.Equal(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Win32NT" : "Unix", environmentElement.Attribute("platform")?.Value);
			Assert.Equal(Path.GetDirectoryName(starting.AssemblyPath), environmentElement.Attribute("cwd")?.Value);
			Assert.Equal("expected-computer", environmentElement.Attribute("machine-name")?.Value);
			Assert.Equal("expected-user", environmentElement.Attribute("user")?.Value);
			Assert.Equal(expectedDomain, environmentElement.Attribute("user-domain")?.Value);
			Assert.Equal("", environmentElement.Attribute("culture")?.Value);
			Assert.Equal("", environmentElement.Attribute("uiculture")?.Value);
			Assert.Equal(RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(), environmentElement.Attribute("os-architecture")?.Value);

			var settingsElement = Assert.Single(testSuiteElement.Elements("settings"));
			Assert.Equal(Path.GetDirectoryName(starting.AssemblyPath), settingsElement.XPathSelectElement("setting[@name='WorkDirectory']")?.Attribute("value")?.Value);
			Assert.Equal(starting.TargetFramework, settingsElement.XPathSelectElement("setting[@name='ImageTargetFrameworkName']")?.Attribute("value")?.Value);
			Assert.Equal(targetRuntimeFramework, settingsElement.XPathSelectElement("setting[@name='TargetRuntimeFramework']")?.Attribute("value")?.Value);
		}
	}

	[CulturedTheoryDefault]
	[InlineData("Class Name", 0)]
	[InlineData(null, 42)]
	public async ValueTask TestCollections(
		string? testCollectionClass,
		int testsFailed)
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var assemblyFinished = TestData.TestAssemblyFinished();
		var testCollectionStarted = TestData.TestCollectionStarting(
			testCollectionDisplayName: "Collection Name",
			testCollectionClass: testCollectionClass,
			testCollectionUniqueID: "abc123"
		);
		var testCollectionFinished = TestData.TestCollectionFinished(
			executionTime: 123.4567m,
			testCollectionUniqueID: "abc123",
			testsFailed: testsFailed,
			testsNotRun: 3,
			testsSkipped: 6,
			testsTotal: 2112
		);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testCollectionStarted);
		handler.OnMessage(testCollectionFinished);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var testSuiteElement = Assert.Single(testRunElement.XPathSelectElements("test-suite/test-suite"));
		VerifyTestSuite(testSuiteElement, "TestSuite", "1-1000", testCollectionFinished, "Collection Name", testCollectionClass);
	}

	[CulturedTheoryDefault]
	[InlineData(0)]
	[InlineData(42)]
	public async ValueTask TestClasses(int testsFailed)
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var assemblyFinished = TestData.TestAssemblyFinished();
		var testCollectionStarted = TestData.TestCollectionStarting();
		var testCollectionFinished = TestData.TestCollectionFinished();
		var testClassStarted = TestData.TestClassStarting(
			testClassUniqueID: "test-class-id",
			testClassName: "TestNamespace.TestClass",
			testClassSimpleName: "TestClass"
		);
		var testClassFinished = TestData.TestClassFinished(
			executionTime: 123.4567m,
			testClassUniqueID: "test-class-id",
			testsFailed: testsFailed,
			testsNotRun: 3,
			testsSkipped: 6,
			testsTotal: 2112
		);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testCollectionStarted);
		handler.OnMessage(testClassStarted);
		handler.OnMessage(testClassFinished);
		handler.OnMessage(testCollectionFinished);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var testFixtureElement = Assert.Single(testRunElement.XPathSelectElements("test-suite/test-suite/test-suite"));
		VerifyTestSuite(testFixtureElement, "TestFixture", "1-1001", testClassFinished, "TestClass", "TestNamespace.TestClass");
	}

	[CulturedFactDefault]
	public async ValueTask TestPassed()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting();
		var methodStarting = TestData.TestMethodStarting();
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(traits: TestData.EmptyTraits);
		var testPassed = TestData.TestPassed(executionTime: 123.4567809m, output: "");
		var testFinished = TestData.TestFinished();
		var caseFinished = TestData.TestCaseFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var methodFinished = TestData.TestMethodFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var classFinished = TestData.TestClassFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var collectionFinished = TestData.TestCollectionFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(testFinished);
		handler.OnMessage(caseFinished);
		handler.OnMessage(methodFinished);
		handler.OnMessage(classFinished);
		handler.OnMessage(collectionFinished);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var assemblyElement = Assert.Single(testRunElement.Elements("test-suite"));
		VerifyTestSuite(assemblyElement, "Assembly", "1-1003", assemblyFinished, "test-assembly.dll", "./test-assembly.dll");
		var testSuiteElement = Assert.Single(assemblyElement.Elements("test-suite"));
		VerifyTestSuite(testSuiteElement, "TestSuite", "1-1000", collectionFinished, "test-collection-display-name", "test-collection-class");
		var testFixtureElement = Assert.Single(testSuiteElement.Elements("test-suite"));
		VerifyTestSuite(testFixtureElement, "TestFixture", "1-1001", classFinished, "test-class-simple-name", "test-class-name");
		var testCaseElement = Assert.Single(testFixtureElement.Elements("test-case"));
		Assert.Equal("1-1002", testCaseElement.Attribute("id")?.Value);
		Assert.Equal("test-method", testCaseElement.Attribute("name")?.Value);
		Assert.Equal("test-display-name", testCaseElement.Attribute("fullname")?.Value);
		Assert.Equal("test-method", testCaseElement.Attribute("methodname")?.Value);
		Assert.Equal("test-class-name", testCaseElement.Attribute("classname")?.Value);
		Assert.Equal("Runnable", testCaseElement.Attribute("runstate")?.Value);
		Assert.Equal("Passed", testCaseElement.Attribute("result")?.Value);
		Assert.Null(testCaseElement.Attribute("label")?.Value);
		Assert.Equal("2024-07-04T21:12:08.0000000Z", testCaseElement.Attribute("start-time")?.Value);
		Assert.Equal("2024-07-04T21:12:28.0000000Z", testCaseElement.Attribute("end-time")?.Value);
		Assert.Equal("123.456781", testCaseElement.Attribute("duration")?.Value);
		Assert.Equal("0", testCaseElement.Attribute("asserts")?.Value);

		Assert.Null(testCaseElement.Element("failure"));

		Assert.Null(testCaseElement.Element("reason"));

		Assert.Null(testCaseElement.Element("output"));

		Assert.Null(testCaseElement.Element("assertions"));

		Assert.Null(testCaseElement.Element("properties"));

		Assert.Null(testCaseElement.Element("attachments"));
	}

	[CulturedFactDefault]
	public async ValueTask TestPassed_WithOneWarning()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting();
		var methodStarting = TestData.TestMethodStarting();
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(traits: TestData.EmptyTraits);
		var testPassed = TestData.TestPassed(executionTime: 123.4567809m, output: "", warnings: ["This is a warning"]);
		var testFinished = TestData.TestFinished();
		var caseFinished = TestData.TestCaseFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var methodFinished = TestData.TestMethodFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var classFinished = TestData.TestClassFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var collectionFinished = TestData.TestCollectionFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(testFinished);
		handler.OnMessage(caseFinished);
		handler.OnMessage(methodFinished);
		handler.OnMessage(classFinished);
		handler.OnMessage(collectionFinished);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var assemblyElement = Assert.Single(testRunElement.Elements("test-suite"));
		VerifyTestSuite(assemblyElement, "Assembly", "1-1003", assemblyFinished, "test-assembly.dll", "./test-assembly.dll", warnings: 1);
		var testSuiteElement = Assert.Single(assemblyElement.Elements("test-suite"));
		VerifyTestSuite(testSuiteElement, "TestSuite", "1-1000", collectionFinished, "test-collection-display-name", "test-collection-class", warnings: 1);
		var testFixtureElement = Assert.Single(testSuiteElement.Elements("test-suite"));
		VerifyTestSuite(testFixtureElement, "TestFixture", "1-1001", classFinished, "test-class-simple-name", "test-class-name", warnings: 1);
		var testCaseElement = Assert.Single(testFixtureElement.Elements("test-case"));
		Assert.Equal("Runnable", testCaseElement.Attribute("runstate")?.Value);
		Assert.Equal("Warning", testCaseElement.Attribute("result")?.Value);
		Assert.Null(testCaseElement.Attribute("label")?.Value);

		var reasonElement = Assert.Single(testCaseElement.Elements("reason"));
		Assert.Equal("This is a warning", reasonElement.Element("message")?.Value);
		Assert.Equal(string.Empty, reasonElement.Element("stack-trace")?.Value);

		var assertionsElement = Assert.Single(testCaseElement.Elements("assertions"));
		var assertionElement = Assert.Single(assertionsElement.Elements("assertion"));
		Assert.Equal("Warning", assertionElement.Attribute("result")?.Value);
		Assert.Equal("This is a warning", assertionElement.Element("message")?.Value);
		Assert.Equal(string.Empty, assertionElement.Element("stack-trace")?.Value);
	}

	[CulturedFactDefault]
	public async ValueTask TestPassed_WithMultipleWarnings()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting();
		var methodStarting = TestData.TestMethodStarting();
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(traits: TestData.EmptyTraits);
		var testPassed = TestData.TestPassed(executionTime: 123.4567809m, output: "", warnings: ["This is a warning", "This is a second warning"]);
		var testFinished = TestData.TestFinished();
		var caseFinished = TestData.TestCaseFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var methodFinished = TestData.TestMethodFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var classFinished = TestData.TestClassFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var collectionFinished = TestData.TestCollectionFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(testFinished);
		handler.OnMessage(caseFinished);
		handler.OnMessage(methodFinished);
		handler.OnMessage(classFinished);
		handler.OnMessage(collectionFinished);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var assemblyElement = Assert.Single(testRunElement.Elements("test-suite"));
		VerifyTestSuite(assemblyElement, "Assembly", "1-1003", assemblyFinished, "test-assembly.dll", "./test-assembly.dll", warnings: 1);
		var testSuiteElement = Assert.Single(assemblyElement.Elements("test-suite"));
		VerifyTestSuite(testSuiteElement, "TestSuite", "1-1000", collectionFinished, "test-collection-display-name", "test-collection-class", warnings: 1);
		var testFixtureElement = Assert.Single(testSuiteElement.Elements("test-suite"));
		VerifyTestSuite(testFixtureElement, "TestFixture", "1-1001", classFinished, "test-class-simple-name", "test-class-name", warnings: 1);
		var testCaseElement = Assert.Single(testFixtureElement.Elements("test-case"));
		Assert.Equal("Runnable", testCaseElement.Attribute("runstate")?.Value);
		Assert.Equal("Warning", testCaseElement.Attribute("result")?.Value);
		Assert.Null(testCaseElement.Attribute("label")?.Value);

		Assert.Null(testCaseElement.Element("failure"));

		var reasonElement = Assert.Single(testCaseElement.Elements("reason"));
		Assert.Equal("""
			Multiple failures or warnings in test:

			  1) This is a warning

			  2) This is a second warning
			""", reasonElement.Element("message")?.Value, ignoreLineEndingDifferences: true);
		Assert.Equal(string.Empty, reasonElement.Element("stack-trace")?.Value);

		var assertionsElement = Assert.Single(testCaseElement.Elements("assertions"));
		Assert.Collection(
			assertionsElement.Elements("assertion"),
			assertionElement =>
			{
				Assert.Equal("Warning", assertionElement.Attribute("result")?.Value);
				Assert.Equal("This is a warning", assertionElement.Element("message")?.Value);
				Assert.Equal(string.Empty, assertionElement.Element("stack-trace")?.Value);
			},
			assertionElement =>
			{
				Assert.Equal("Warning", assertionElement.Attribute("result")?.Value);
				Assert.Equal("This is a second warning", assertionElement.Element("message")?.Value);
				Assert.Equal(string.Empty, assertionElement.Element("stack-trace")?.Value);
			}
		);
	}

	[CulturedFactDefault]
	public async ValueTask TestFailed()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting();
		var methodStarting = TestData.TestMethodStarting();
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(traits: TestData.EmptyTraits);
		var testFailed = TestData.TestFailed(
			exceptionParentIndices: [-1],
			exceptionTypes: ["Exception Type"],
			executionTime: 123.4567809m,
			messages: ["Exception Message"],
			output: "test output",
			stackTraces: ["Exception Stack Trace"]
		);
		var testFinished = TestData.TestFinished();
		var caseFinished = TestData.TestCaseFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var methodFinished = TestData.TestMethodFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var classFinished = TestData.TestClassFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var collectionFinished = TestData.TestCollectionFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testFailed);
		handler.OnMessage(testFinished);
		handler.OnMessage(caseFinished);
		handler.OnMessage(methodFinished);
		handler.OnMessage(classFinished);
		handler.OnMessage(collectionFinished);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var assemblyElement = Assert.Single(testRunElement.Elements("test-suite"));
		VerifyTestSuite(assemblyElement, "Assembly", "1-1003", assemblyFinished, "test-assembly.dll", "./test-assembly.dll");
		var testSuiteElement = Assert.Single(assemblyElement.Elements("test-suite"));
		VerifyTestSuite(testSuiteElement, "TestSuite", "1-1000", collectionFinished, "test-collection-display-name", "test-collection-class");
		var testFixtureElement = Assert.Single(testSuiteElement.Elements("test-suite"));
		VerifyTestSuite(testFixtureElement, "TestFixture", "1-1001", classFinished, "test-class-simple-name", "test-class-name");
		var testCaseElement = Assert.Single(testFixtureElement.Elements("test-case"));
		Assert.Equal("Runnable", testCaseElement.Attribute("runstate")?.Value);
		Assert.Equal("Failed", testCaseElement.Attribute("result")?.Value);
		Assert.Null(testCaseElement.Attribute("label")?.Value);

		var failureElement = Assert.Single(testCaseElement.Elements("failure"));
		Assert.Equal("Exception Type : Exception Message", failureElement.Element("message")?.Value);
		Assert.Equal("Exception Stack Trace", failureElement.Element("stack-trace")?.Value);

		Assert.Equal("test output", testCaseElement.Element("output")?.Value);

		var assertionsElement = Assert.Single(testCaseElement.Elements("assertions"));
		var assertionElement = Assert.Single(assertionsElement.Elements("assertion"));
		Assert.Equal("Failed", assertionElement.Attribute("result")?.Value);
		Assert.Equal("Exception Type : Exception Message", assertionElement.Element("message")?.Value);
		Assert.Equal("Exception Stack Trace", assertionElement.Element("stack-trace")?.Value);
	}

	[CulturedFactDefault]
	public async ValueTask TestFailed_NullStackTrace()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting();
		var methodStarting = TestData.TestMethodStarting();
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(traits: TestData.EmptyTraits);
		var testFailed = TestData.TestFailed(
			exceptionParentIndices: [-1],
			exceptionTypes: ["Exception Type"],
			executionTime: 123.4567809m,
			messages: ["Exception Message"],
			output: "",
			stackTraces: [default]
		);
		var testFinished = TestData.TestFinished();
		var caseFinished = TestData.TestCaseFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var methodFinished = TestData.TestMethodFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var classFinished = TestData.TestClassFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var collectionFinished = TestData.TestCollectionFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testFailed);
		handler.OnMessage(testFinished);
		handler.OnMessage(caseFinished);
		handler.OnMessage(methodFinished);
		handler.OnMessage(classFinished);
		handler.OnMessage(collectionFinished);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var assemblyElement = Assert.Single(testRunElement.Elements("test-suite"));
		VerifyTestSuite(assemblyElement, "Assembly", "1-1003", assemblyFinished, "test-assembly.dll", "./test-assembly.dll");
		var testSuiteElement = Assert.Single(assemblyElement.Elements("test-suite"));
		VerifyTestSuite(testSuiteElement, "TestSuite", "1-1000", collectionFinished, "test-collection-display-name", "test-collection-class");
		var testFixtureElement = Assert.Single(testSuiteElement.Elements("test-suite"));
		VerifyTestSuite(testFixtureElement, "TestFixture", "1-1001", classFinished, "test-class-simple-name", "test-class-name");
		var testCaseElement = Assert.Single(testFixtureElement.Elements("test-case"));
		Assert.Equal("Runnable", testCaseElement.Attribute("runstate")?.Value);
		Assert.Equal("Failed", testCaseElement.Attribute("result")?.Value);
		Assert.Null(testCaseElement.Attribute("label")?.Value);

		var failureElement = Assert.Single(testCaseElement.Elements("failure"));
		Assert.Equal("Exception Type : Exception Message", failureElement.Element("message")?.Value);
		Assert.Equal(string.Empty, failureElement.Element("stack-trace")?.Value);

		var assertionsElement = Assert.Single(testCaseElement.Elements("assertions"));
		var assertionElement = Assert.Single(assertionsElement.Elements("assertion"));
		Assert.Equal("Failed", assertionElement.Attribute("result")?.Value);
		Assert.Equal("Exception Type : Exception Message", assertionElement.Element("message")?.Value);
		Assert.Equal(string.Empty, assertionElement.Element("stack-trace")?.Value);
	}

	[CulturedFactDefault]
	public async ValueTask TestFailed_WithWarning()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting();
		var methodStarting = TestData.TestMethodStarting();
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(traits: TestData.EmptyTraits);
		var testFailed = TestData.TestFailed(
			exceptionParentIndices: [-1],
			exceptionTypes: ["Exception Type"],
			executionTime: 123.4567809m,
			messages: ["Exception Message"],
			stackTraces: ["Exception Stack Trace"],
			output: "",
			warnings: ["This is a warning"]
		);
		var testFinished = TestData.TestFinished();
		var caseFinished = TestData.TestCaseFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var methodFinished = TestData.TestMethodFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var classFinished = TestData.TestClassFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var collectionFinished = TestData.TestCollectionFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testFailed);
		handler.OnMessage(testFinished);
		handler.OnMessage(caseFinished);
		handler.OnMessage(methodFinished);
		handler.OnMessage(classFinished);
		handler.OnMessage(collectionFinished);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var assemblyElement = Assert.Single(testRunElement.Elements("test-suite"));
		VerifyTestSuite(assemblyElement, "Assembly", "1-1003", assemblyFinished, "test-assembly.dll", "./test-assembly.dll");
		var testSuiteElement = Assert.Single(assemblyElement.Elements("test-suite"));
		VerifyTestSuite(testSuiteElement, "TestSuite", "1-1000", collectionFinished, "test-collection-display-name", "test-collection-class");
		var testFixtureElement = Assert.Single(testSuiteElement.Elements("test-suite"));
		VerifyTestSuite(testFixtureElement, "TestFixture", "1-1001", classFinished, "test-class-simple-name", "test-class-name");
		var testCaseElement = Assert.Single(testFixtureElement.Elements("test-case"));
		Assert.Equal("Runnable", testCaseElement.Attribute("runstate")?.Value);
		Assert.Equal("Failed", testCaseElement.Attribute("result")?.Value);
		Assert.Null(testCaseElement.Attribute("label")?.Value);

		var failureElement = Assert.Single(testCaseElement.Elements("failure"));
		Assert.Equal("""
			Multiple failures or warnings in test:

			  1) This is a warning

			  2) Exception Type : Exception Message
			""", failureElement.Element("message")?.Value, ignoreLineEndingDifferences: true);
		Assert.Equal(string.Empty, failureElement.Element("stack-trace")?.Value);

		var assertionsElement = Assert.Single(testCaseElement.Elements("assertions"));
		Assert.Collection(
			assertionsElement.Elements("assertion"),
			assertionElement =>
			{
				Assert.Equal("Warning", assertionElement.Attribute("result")?.Value);
				Assert.Equal("This is a warning", assertionElement.Element("message")?.Value);
				Assert.Equal(string.Empty, assertionElement.Element("stack-trace")?.Value);
			},
			assertionElement =>
			{
				Assert.Equal("Failed", assertionElement.Attribute("result")?.Value);
				Assert.Equal("Exception Type : Exception Message", assertionElement.Element("message")?.Value);
				Assert.Equal("Exception Stack Trace", assertionElement.Element("stack-trace")?.Value);
			}
		);
	}

	[CulturedFactDefault]
	public async ValueTask TestSkipped()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting();
		var methodStarting = TestData.TestMethodStarting();
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(traits: TestData.EmptyTraits);
		var testSkipped = TestData.TestSkipped(reason: "I don't want to run", output: "");
		var testFinished = TestData.TestFinished();
		var caseFinished = TestData.TestCaseFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var methodFinished = TestData.TestMethodFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var classFinished = TestData.TestClassFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var collectionFinished = TestData.TestCollectionFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testSkipped);
		handler.OnMessage(testFinished);
		handler.OnMessage(caseFinished);
		handler.OnMessage(methodFinished);
		handler.OnMessage(classFinished);
		handler.OnMessage(collectionFinished);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var assemblyElement = Assert.Single(testRunElement.Elements("test-suite"));
		VerifyTestSuite(assemblyElement, "Assembly", "1-1003", assemblyFinished, "test-assembly.dll", "./test-assembly.dll");
		var testSuiteElement = Assert.Single(assemblyElement.Elements("test-suite"));
		VerifyTestSuite(testSuiteElement, "TestSuite", "1-1000", collectionFinished, "test-collection-display-name", "test-collection-class");
		var testFixtureElement = Assert.Single(testSuiteElement.Elements("test-suite"));
		VerifyTestSuite(testFixtureElement, "TestFixture", "1-1001", classFinished, "test-class-simple-name", "test-class-name");
		var testCaseElement = Assert.Single(testFixtureElement.Elements("test-case"));
		Assert.Equal("Ignored", testCaseElement.Attribute("runstate")?.Value);
		Assert.Equal("Skipped", testCaseElement.Attribute("result")?.Value);
		Assert.Equal("Ignored", testCaseElement.Attribute("label")?.Value);
		Assert.Equal("2024-07-04T21:12:08.0000000Z", testCaseElement.Attribute("start-time")?.Value);
		Assert.Equal("2024-07-04T21:12:28.0000000Z", testCaseElement.Attribute("end-time")?.Value);
		Assert.Equal("0.000000", testCaseElement.Attribute("duration")?.Value);
		Assert.Equal("0", testCaseElement.Attribute("asserts")?.Value);

		var reasonElement = Assert.Single(testCaseElement.Elements("reason"));
		Assert.Equal("I don't want to run", reasonElement.Element("message")?.Value);

		var propertiesElement = Assert.Single(testCaseElement.Elements("properties"));
		var propertyElement = Assert.Single(propertiesElement.Elements("property"));
		Assert.Equal("_SKIPREASON", propertyElement.Attribute("name")?.Value);
		Assert.Equal("I don't want to run", propertyElement.Attribute("value")?.Value);
	}

	[CulturedFactDefault]
	public async ValueTask TestSkipped_WithTraits()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting();
		var methodStarting = TestData.TestMethodStarting();
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(traits: TestData.DefaultTraits);
		var testSkipped = TestData.TestSkipped(reason: "I don't want to run", output: "");
		var testFinished = TestData.TestFinished();
		var caseFinished = TestData.TestCaseFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var methodFinished = TestData.TestMethodFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var classFinished = TestData.TestClassFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var collectionFinished = TestData.TestCollectionFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testSkipped);
		handler.OnMessage(testFinished);
		handler.OnMessage(caseFinished);
		handler.OnMessage(methodFinished);
		handler.OnMessage(classFinished);
		handler.OnMessage(collectionFinished);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var assemblyElement = Assert.Single(testRunElement.Elements("test-suite"));
		VerifyTestSuite(assemblyElement, "Assembly", "1-1003", assemblyFinished, "test-assembly.dll", "./test-assembly.dll");
		var testSuiteElement = Assert.Single(assemblyElement.Elements("test-suite"));
		VerifyTestSuite(testSuiteElement, "TestSuite", "1-1000", collectionFinished, "test-collection-display-name", "test-collection-class");
		var testFixtureElement = Assert.Single(testSuiteElement.Elements("test-suite"));
		VerifyTestSuite(testFixtureElement, "TestFixture", "1-1001", classFinished, "test-class-simple-name", "test-class-name");
		var testCaseElement = Assert.Single(testFixtureElement.Elements("test-case"));
		Assert.Equal("Ignored", testCaseElement.Attribute("runstate")?.Value);
		Assert.Equal("Skipped", testCaseElement.Attribute("result")?.Value);
		Assert.Equal("Ignored", testCaseElement.Attribute("label")?.Value);
		Assert.Equal("2024-07-04T21:12:08.0000000Z", testCaseElement.Attribute("start-time")?.Value);
		Assert.Equal("2024-07-04T21:12:28.0000000Z", testCaseElement.Attribute("end-time")?.Value);
		Assert.Equal("0.000000", testCaseElement.Attribute("duration")?.Value);
		Assert.Equal("0", testCaseElement.Attribute("asserts")?.Value);

		var reasonElement = Assert.Single(testCaseElement.Elements("reason"));
		Assert.Equal("I don't want to run", reasonElement.Element("message")?.Value);

		var propertiesElement = Assert.Single(testCaseElement.Elements("properties"));
		Assert.Collection(
			propertiesElement.Elements("property").OrderBy(e => e.Attribute("name")?.Value).ThenBy(e => e.Attribute("value")?.Value),
			propertyElement =>
			{
				Assert.Equal("_SKIPREASON", propertyElement.Attribute("name")?.Value);
				Assert.Equal("I don't want to run", propertyElement.Attribute("value")?.Value);
			},
			propertyElement =>
			{
				Assert.Equal("biff", propertyElement.Attribute("name")?.Value);
				Assert.Equal("bang", propertyElement.Attribute("value")?.Value);
			},
			propertyElement =>
			{
				Assert.Equal("foo", propertyElement.Attribute("name")?.Value);
				Assert.Equal("bar", propertyElement.Attribute("value")?.Value);
			},
			propertyElement =>
			{
				Assert.Equal("foo", propertyElement.Attribute("name")?.Value);
				Assert.Equal("baz", propertyElement.Attribute("value")?.Value);
			}
		);
	}

	[CulturedFactDefault]
	public async ValueTask TestNotRun()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting();
		var methodStarting = TestData.TestMethodStarting();
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(traits: TestData.EmptyTraits);
		var testNotRun = TestData.TestNotRun(output: "");
		var testFinished = TestData.TestFinished();
		var caseFinished = TestData.TestCaseFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var methodFinished = TestData.TestMethodFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var classFinished = TestData.TestClassFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var collectionFinished = TestData.TestCollectionFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testNotRun);
		handler.OnMessage(testFinished);
		handler.OnMessage(caseFinished);
		handler.OnMessage(methodFinished);
		handler.OnMessage(classFinished);
		handler.OnMessage(collectionFinished);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var assemblyElement = Assert.Single(testRunElement.Elements("test-suite"));
		VerifyTestSuite(assemblyElement, "Assembly", "1-1003", assemblyFinished, "test-assembly.dll", "./test-assembly.dll");
		var testSuiteElement = Assert.Single(assemblyElement.Elements("test-suite"));
		VerifyTestSuite(testSuiteElement, "TestSuite", "1-1000", collectionFinished, "test-collection-display-name", "test-collection-class");
		var testFixtureElement = Assert.Single(testSuiteElement.Elements("test-suite"));
		VerifyTestSuite(testFixtureElement, "TestFixture", "1-1001", classFinished, "test-class-simple-name", "test-class-name");
		var testCaseElement = Assert.Single(testFixtureElement.Elements("test-case"));
		Assert.Equal("Explicit", testCaseElement.Attribute("runstate")?.Value);
		Assert.Equal("Skipped", testCaseElement.Attribute("result")?.Value);
		Assert.Equal("Explicit", testCaseElement.Attribute("label")?.Value);
		Assert.Equal("2024-07-04T21:12:08.0000000Z", testCaseElement.Attribute("start-time")?.Value);
		Assert.Equal("2024-07-04T21:12:28.0000000Z", testCaseElement.Attribute("end-time")?.Value);
		Assert.Equal("0.000000", testCaseElement.Attribute("duration")?.Value);
		Assert.Equal("0", testCaseElement.Attribute("asserts")?.Value);
	}

	[CulturedFactDefault]
	public async ValueTask TestResult_WithAttachments()
	{
		var attachments = new Dictionary<string, TestAttachment>
		{
			{ "hello", TestAttachment.Create("world") },
			{ "bytes", TestAttachment.Create([1, 2, 3], "application/octet-stream") },
		};
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting();
		var methodStarting = TestData.TestMethodStarting();
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(traits: TestData.EmptyTraits);
		var testPassed = TestData.TestPassed(output: "");
		var testFinished = TestData.TestFinished(attachments: attachments);
		var caseFinished = TestData.TestCaseFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var methodFinished = TestData.TestMethodFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var classFinished = TestData.TestClassFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var collectionFinished = TestData.TestCollectionFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(testFinished);
		handler.OnMessage(caseFinished);
		handler.OnMessage(methodFinished);
		handler.OnMessage(classFinished);
		handler.OnMessage(collectionFinished);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var assemblyElement = Assert.Single(testRunElement.Elements("test-suite"));
		VerifyTestSuite(assemblyElement, "Assembly", "1-1003", assemblyFinished, "test-assembly.dll", "./test-assembly.dll");
		var testSuiteElement = Assert.Single(assemblyElement.Elements("test-suite"));
		VerifyTestSuite(testSuiteElement, "TestSuite", "1-1000", collectionFinished, "test-collection-display-name", "test-collection-class");
		var testFixtureElement = Assert.Single(testSuiteElement.Elements("test-suite"));
		VerifyTestSuite(testFixtureElement, "TestFixture", "1-1001", classFinished, "test-class-simple-name", "test-class-name");
		var testCaseElement = Assert.Single(testFixtureElement.Elements("test-case"));

		var attachmentsElement = Assert.Single(testCaseElement.Elements("attachments"));
		Assert.Collection(
			attachmentsElement.Elements("attachment").OrderBy(a => a.Attribute("description")?.Value),
			attachmentElement =>
			{
				Assert.Equal("bytes", attachmentElement.Attribute("description")?.Value);

				var path = attachmentElement.Attribute("filePath")?.Value;
				Assert.NotNull(path);
				Assert.True(handler.FileSystem.Exists(path));
				Assert.Equal("bytes.bin", Path.GetFileName(path));
				Assert.Equal([1, 2, 3], handler.FileSystem.ReadAllBytes(path));
			},
			attachmentElement =>
			{
				Assert.Equal("hello", attachmentElement.Attribute("description")?.Value);

				var path = attachmentElement.Attribute("filePath")?.Value;
				Assert.NotNull(path);
				Assert.True(handler.FileSystem.Exists(path));
				Assert.Equal("hello.txt", Path.GetFileName(path));
				Assert.Equal("world", handler.FileSystem.ReadAllText(path));
			}
		);
	}

	public static IEnumerable<TheoryDataRow<IMessageSinkMessage>> AssemblyErrorData =
	[
		new(TestData.ErrorMessage()),
		new(TestData.TestAssemblyCleanupFailure()),
	];

	[Theory(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(AssemblyErrorData))]
	public async ValueTask SingleAssemblyError(IMessageSinkMessage errorMessage)
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 0);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(errorMessage);
		handler.OnMessage(assemblyFinished);

		var testRunElement = await handler.TestRunElement();
		var assemblyElement = Assert.Single(testRunElement.Elements("test-suite"));
		var errorMetadata = errorMessage as IErrorMetadata;
		Assert.NotNull(errorMetadata);
		var errors = new[] { (ExceptionUtility.CombineMessages(errorMetadata), ExceptionUtility.CombineStackTraces(errorMetadata)) };
		VerifyTestSuite(assemblyElement, "Assembly", "1-1000", assemblyFinished, "test-assembly.dll", "./test-assembly.dll", localErrors: errors);
	}

	[Fact]
	public async ValueTask MultipleAssemblyErrors()
	{
		var errorMessage1 = TestData.ErrorMessage();
		var errorMessage2 = TestData.TestAssemblyCleanupFailure();

		var assemblyStarting = TestData.TestAssemblyStarting();
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 0);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(errorMessage1);
		handler.OnMessage(errorMessage2);
		handler.OnMessage(assemblyFinished);

		var errors = new[] {
			(ExceptionUtility.CombineMessages(errorMessage1), ExceptionUtility.CombineStackTraces(errorMessage1)),
			(ExceptionUtility.CombineMessages(errorMessage2), ExceptionUtility.CombineStackTraces(errorMessage2)),
		};
		var testRunElement = await handler.TestRunElement();
		var assemblyElement = Assert.Single(testRunElement.Elements("test-suite"));
		VerifyTestSuite(assemblyElement, "Assembly", "1-1000", assemblyFinished, "test-assembly.dll", "./test-assembly.dll", localErrors: errors);
	}

	[Fact]
	public async ValueTask TestSuiteError()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var cleanupFailure = TestData.TestCollectionCleanupFailure();
		var collectionFinished = TestData.TestCollectionFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 0);
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 0);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(cleanupFailure);
		handler.OnMessage(collectionFinished);
		handler.OnMessage(assemblyFinished);

		var errors = new[] { (ExceptionUtility.CombineMessages(cleanupFailure), ExceptionUtility.CombineStackTraces(cleanupFailure)) };
		var testRunElement = await handler.TestRunElement();
		var assemblyElement = Assert.Single(testRunElement.Elements("test-suite"));
		VerifyTestSuite(assemblyElement, "Assembly", "1-1001", assemblyFinished, "test-assembly.dll", "./test-assembly.dll", childErrorCount: 1);
		var testSuiteElement = Assert.Single(assemblyElement.Elements("test-suite"));
		VerifyTestSuite(testSuiteElement, "TestSuite", "1-1000", collectionFinished, "test-collection-display-name", "test-collection-class", localErrors: errors);
	}

	public static IEnumerable<TheoryDataRow<IMessageSinkMessage>> FixtureErrorData =
	[
		new(TestData.TestClassCleanupFailure()),
		new(TestData.TestMethodCleanupFailure()),
		new(TestData.TestCaseCleanupFailure()),
		new(TestData.TestCleanupFailure()),
	];

	[Theory(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(FixtureErrorData))]
	public async ValueTask TestFixtureErrors(IMessageSinkMessage errorMessage)
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting();
		var classFinished = TestData.TestClassFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 0);
		var collectionFinished = TestData.TestCollectionFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 0);
		var assemblyFinished = TestData.TestAssemblyFinished(testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 0);
		await using var handler = TestableNUnitResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(errorMessage);
		handler.OnMessage(classFinished);
		handler.OnMessage(collectionFinished);
		handler.OnMessage(assemblyFinished);

		var cleanupFailure = errorMessage as IErrorMetadata;
		Assert.NotNull(cleanupFailure);
		var errors = new[] { (ExceptionUtility.CombineMessages(cleanupFailure), ExceptionUtility.CombineStackTraces(cleanupFailure)) };
		var testRunElement = await handler.TestRunElement();
		var assemblyElement = Assert.Single(testRunElement.Elements("test-suite"));
		VerifyTestSuite(assemblyElement, "Assembly", "1-1002", assemblyFinished, "test-assembly.dll", "./test-assembly.dll", childErrorCount: 1);
		var testSuiteElement = Assert.Single(assemblyElement.Elements("test-suite"));
		VerifyTestSuite(testSuiteElement, "TestSuite", "1-1000", collectionFinished, "test-collection-display-name", "test-collection-class", childErrorCount: 1);
		var testFixtureElement = Assert.Single(testSuiteElement.Elements("test-suite"));
		VerifyTestSuite(testFixtureElement, "TestFixture", "1-1001", collectionFinished, "test-class-simple-name", "test-class-name", localErrors: errors);
	}

	static string ToResult(
		int failCount,
		int warningCount) =>
			failCount != 0 ? "Failed" : warningCount != 0 ? "Warning" : "Passed";

	static void VerifyTestSuite(
		XElement testSuiteElement,
		string type,
		string id,
		IExecutionSummaryMetadata summary,
		string? name,
		string? fullName,
		int warnings = 0,
		int childErrorCount = 0,
		(string Message, string? StackTrace)[]? localErrors = null,
		string expectedStartTime = "2024-07-04T21:12:08.0000000Z",
		string expectedEndTime = "2024-07-04T21:12:28.0000000Z")
	{
		var result = ToResult(summary.TestsFailed + childErrorCount + (localErrors?.Length ?? 0), warnings);

		Assert.Equal(type, testSuiteElement.Attribute("type")?.Value);
		Assert.Equal(id, testSuiteElement.Attribute("id")?.Value);
		Assert.Equal(name ?? string.Empty, testSuiteElement.Attribute("name")?.Value);
		Assert.Equal(fullName ?? string.Empty, testSuiteElement.Attribute("fullname")?.Value);
		Assert.Equal("Runnable", testSuiteElement.Attribute("runstate")?.Value);
		Assert.Equal(summary.TestsTotal.ToString(CultureInfo.InvariantCulture), testSuiteElement.Attribute("testcasecount")?.Value);
		Assert.Equal(result, testSuiteElement.Attribute("result")?.Value);
		Assert.Equal(expectedStartTime, testSuiteElement.Attribute("start-time")?.Value);
		Assert.Equal(expectedEndTime, testSuiteElement.Attribute("end-time")?.Value);
		Assert.Equal(summary.ExecutionTime.ToString("0.000000", CultureInfo.InvariantCulture), testSuiteElement.Attribute("duration")?.Value);
		Assert.Equal(summary.TestsTotal.ToString(CultureInfo.InvariantCulture), testSuiteElement.Attribute("total")?.Value);
		Assert.Equal((summary.TestsTotal - summary.TestsFailed - summary.TestsSkipped - summary.TestsNotRun - warnings).ToString(CultureInfo.InvariantCulture), testSuiteElement.Attribute("passed")?.Value);
		Assert.Equal(summary.TestsFailed.ToString(CultureInfo.InvariantCulture), testSuiteElement.Attribute("failed")?.Value);
		Assert.Equal(warnings.ToString(CultureInfo.InvariantCulture), testSuiteElement.Attribute("warnings")?.Value);
		Assert.Equal("0", testSuiteElement.Attribute("inconclusive")?.Value);
		Assert.Equal((summary.TestsNotRun + summary.TestsSkipped).ToString(CultureInfo.InvariantCulture), testSuiteElement.Attribute("skipped")?.Value);
		Assert.Equal("0", testSuiteElement.Attribute("asserts")?.Value);

		switch (result)
		{
			case "Passed":
				Assert.Null(testSuiteElement.Attribute("site")?.Value);
				Assert.Null(testSuiteElement.Element("failure")?.Element("message")?.Value);
				break;

			case "Warning":
				Assert.Equal("Child", testSuiteElement.Attribute("site")?.Value);
				Assert.Equal("One or more child tests had warnings", testSuiteElement.Element("failure")?.Element("message")?.Value);
				break;

			case "Failed":
				VerifyTestSuiteErrors(
					testSuiteElement,
					localErrors is null ? "Child" : "TearDown",
					localErrors ?? [("One or more child tests had errors", null)]
				);
				break;
		}
	}

	static void VerifyTestSuiteErrors(
		XElement testSuiteElement,
		string site,
		(string Message, string? StackTrace)[] errors)
	{
		Assert.Equal(site, testSuiteElement.Attribute("site")?.Value);

		var failureElement = Assert.Single(testSuiteElement.Elements("failure"));

		if (errors.Length == 1)
		{
			Assert.Equal(errors[0].Message, failureElement.Element("message")?.Value);
			Assert.Equal(errors[0].StackTrace, failureElement.Element("stack-trace")?.Value, ignoreLineEndingDifferences: true);
			return;
		}

		var expectedMessage =
			"Multiple failures in clean-up:\r\n\r\n" +
			string.Join("\r\n\r\n", errors.Select((a, idx) => $"{idx + 1}) {a.Message}{(a.StackTrace is null ? "" : "\r\n" + a.StackTrace)}"));

		Assert.Equal(expectedMessage, failureElement.Element("message")?.Value, ignoreLineEndingDifferences: true);
		Assert.Null(failureElement.Element("stack-trace")?.Value);
	}

	class ClassUnderTest
	{
		[Fact]
		public async ValueTask TestMethod() { }
	}

	class TestableNUnitResultWriterMessageHandler :
			NUnitResultWriterMessageHandler
	{
		private readonly StringWriter stringWriter;

		public TestableNUnitResultWriterMessageHandler(
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
			var assemblyElement = document.XPathSelectElement("/test-run");
			Assert.NotNull(assemblyElement);

			return assemblyElement;
		}

		public static TestableNUnitResultWriterMessageHandler Create()
		{
			var stringWriter = new StringWriter();
			var textWriter = new XmlTextWriter(stringWriter);

			return new(stringWriter, textWriter, new SpyFileSystem());
		}
	}
}
