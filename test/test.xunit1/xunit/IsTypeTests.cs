using System;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class IsTypeTests
    {
        [Fact]
        public void IsType()
        {
            InvalidCastException expected = new InvalidCastException();
            Assert.IsType(typeof(InvalidCastException), expected);
            Assert.IsType<InvalidCastException>(expected);
        }

        [Fact]
        public void IsTypeReturnsCastObject()
        {
            InvalidCastException expected = new InvalidCastException();
            InvalidCastException actual = Assert.IsType<InvalidCastException>(expected);
            Assert.Same(expected, actual);
        }

        [Fact]
        public void IsTypeThrowsExceptionWhenWrongType()
        {
            AssertException exception =
                Assert.Throws<IsTypeException>(() => Assert.IsType<InvalidCastException>(new InvalidOperationException()));

            Assert.Equal("Assert.IsType() Failure", exception.UserMessage);
        }

        [Fact]
        public void IsTypeThrowsExceptionWhenPassedNull()
        {
            Assert.Throws<IsTypeException>(() => Assert.IsType<object>(null));
        }
    }
}
