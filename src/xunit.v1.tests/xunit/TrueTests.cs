using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class TrueTests
    {
        [Fact]
        public void AssertTrue()
        {
            Assert.True(true);
        }

        [Fact]
        public void AssertTrueThrowsExceptionWhenFalse()
        {
            TrueException exception = Assert.Throws<TrueException>(() => Assert.True(false));

            Assert.Equal("Assert.True() Failure", exception.UserMessage);
        }
    }
}
