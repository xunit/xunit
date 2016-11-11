using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestCaseDescriptorFactoryTests
{
    protected List<string> callbackResults;

    protected void Callback(List<string> results)
        => callbackResults = results;

    public class MockDiscovery : TestCaseDescriptorFactoryTests
    {
        ITestFrameworkDiscoverer discoverer;

        public MockDiscovery()
        {
            discoverer = Substitute.For<ITestFrameworkDiscoverer>();
            discoverer.Serialize(null).ReturnsForAnyArgs(callInfo => $"Serialization of test case ID '{callInfo.Arg<ITestCase>().UniqueID}'");
        }

        [Fact]
        public void EmptyTestCase()
        {
            var testCase = Substitute.For<ITestCase>();

            new TestCaseDescriptorFactory(discoverer, new List<ITestCase> { testCase }, (Action<List<string>>)Callback);

            var result = Assert.Single(callbackResults);
            Assert.Equal("C \nM \nU \nD \nS Serialization of test case ID ''\n", result);
        }

        [Fact]
        public void NoDiscovererMeansNoSerialization()
        {
            var testCase = Substitute.For<ITestCase>();

            new TestCaseDescriptorFactory(null, new List<ITestCase> { testCase }, (Action<List<string>>)Callback);

            var result = Assert.Single(callbackResults);
            Assert.Equal("C \nM \nU \nD \n", result);
        }

        [Fact]
        public void PopulatedTestCase()
        {
            var sourceInfo = Substitute.For<ISourceInformation>();
            sourceInfo.FileName.Returns(@"C:\Foo\Bar.dll");
            sourceInfo.LineNumber.Returns(123456);

            var traits = new Dictionary<string, List<string>>
            {
                { "Name \n 1", new List<string> { "Value 1a", "Value \n 1b" } },
                { "Name 2", new List<string> { "Value 2" } }
            };

            var testCase = Mocks.TestCase<MockDiscovery>("PopulatedTestCase", "Display \n Name", "Skip \n Reason", "ABCDEF0123456789");
            testCase.SourceInformation.Returns(sourceInfo);
            testCase.Traits.Returns(traits);

            new TestCaseDescriptorFactory(discoverer, new List<ITestCase> { testCase }, (Action<List<string>>)Callback);

            var result = Assert.Single(callbackResults);
            Assert.Equal("C TestCaseDescriptorFactoryTests+MockDiscovery\n" +
                         "M PopulatedTestCase\n" +
                         "U ABCDEF0123456789\n" +
                         "D Display \\n Name\n" +
                         "S Serialization of test case ID 'ABCDEF0123456789'\n" +
                         "R Skip \\n Reason\n" +
                         "F C:\\Foo\\Bar.dll\n" +
                         "L 123456\n" +
                         "T Name \\n 1\nValue 1a\n" +
                         "T Name \\n 1\nValue \\n 1b\n" +
                         "T Name 2\nValue 2\n", result);
        }
    }

    public class RealDiscovery : TestCaseDescriptorFactoryTests
    {
        XunitTestFrameworkDiscoverer discoverer;
        List<ITestCase> testCases;

        public RealDiscovery()
        {
            var sourceInformationProvider = new NullSourceInformationProvider();
            var diagnosticMessageSink = new Xunit.NullMessageSink();
            var assembly = typeof(TestCaseDescriptorFactoryTests).Assembly;
            var assemblyInfo = Reflector.Wrap(assembly);

            discoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, sourceInformationProvider, diagnosticMessageSink);

            var discoverySink = new SpyMessageSink<IDiscoveryCompleteMessage>();
            discoverer.Find("TestCaseDescriptorFactoryTests+TestClass", false, discoverySink, TestFrameworkOptions.ForDiscovery());
            discoverySink.Finished.WaitOne();

            testCases = discoverySink.Messages.OfType<ITestCaseDiscoveryMessage>().Select(m => m.TestCase).ToList();
        }

        [Fact]
        public void XunitFactHasSpecialSerialization()
        {
            var testCase = testCases.Single(tc => tc.TestMethod.Method.Name == "FactMethod");

            new TestCaseDescriptorFactory(discoverer, new List<ITestCase> { testCase }, (Action<List<string>>)Callback);

            var result = Assert.Single(callbackResults);
            var serialization = Assert.Single(result.Split('\n').Where(line => line.StartsWith("S ")));
            Assert.Equal($"S :F:TestCaseDescriptorFactoryTests+TestClass:FactMethod:1:0:{testCase.TestMethod.TestClass.TestCollection.UniqueID.ToString("N")}", serialization);
        }

        [Fact]
        public void XunitTheoryDoesNotHaveSpecialSerialization()
        {
            var testCase = testCases.Single(tc => tc.TestMethod.Method.Name == "TheoryMethod");

            new TestCaseDescriptorFactory(discoverer, new List<ITestCase> { testCase }, (Action<List<string>>)Callback);

            var result = Assert.Single(callbackResults);
            var serialization = Assert.Single(result.Split('\n').Where(line => line.StartsWith("S ")));
            Assert.False(serialization.StartsWith("S :FACT"));
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
