using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class NotNullTests
    {
        [Fact]
        public void NotNull()
        {
            Assert.NotNull(new object());
        }

        [Fact]
        public void NotNullThrowsException()
        {
            Assert.Throws<NotNullException>(() => Assert.NotNull(null));
        }
    }
}
