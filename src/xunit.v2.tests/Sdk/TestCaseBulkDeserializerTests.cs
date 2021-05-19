using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestCaseBulkDeserializerTests
{
	readonly XunitTestFrameworkDiscoverer discoverer;
	readonly XunitTestFrameworkExecutor executor;

	public TestCaseBulkDeserializerTests()
	{
		var sourceInformationProvider = new NullSourceInformationProvider();
		var diagnosticMessageSink = new Xunit.NullMessageSink();
		var assembly = typeof(TestCaseBulkDeserializerTests).Assembly;
		var assemblyInfo = Reflector.Wrap(assembly);

		discoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, sourceInformationProvider, diagnosticMessageSink);
		executor = new XunitTestFrameworkExecutor(assembly.GetName(), sourceInformationProvider, diagnosticMessageSink);
	}

	[Theory]
	// Standard
	[InlineData(":F:TestCaseBulkDeserializerTests+TestClass:FactMethod:2:1:{0}")]
	// Trailing colon
	[InlineData(":F:TestCaseBulkDeserializerTests+TestClass:FactMethod:2:1:{0}:")]
	// Extra data
	[InlineData(":F:TestCaseBulkDeserializerTests+TestClass:FactMethod:2:1:{0}:unused")]
	public void CanDeserializeSpecialFactSerialization(string format)
	{
		var guid = Guid.NewGuid();
		var results = default(List<KeyValuePair<string, ITestCase>>);
		var serializedTestCases = new List<string> { string.Format(format, guid.ToString("N")) };
		Action<List<KeyValuePair<string, ITestCase>>> callback = r => results = r;

		new TestCaseBulkDeserializer(discoverer, executor, serializedTestCases, callback);

		var kvp = Assert.Single(results);
		Assert.Equal(kvp.Value.UniqueID, kvp.Key);
		Assert.Equal("TestCaseBulkDeserializerTests+TestClass", kvp.Value.TestMethod.TestClass.Class.Name);
		Assert.Equal("FactMethod", kvp.Value.TestMethod.Method.Name);
		Assert.Equal(guid, kvp.Value.TestMethod.TestClass.TestCollection.UniqueID);
	}

	[Fact]
	public void XunitFactWithColonsGetsEscaped()
	{
		var testMethod = Mocks.TestMethod("TESTS:TESTS", "a:b");
		var testCase = new XunitTestCase(null, Xunit.Sdk.TestMethodDisplay.ClassAndMethod, Xunit.Sdk.TestMethodDisplayOptions.None, testMethod);

		var serializedTestCase = discoverer.Serialize(testCase);

		Assert.StartsWith(":F:TESTS::TESTS:a::b:1:0:", serializedTestCase);
	}

	[Fact]
	public void CanDeserializeGeneralizedSerialization()
	{
		var discoverySink = new SpyMessageSink<IDiscoveryCompleteMessage>();
		discoverer.Find("TestCaseBulkDeserializerTests+TestClass", false, discoverySink, TestFrameworkOptions.ForDiscovery());
		discoverySink.Finished.WaitOne();
		var serializedTestCases =
			discoverySink
				.Messages
				.OfType<ITestCaseDiscoveryMessage>()
				.Where(m => m.TestCase.TestMethod.Method.Name == "TheoryMethod")
				.Select(m => discoverer.Serialize(m.TestCase))
				.ToList();

		var results = default(List<KeyValuePair<string, ITestCase>>);
		Action<List<KeyValuePair<string, ITestCase>>> callback = r => results = r;

		new TestCaseBulkDeserializer(discoverer, executor, serializedTestCases, callback);

		var kvp = Assert.Single(results);
		Assert.Equal(kvp.Value.UniqueID, kvp.Key);
		Assert.Equal("TestCaseBulkDeserializerTests+TestClass", kvp.Value.TestMethod.TestClass.Class.Name);
		Assert.Equal("TheoryMethod", kvp.Value.TestMethod.Method.Name);
		Assert.Equal("TestCaseBulkDeserializerTests+TestClass.TheoryMethod(x: 42)", kvp.Value.DisplayName);
	}

	class TestClass
	{
		[Fact]
		public void FactMethod() { }

		[Theory]
		[InlineData(42)]
		public void TheoryMethod(int x) { }
	}
}
