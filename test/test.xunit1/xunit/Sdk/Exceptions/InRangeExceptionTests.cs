using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class InRangeExceptionTests
    {
        [Fact]
        public void SerializesCustomProperties()
        {
            var originalException = new InRangeException("Actual", "Low", "High");

            var deserializedException = SerializationUtility.SerializeAndDeserialize(originalException);

            Assert.Equal(originalException.Actual, deserializedException.Actual);
            Assert.Equal(originalException.Low, deserializedException.Low);
            Assert.Equal(originalException.High, deserializedException.High);
        }
    }
}
