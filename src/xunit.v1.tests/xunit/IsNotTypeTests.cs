using System;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class IsNotTypeTests
    {
        [Fact]
        public void IsNotType()
        {
            InvalidCastException expected = new InvalidCastException();
            Assert.IsNotType(typeof(Exception), expected);
            Assert.IsNotType<Exception>(expected);
        }

        [Fact]
        public void IsNotTypeThrowsExceptionWhenWrongType()
        {
            AssertException exception =
                Assert.Throws<IsNotTypeException>(() => Assert.IsNotType<InvalidCastException>(new InvalidCastException()));

            Assert.Equal("Assert.IsNotType() Failure", exception.UserMessage);
        }

        [Fact]
        public void NullObjectDoesNotThrow()
        {
            Assert.DoesNotThrow(() => Assert.IsNotType<object>(null));
        }
    }
}
