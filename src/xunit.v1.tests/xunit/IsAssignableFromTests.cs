using System;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class IsAssignableFromTests
    {
        [Fact]
        public void IsAssignableFrom_SameType()
        {
            var expected = new InvalidCastException();
            Assert.IsAssignableFrom(typeof(InvalidCastException), expected);
            Assert.IsAssignableFrom<InvalidCastException>(expected);
        }

        [Fact]
        public void IsAssignableFrom_BaseType()
        {
            var expected = new InvalidCastException();
            Assert.IsAssignableFrom(typeof(Exception), expected);
            Assert.IsAssignableFrom<Exception>(expected);
        }

        [Fact]
        public void IsAssignableFrom_Interface()
        {
            var expected = new DisposableClass();
            Assert.IsAssignableFrom(typeof(IDisposable), expected);
            Assert.IsAssignableFrom<IDisposable>(expected);
        }

        [Fact]
        public void IsAssignableFromReturnsCastObject()
        {
            InvalidCastException expected = new InvalidCastException();
            InvalidCastException actual = Assert.IsAssignableFrom<InvalidCastException>(expected);
            Assert.Same(expected, actual);
        }

        [Fact]
        public void IsAssignableFromThrowsExceptionWhenWrongType()
        {
            var exception =
                Assert.Throws<IsAssignableFromException>(
                    () => Assert.IsAssignableFrom<InvalidCastException>(new InvalidOperationException())
                );

            Assert.Equal("Assert.IsAssignableFrom() Failure", exception.UserMessage);
        }

        [Fact]
        public void IsAssignableFromThrowsExceptionWhenPassedNull()
        {
            Assert.Throws<IsAssignableFromException>(() => Assert.IsAssignableFrom<object>(null));
        }

        class DisposableClass : IDisposable
        {
            public void Dispose() { }
        }
    }
}
