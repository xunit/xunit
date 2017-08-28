using System;
using System.Collections.Generic;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestCaseDescriptorFactoryTests
{
    List<string> callbackResults;
    ITestFrameworkDiscoverer discoverer;

    public TestCaseDescriptorFactoryTests()
    {
        discoverer = Substitute.For<ITestFrameworkDiscoverer>();
        discoverer.Serialize(null).ReturnsForAnyArgs(callInfo => $"Serialization of test case ID '{callInfo.Arg<ITestCase>().UniqueID}'");
    }

    void Callback(List<string> results)
        => callbackResults = results;

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

        var testCase = Mocks.TestCase<TestCaseDescriptorFactoryTests>("PopulatedTestCase", "Display \n Name", "Skip \n Reason", "ABCDEF0123456789");
        testCase.SourceInformation.Returns(sourceInfo);
        testCase.Traits.Returns(traits);

        new TestCaseDescriptorFactory(discoverer, new List<ITestCase> { testCase }, (Action<List<string>>)Callback);

        var result = Assert.Single(callbackResults);
        Assert.Equal("C TestCaseDescriptorFactoryTests\n" +
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
