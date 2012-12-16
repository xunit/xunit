using Xunit;
using Xunit.Sdk;

public class BooleanAssertsTests
{
    public class False
    {
        [Fact]
        public void AssertFalse()
        {
            Assert.False(false);
        }

        [Fact]
        public void ThrowsExceptionWhenTrue()
        {
            var ex = Record.Exception(() => Assert.False(true));

            Assert.IsType<FalseException>(ex);
            Assert.Equal("Assert.False() Failure", ex.Message);
        }

        [Fact]
        public void UserSuppliedMessage()
        {
            var ex = Record.Exception(() => Assert.False(true, "Custom User Message"));

            Assert.Equal("Custom User Message", ex.Message);
        }
    }

    public class True
    {
        [Fact]
        public void AssertTrue()
        {
            Assert.True(true);
        }

        [Fact]
        public void ThrowsExceptionWhenFalse()
        {
            var ex = Record.Exception(() => Assert.True(false));

            Assert.IsType<TrueException>(ex);
            Assert.Equal("Assert.True() Failure", ex.Message);
        }

        [Fact]
        public void UserSuppliedMessage()
        {
            var ex = Record.Exception(() => Assert.True(false, "Custom User Message"));

            Assert.Equal("Custom User Message", ex.Message);
        }
    }
}
