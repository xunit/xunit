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

public class XmlV1ResultWriterMessageHandlerTests
{
	[Fact]
	public async ValueTask AssemblyStarting()
	{
		var assemblyStarting = TestData.TestAssemblyStarting(
			assemblyPath: "/path/to/assembly.dll",
			configFilePath: "config",
			startTime: new DateTimeOffset(2013, 7, 6, 16, 24, 32, TimeSpan.Zero),
			testEnvironment: "256-bit MentalFloss",
			testFrameworkDisplayName: "xUnit.net v14.42"
		);
		await using var handler = TestableXmlV1ResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);

		var assemblyElement = await handler.AssemblyElement();
		Assert.Equal("config", assemblyElement.Attribute("configFile")?.Value);
		Assert.Equal("256-bit MentalFloss", assemblyElement.Attribute("environment")?.Value);
		Assert.Equal("/path/to/assembly.dll", assemblyElement.Attribute("name")?.Value);
		Assert.Equal("2013-07-06", assemblyElement.Attribute("run-date")?.Value);
		Assert.Equal("16:24:32", assemblyElement.Attribute("run-time")?.Value);
		Assert.Equal("xUnit.net v14.42", assemblyElement.Attribute("test-framework")?.Value);
	}

	[Fact]
	public async ValueTask AssemblyStartingDoesNotIncludeOptionalValue()
	{
		var assemblyStarting = TestData.TestAssemblyStarting(assemblyPath: "/path/to/assembly.dll", configFilePath: null);
		await using var handler = TestableXmlV1ResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);

		var assemblyElement = await handler.AssemblyElement();
		Assert.Null(assemblyElement.Attribute("configFile"));
	}

	[CulturedFactDefault]
	public async ValueTask AssemblyFinished()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var assemblyFinished = TestData.TestAssemblyFinished(
			executionTime: 123.4567M,
			testsFailed: 42,
			testsNotRun: 3,
			testsSkipped: 6,
			testsTotal: 2112
		);
		await using var handler = TestableXmlV1ResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(assemblyFinished);

		var assemblyElement = await handler.AssemblyElement();
		Assert.Equal("42", assemblyElement.Attribute("failed")?.Value);
		Assert.Equal("2061", assemblyElement.Attribute("passed")?.Value);
		Assert.Equal("6", assemblyElement.Attribute("skipped")?.Value);
		Assert.Equal(123.457M.ToString(CultureInfo.InvariantCulture), assemblyElement.Attribute("time")?.Value);
		Assert.Equal("2112", assemblyElement.Attribute("total")?.Value);
	}

	[CulturedFactDefault]
	public async ValueTask TestClasses()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var assemblyFinished = TestData.TestAssemblyFinished();
		var testClassStarting = TestData.TestClassStarting(testClassUniqueID: "class-id", testClassName: "Class Name");
		var testClassFinished = TestData.TestClassFinished(testClassUniqueID: "class-id", testsTotal: 2112, testsFailed: 42, testsSkipped: 6, testsNotRun: 3, executionTime: 123.4567m);
		await using var handler = TestableXmlV1ResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testClassStarting);
		handler.OnMessage(testClassFinished);
		handler.OnMessage(assemblyFinished);

		var assemblyElement = await handler.AssemblyElement();
		var classElement = Assert.Single(assemblyElement.Elements("class"));
		Assert.Equal("Class Name", classElement.Attribute("name")?.Value);
		Assert.Equal("2112", classElement.Attribute("total")?.Value);
		Assert.Equal("2061", classElement.Attribute("passed")?.Value);
		Assert.Equal("42", classElement.Attribute("failed")?.Value);
		Assert.Equal("6", classElement.Attribute("skipped")?.Value);
		Assert.Equal(123.457M.ToString(CultureInfo.InvariantCulture), classElement.Attribute("time")?.Value);
	}

	[CulturedFactDefault]
	public async ValueTask TestPassed()
	{
		var assemblyFinished = TestData.TestAssemblyFinished();
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting(traits: TestData.EmptyTraits);
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testPassed = TestData.TestPassed(executionTime: 123.4567809m, output: "test output");
		await using var handler = TestableXmlV1ResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(assemblyFinished);

		var assemblyElement = await handler.AssemblyElement();
		var testElement = Assert.Single(assemblyElement.Elements("class").Single().Elements("test"));
		Assert.Equal("Test Display Name", testElement.Attribute("name")?.Value);
		Assert.Equal(typeof(ClassUnderTest).FullName, testElement.Attribute("type")?.Value);
		Assert.Equal("TestMethod", testElement.Attribute("method")?.Value);
		Assert.Equal("Pass", testElement.Attribute("result")?.Value);
		Assert.Equal(123.4567809M.ToString(CultureInfo.InvariantCulture), testElement.Attribute("time")?.Value);
		Assert.Empty(testElement.Elements("reason"));
		Assert.Empty(testElement.Elements("failure"));
		Assert.Empty(testElement.Elements("traits"));
	}

	[CulturedFactDefault]
	public async ValueTask TestFailed()
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
			stackTraces: ["Exception Stack Trace"]
		);
		await using var handler = TestableXmlV1ResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testFailed);
		handler.OnMessage(assemblyFinished);

		var assemblyElement = await handler.AssemblyElement();
		var testElement = Assert.Single(assemblyElement.Elements("class").Single().Elements("test"));
		Assert.Equal("Test Display Name", testElement.Attribute("name")?.Value);
		Assert.Equal(typeof(ClassUnderTest).FullName, testElement.Attribute("type")?.Value);
		Assert.Equal("TestMethod", testElement.Attribute("method")?.Value);
		Assert.Equal("Fail", testElement.Attribute("result")?.Value);
		Assert.Equal(123.4567809M.ToString(CultureInfo.InvariantCulture), testElement.Attribute("time")?.Value);
		var failureElement = Assert.Single(testElement.Elements("failure"));
		Assert.Equal("Exception Type", failureElement.Attribute("exception-type")?.Value);
		Assert.Equal("Exception Type : Exception Message", failureElement.Elements("message").Single().Value);
		Assert.Equal("Exception Stack Trace", failureElement.Elements("stack-trace").Single().Value);
		Assert.Empty(testElement.Elements("reason"));
	}

	[Fact]
	public async ValueTask TestFailed_NullStackTrace()
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
			stackTraces: [default]
		);
		await using var handler = TestableXmlV1ResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testFailed);
		handler.OnMessage(assemblyFinished);

		var assemblyElement = await handler.AssemblyElement();
		var testElement = Assert.Single(assemblyElement.Elements("class").Single().Elements("test"));
		var failureElement = Assert.Single(testElement.Elements("failure"));
		Assert.Empty(failureElement.Elements("stack-trace").Single().Value);
	}

	[CulturedFactDefault]
	public async ValueTask TestSkipped()
	{
		var assemblyFinished = TestData.TestAssemblyFinished();
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting();
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testSkipped = TestData.TestSkipped(reason: "Skip Reason");
		await using var handler = TestableXmlV1ResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testSkipped);
		handler.OnMessage(assemblyFinished);

		var assemblyElement = await handler.AssemblyElement();
		var testElement = Assert.Single(assemblyElement.Elements("class").Single().Elements("test"));
		Assert.Equal("Test Display Name", testElement.Attribute("name")?.Value);
		Assert.Equal(typeof(ClassUnderTest).FullName, testElement.Attribute("type")?.Value);
		Assert.Equal("TestMethod", testElement.Attribute("method")?.Value);
		Assert.Equal("Skip", testElement.Attribute("result")?.Value);
		Assert.Equal(0m.ToString(CultureInfo.InvariantCulture), testElement.Attribute("time")?.Value);
		var reasonElement = Assert.Single(testElement.Elements("reason"));
		var messageElement = Assert.Single(reasonElement.Elements("message"));
		Assert.Equal("Skip Reason", reasonElement.Value);
		Assert.Empty(testElement.Elements("failure"));
	}

	[Fact]
	public async ValueTask TestResult_WithTraits()
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
		await using var handler = TestableXmlV1ResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(assemblyFinished);

		var assemblyElement = await handler.AssemblyElement();
		var traitsElements = assemblyElement.Elements("class").Single().Elements("test").Single().Elements("traits").Single().Elements("trait");
		var name1Element = Assert.Single(traitsElements, e => e.Attribute("name")?.Value == "name1");
		Assert.Equal("value1", name1Element.Attribute("value")?.Value);
		var name2Element = Assert.Single(traitsElements, e => e.Attribute("name")?.Value == "name2");
		Assert.Equal("value2", name2Element.Attribute("value")?.Value);
	}

	public static IEnumerable<TheoryDataRow<string, string>> IllegalXmlTestData =>
	[
		(
			new string(Enumerable.Range(0, 32).Select(x => (char)x).ToArray()),
			@"\0\x01\x02\x03\x04\x05\x06\a\b\t\n\v\f\r\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f"
		),
		// Invalid surrogate characters should be added as \x----, where ---- is the hex value of the char
		(
			"\xd800 Hello.World \xdc00",
			@"\xd800 Hello.World \xdc00"
		),
		// ...but valid ones should be outputted like normal
		(
			"\xd800\xdfff This.Is.Valid \xda00\xdd00",
			"\xd800\xdfff This.Is.Valid \xda00\xdd00" // note: no @
		),
	];

	[Theory]
	[MemberData(nameof(IllegalXmlTestData))]
	public async ValueTask TestResult_WithIllegalStringValue(
		string inputName,
		string outputName)
	{
		var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
		var testStarting = TestData.TestStarting(testDisplayName: inputName);
		var testSkipped = TestData.TestSkipped(reason: "Bad\0\r\nString");
		await using var handler = TestableXmlV1ResultWriterMessageHandler.Create();

		handler.OnMessage(TestData.TestAssemblyStarting());
		handler.OnMessage(TestData.TestCollectionStarting());
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(TestData.TestCaseStarting());
		handler.OnMessage(testStarting);
		handler.OnMessage(testSkipped);
		handler.OnMessage(TestData.TestFinished());
		handler.OnMessage(TestData.TestCaseFinished());
		handler.OnMessage(TestData.TestMethodFinished());
		handler.OnMessage(TestData.TestClassFinished());
		handler.OnMessage(TestData.TestCollectionFinished());
		handler.OnMessage(TestData.TestAssemblyFinished());

		var assemblyElement = await handler.AssemblyElement();
		var testElement = assemblyElement.XPathSelectElement("class/test");
		Assert.NotNull(testElement);
		Assert.Equal(outputName, testElement.Attribute("name")?.Value);
		var reasonElement = testElement.XPathSelectElement("reason");
		Assert.NotNull(reasonElement);
		Assert.Equal("Bad\\0\nString", reasonElement.Value);
	}

	class ClassUnderTest
	{
		[Fact]
		public async ValueTask TestMethod() { }
	}

	static void AssertFailureElement(
		XElement assemblyElement,
		string messageType,
		string? name)
	{
		var errorElement = Assert.Single(assemblyElement.Element("errors")!.Elements());
		Assert.Equal(messageType, errorElement.Attribute("type")?.Value);

		if (name is null)
			Assert.Null(errorElement.Attribute("name"));
		else
			Assert.Equal(name, errorElement.Attribute("name")?.Value);

		var failureElement = Assert.Single(errorElement.Elements("failure"));
		Assert.Equal("ExceptionType", failureElement.Attribute("exception-type")?.Value);
		Assert.Equal("ExceptionType : This is\\t\nmy message", failureElement.Elements("message").Single().Value);
		Assert.Equal("Line 1\nLine 2\nLine 3", failureElement.Elements("stack-trace").Single().Value);
	}

	class TestableXmlV1ResultWriterMessageHandler(
		StringWriter stringWriter,
		XmlWriter xmlWriter) :
			XmlV1ResultWriterMessageHandler(xmlWriter)
	{
		public async ValueTask<XElement> AssemblyElement()
		{
			await DisposeAsync();

			var document = XDocument.Parse(stringWriter.ToString());
			var assemblyElement = document.XPathSelectElement("/assemblies/assembly");
			Assert.NotNull(assemblyElement);

			return assemblyElement;
		}

		public static TestableXmlV1ResultWriterMessageHandler Create()
		{
			var stringWriter = new StringWriter();
			var textWriter = new XmlTextWriter(stringWriter);

			return new TestableXmlV1ResultWriterMessageHandler(stringWriter, textWriter);
		}
	}
}
