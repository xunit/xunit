using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class FalseTests
    {
        [Fact]
        public void AssertFalse()
        {
            Assert.False(false);
        }

        [Fact]
        public void AssertFalseThrowsExceptionWhenTrue()
        {
            try
            {
                Assert.False(true);
            }
            catch (AssertException exception)
            {
                Assert.Equal("Assert.False() Failure", exception.UserMessage);
            }
        }
    }
}
