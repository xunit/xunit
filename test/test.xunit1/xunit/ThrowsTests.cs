using System;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class ThrowsTests
    {
        public class DoesNotThrow
        {
            [Fact]
            public void DoesNotThrowException()
            {
                bool methodCalled = false;

                Assert.DoesNotThrow(() => methodCalled = true);

                Assert.True(methodCalled);
            }
        }

        public class ThrowsGenericNoReturnValue
        {
            [Fact]
            public void ExpectExceptionButCodeDoesNotThrow()
            {
                try
                {
                    Assert.Throws<ArgumentException>(delegate { });
                }
                catch (AssertActualExpectedException exception)
                {
                    Assert.Equal("(No exception was thrown)", exception.Actual);
                }
            }

            [Fact]
            public void ExpectExceptionButCodeThrowsDerivedException()
            {
                try
                {
                    Assert.Throws<Exception>(delegate { throw new InvalidOperationException(); });
                }
                catch (AssertException exception)
                {
                    Assert.Equal("Assert.Throws() Failure", exception.UserMessage);
                }
            }

            [Fact]
            public void StackTraceForThrowsIsOriginalThrowNotAssertThrows()
            {
                try
                {
                    Assert.Throws<InvalidCastException>(() => ThrowingMethod());
                }
                catch (AssertActualExpectedException exception)
                {
                    Assert.Contains("ThrowsGenericNoReturnValue.ThrowingMethod", exception.StackTrace);
                    Assert.DoesNotContain("Xunit.Assert", exception.StackTrace);
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void ThrowingMethod()
            {
                throw new ArgumentException();
            }

            [Fact]
            public void GotExpectedException()
            {
                ArgumentException ex =
                    Assert.Throws<ArgumentException>(delegate { throw new ArgumentException(); });

                Assert.NotNull(ex);
            }
        }

        public class ThrowsGenericWithReturnValue
        {
            [Fact]
            public void ExpectExceptionButCodeDoesNotThrow()
            {
                StubAccessor accessor = new StubAccessor();

                try
                {
                    Assert.Throws<ArgumentException>(() => accessor.SuccessfulProperty);
                }
                catch (AssertActualExpectedException exception)
                {
                    Assert.Equal("(No exception was thrown)", exception.Actual);
                }
            }

            [Fact]
            public void ExpectExceptionButCodeThrowsDerivedException()
            {
                StubAccessor accessor = new StubAccessor();

                try
                {
                    Assert.Throws<Exception>(() => accessor.FailingProperty);
                }
                catch (AssertException exception)
                {
                    Assert.Equal("Assert.Throws() Failure", exception.UserMessage);
                }
            }

            [Fact]
            public void StackTraceForThrowsIsOriginalThrowNotAssertThrows()
            {
                StubAccessor accessor = new StubAccessor();

                try
                {
                    Assert.Throws<InvalidCastException>(() => accessor.FailingProperty);
                }
                catch (AssertActualExpectedException exception)
                {
                    Assert.Contains("StubAccessor.get_FailingProperty", exception.StackTrace);
                    Assert.DoesNotContain("Xunit.Assert", exception.StackTrace);
                }
            }

            [Fact]
            public void GotExpectedException()
            {
                StubAccessor accessor = new StubAccessor();

                InvalidOperationException ex =
                    Assert.Throws<InvalidOperationException>(() => accessor.FailingProperty);

                Assert.NotNull(ex);
            }
        }

        public class ThrowsNonGenericNoReturnValue
        {
            [Fact]
            public void ExpectExceptionButCodeDoesNotThrow()
            {
                try
                {
                    Assert.Throws(typeof(ArgumentException), delegate { });
                }
                catch (AssertActualExpectedException exception)
                {
                    Assert.Equal("(No exception was thrown)", exception.Actual);
                }
            }

            [Fact]
            public void ExpectExceptionButCodeThrowsDerivedException()
            {
                try
                {
                    Assert.Throws(typeof(Exception), delegate { throw new InvalidOperationException(); });
                }
                catch (AssertException exception)
                {
                    Assert.Equal("Assert.Throws() Failure", exception.UserMessage);
                }
            }

            [Fact]
            public void GotExpectedException()
            {
                Exception ex =
                    Assert.Throws(typeof(ArgumentException), delegate { throw new ArgumentException(); });

                Assert.NotNull(ex);
                Assert.IsType<ArgumentException>(ex);
            }
        }

        public class ThrowsNonGenericWithReturnValue
        {
            [Fact]
            public void ExpectExceptionButCodeDoesNotThrow()
            {
                StubAccessor accessor = new StubAccessor();

                try
                {
                    Assert.Throws(typeof(ArgumentException), () => accessor.SuccessfulProperty);
                }
                catch (AssertActualExpectedException exception)
                {
                    Assert.Equal("(No exception was thrown)", exception.Actual);
                }
            }

            [Fact]
            public void ExpectExceptionButCodeThrowsDerivedException()
            {
                StubAccessor accessor = new StubAccessor();

                try
                {
                    Assert.Throws(typeof(Exception), () => accessor.FailingProperty);
                }
                catch (AssertException exception)
                {
                    Assert.Equal("Assert.Throws() Failure", exception.UserMessage);
                }
            }

            [Fact]
            public void GotExpectedException()
            {
                StubAccessor accessor = new StubAccessor();

                Exception ex =
                    Assert.Throws(typeof(InvalidOperationException), () => accessor.FailingProperty);

                Assert.NotNull(ex);
                Assert.IsType<InvalidOperationException>(ex);
            }
        }

        class StubAccessor
        {
            public int SuccessfulProperty { get; set; }

            public int FailingProperty
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                get { throw new InvalidOperationException(); }
            }
        }
    }
}
