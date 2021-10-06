using Xunit;
using Xunit.Sdk;
using Xunit.Serialization;
using TestMethodDisplay = Xunit.Sdk.TestMethodDisplay;
using TestMethodDisplayOptions = Xunit.Sdk.TestMethodDisplayOptions;

public class XunitSkippedDataRowTestCaseTests
{
    // https://github.com/xunit/visualstudio.xunit/issues/266
    [Fact]
    public void SkipReasonIsProperlySerialized()
    {
        var messageSink = SpyMessageSink.Create();
        var testMethod = Mocks.TestMethod(typeof(XunitSkippedDataRowTestCaseTests), nameof(SkipReasonIsProperlySerialized));
        var testCase = new XunitSkippedDataRowTestCase(messageSink, TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, testMethod, "Skip Reason");

        var serialized = XunitSerializationInfo.Serialize(testCase);
        var deserialized = XunitSerializationInfo.Deserialize(testCase.GetType(), serialized);

        var deserializedTestCase = Assert.IsType<XunitSkippedDataRowTestCase>(deserialized);
        Assert.Equal("Skip Reason", deserializedTestCase.SkipReason);
    }
}
