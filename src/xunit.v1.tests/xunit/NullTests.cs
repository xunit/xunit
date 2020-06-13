using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class NullTests
    {
        [Fact]
        public void Null()
        {
            Assert.Null(null);
        }

        [Fact]
        public void NullThrowsExceptionWhenNotNull()
        {
            try
            {
                Assert.Null(new object());
            }
            catch (AssertException exception)
            {
                Assert.Equal("Assert.Null() Failure", exception.UserMessage);
            }
        }
    }
}
