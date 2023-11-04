using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using NSubstitute;
using Xunit;
using Xunit.Runner.Common;
using Xunit.v3;

public class DelegatingXmlCreationSinkTests
{
	readonly IExecutionSink innerSink;

	public DelegatingXmlCreationSinkTests()
	{
		innerSink = new DelegatingSummarySink(
			new XunitProjectAssembly(new XunitProject()),
			_TestFrameworkOptions.ForDiscovery(),
			_TestFrameworkOptions.ForExecution(),
			AppDomainOption.Disabled,
			false,
			Substitute.For<_IMessageSink>()
		);
	}

	[Fact]
	public void AddsAssemblyStartingInformationToXml()
	{
		var assemblyStarting = TestData.TestAssemblyStarting(
			assemblyPath: "assembly",
			assemblyUniqueID: "assembly-id",
			configFilePath: "config",
			startTime: new DateTimeOffset(2013, 7, 6, 16, 24, 32, TimeSpan.Zero),
			targetFramework: "MentalFloss,Version=v21.12",
			testEnvironment: "256-bit MentalFloss",
			testFrameworkDisplayName: "xUnit.net v14.42"
		);

		var assemblyElement = new XElement("assembly");
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

		sink.OnMessage(assemblyStarting);

		Assert.Equal("assembly", assemblyElement.Attribute("name")!.Value);
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
		var assemblyStarting = TestData.TestAssemblyStarting(assemblyPath: null, configFilePath: null, targetFramework: null);
		var assemblyElement = new XElement("assembly");
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

		sink.OnMessage(assemblyStarting);

		Assert.Null(assemblyElement.Attribute("config-file"));
		Assert.Null(assemblyElement.Attribute("name"));  // derived from AssemblyPath
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
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);
		var errorMessage = TestData.ErrorMessage(
			exceptionParentIndices: new[] { -1 },
			exceptionTypes: new[] { "ExceptionType" },
			messages: new[] { "Message" },
			stackTraces: new[] { "Stack" }
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
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		var classStarting = TestData.TestClassStarting(testClass: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(testMethod: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testPassed = TestData.TestPassed(executionTime: 123.4567809m, output: "test output");

		var assemblyElement = new XElement("assembly");
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		Assert.Equal("DelegatingXmlCreationSinkTests+ClassUnderTest", testElement.Attribute("type")!.Value);
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
		var classStarting = TestData.TestClassStarting(testClass: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(testMethod: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testPassed = TestData.TestPassed(executionTime: 123.4567809m, output: "");

		var assemblyElement = new XElement("assembly");
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		Assert.Equal("DelegatingXmlCreationSinkTests+ClassUnderTest", testElement.Attribute("type")!.Value);
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
	public void AddsFailingTestElementToXml()
	{
		var assemblyFinished = TestData.TestAssemblyFinished();
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting(testClass: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(testMethod: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testFailed = TestData.TestFailed(
			exceptionParentIndices: new[] { -1 },
			exceptionTypes: new[] { "Exception Type" },
			executionTime: 123.4567809m,
			messages: new[] { "Exception Message" },
			output: "test output",
			stackTraces: new[] { "Exception Stack Trace" }
		);

		var assemblyElement = new XElement("assembly");
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		Assert.Equal("DelegatingXmlCreationSinkTests+ClassUnderTest", testElement.Attribute("type")!.Value);
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
		var classStarting = TestData.TestClassStarting(testClass: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(testMethod: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testFailed = TestData.TestFailed(
			exceptionParentIndices: new[] { -1 },
			exceptionTypes: new[] { "Exception Type" },
			executionTime: 123.4567809m,
			messages: new[] { "Exception Message" },
			output: "test output",
			stackTraces: new[] { default(string) }
		);

		var assemblyElement = new XElement("assembly");
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		var classStarting = TestData.TestClassStarting(testClass: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(testMethod: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testSkipped = TestData.TestSkipped(reason: "Skip Reason");

		var assemblyElement = new XElement("assembly");
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		Assert.Equal("DelegatingXmlCreationSinkTests+ClassUnderTest", testElement.Attribute("type")!.Value);
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
		var classStarting = TestData.TestClassStarting(testClass: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(testMethod: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testNotRun = TestData.TestNotRun();

		var assemblyElement = new XElement("assembly");
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		Assert.Equal("DelegatingXmlCreationSinkTests+ClassUnderTest", testElement.Attribute("type")!.Value);
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
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		var traits = new Dictionary<string, IReadOnlyList<string>>
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
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		var classStarting = TestData.TestClassStarting(testClass: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(testMethod: nameof(ClassUnderTest.TestMethod));
		var testStarting = TestData.TestStarting(testDisplayName: inputName);
		var testSkipped = TestData.TestSkipped(reason: "Bad\0\r\nString");

		var assemblyElement = new XElement("assembly");
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		Assert.Equal("Bad\\0\\r\\nString", reasonElement.Value);
	}

	class ClassUnderTest
	{
		[Fact]
		public void TestMethod() { }
	}

	readonly string assemblyID = "assembly-id";
	readonly string classID = "test-class-id";
	readonly string collectionID = "test-collection-id";
	readonly int[] exceptionParentIndices = new[] { -1 };
	readonly string[] exceptionTypes = new[] { "ExceptionType" };
	readonly string[] messages = new[] { "This is my message \t\r\n" };
	readonly string methodID = "test-method-id";
	readonly string[] stackTraces = new[] { "Line 1\r\nLine 2\r\nLine 3" };
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
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

		sink.OnMessage(caseStarting);
		sink.OnMessage(caseCleanupFailure);

		AssertFailureElement(assemblyElement, "test-case-cleanup", "MyTestCase");
	}

	[Fact]
	public void TestClassCleanupFailure()
	{
		var classStarting = TestData.TestClassStarting(
			assemblyUniqueID: assemblyID,
			testClass: "MyType",
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
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
			testMethod: "MyMethod",
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
		var sink = new DelegatingXmlCreationSink(innerSink, assemblyElement);

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
		Assert.Equal("ExceptionType : This is my message \\t\\r\\n", failureElement.Elements("message").Single().Value);
		Assert.Equal("Line 1\r\nLine 2\r\nLine 3", failureElement.Elements("stack-trace").Single().Value);
	}
}
