#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestCaseBulkDeserializerTests
{
    XunitTestFrameworkDiscoverer discoverer;
    XunitTestFrameworkExecutor executor;

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
    [InlineData(":F:TestCaseBulkDeserializerTests+TestClass:FactMethod:2:1:{0}")]         // Standard
    [InlineData(":F:TestCaseBulkDeserializerTests+TestClass:FactMethod:2:1:{0}:")]        // Trailing colon
    [InlineData(":F:TestCaseBulkDeserializerTests+TestClass:FactMethod:2:1:{0}:unused")]  // Extra data
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
        var serializedTestCases = discoverySink.Messages
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

    [Fact]
    public static void DeserializedFactsAndTheoriesFromTheSameClassStillShareFixtures()
    {
        var code = @"
using System;
using System.Threading;
using Xunit;

public class TestClassFixture : IDisposable
{
    public static long StaticConstructorCount = 0;
    public readonly long ConstructorInstance;

    public TestClassFixture() { ConstructorInstance = Interlocked.Increment(ref StaticConstructorCount); }

    public void Dispose() { Assert.Equal(1, StaticConstructorCount); }
}

public class TestClass : IClassFixture<TestClassFixture>
{
    readonly TestClassFixture fixture;

    public TestClass(TestClassFixture fixture) { this.fixture = fixture; }

    [Fact]
    public void FactMethod() { Assert.Equal(1, fixture.ConstructorInstance); }

    [Theory]
    [InlineData(42)]
    public void TheoryMethod(int x) { Assert.Equal(1, fixture.ConstructorInstance); }
}
";
        using (var assembly = CSharpAcceptanceTestV2Assembly.Create(code))
        {
            var discoverySink = new SpyMessageSink<IDiscoveryCompleteMessage>();
            var serializedTestCases = default(List<string>);
            var descriptors = default(List<TestCaseDescriptor>);

            using (var xunit2 = new Xunit2(AppDomainSupport.Required, new NullSourceInformationProvider(), assembly.FileName))
            {
                xunit2.Find("TestClass", false, discoverySink, TestFrameworkOptions.ForDiscovery());
                discoverySink.Finished.WaitOne();

                var testCases = discoverySink.Messages
                                             .OfType<ITestCaseDiscoveryMessage>()
                                             .Select(x => x.TestCase)
                                             .ToList();

                serializedTestCases = testCases.Select(x => xunit2.Serialize(x)).ToList();
                descriptors = xunit2.GetTestCaseDescriptors(testCases, true);
            }

            using (var xunit2 = new Xunit2(AppDomainSupport.Required, new NullSourceInformationProvider(), assembly.FileName))
            {
                var deserializations = default(List<ITestCase>);
                Action<List<KeyValuePair<string, ITestCase>>> callback = r => deserializations = r.Select(x => x.Value).ToList();

                new TestCaseBulkDeserializer(xunit2, xunit2, serializedTestCases, callback);

                var executionSink = new SpyMessageSink<ITestAssemblyFinished>();
                xunit2.RunTests(deserializations, executionSink, TestFrameworkOptions.ForExecution());
                executionSink.Finished.WaitOne();

                var passedTests = executionSink.Messages.OfType<ITestPassed>().ToList();
                var failedTests = executionSink.Messages.OfType<ITestFailed>().ToList();

                Assert.Equal(2, passedTests.Count);
                Assert.Empty(failedTests);
            }
        }
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

#endif
