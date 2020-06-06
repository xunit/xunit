using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public class DefaultTestCaseDescriptorProviderTests
{
    ITestFrameworkDiscoverer discoverer;
    DefaultTestCaseDescriptorProvider provider;

    public DefaultTestCaseDescriptorProviderTests()
    {
        discoverer = Substitute.For<ITestFrameworkDiscoverer>();
        discoverer.Serialize(null).ReturnsForAnyArgs(callInfo => $"Serialization of test case ID '{callInfo.Arg<ITestCase>().UniqueID}'");

        provider = new DefaultTestCaseDescriptorProvider(discoverer);
    }

    [Fact]
    public void EmptyTestCase()
    {
        var testCase = Substitute.For<ITestCase>();

        var results = provider.GetTestCaseDescriptors(new List<ITestCase> { testCase }, false);

        var result = Assert.Single(results);
        Assert.Equal("", result.ClassName);
        Assert.Equal("", result.DisplayName);
        Assert.Equal("", result.MethodName);
        Assert.Null(result.Serialization);
        Assert.Equal("", result.SkipReason);
        Assert.Equal("", result.SourceFileName);
        Assert.Null(result.SourceLineNumber);
        Assert.Empty(result.Traits);
        Assert.Equal("", result.UniqueID);
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

        var testCase = Mocks.TestCase<DefaultTestCaseDescriptorProviderTests>("PopulatedTestCase", "Display \n Name", "Skip \n Reason", "ABCDEF0123456789");
        testCase.SourceInformation.Returns(sourceInfo);
        testCase.Traits.Returns(traits);

        var results = provider.GetTestCaseDescriptors(new List<ITestCase> { testCase }, true);

        var result = Assert.Single(results);
        Assert.Equal("DefaultTestCaseDescriptorProviderTests", result.ClassName);
        Assert.Equal("Display \n Name", result.DisplayName);
        Assert.Equal("PopulatedTestCase", result.MethodName);
        Assert.Equal("Serialization of test case ID 'ABCDEF0123456789'", result.Serialization);
        Assert.Equal("Skip \n Reason", result.SkipReason);
        Assert.Equal(@"C:\Foo\Bar.dll", result.SourceFileName);
        Assert.Equal(123456, result.SourceLineNumber);
        Assert.Equal("ABCDEF0123456789", result.UniqueID);

        Assert.Collection(result.Traits.OrderBy(kvp => kvp.Key),
            key =>
            {
                Assert.Equal("Name \n 1", key.Key);
                Assert.Collection(key.Value,
                    value => Assert.Equal("Value 1a", value),
                    value => Assert.Equal("Value \n 1b", value)
                );
            },
            key =>
            {
                Assert.Equal("Name 2", key.Key);
                var value = Assert.Single(key.Value);
                Assert.Equal("Value 2", value);
            }
        );
    }
}
