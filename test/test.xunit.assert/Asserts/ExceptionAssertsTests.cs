﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class ExceptionAssertsTests
{
    public class Throws_Generic_Action
    {
        [Fact]
        public static void ExpectExceptionButCodeDoesNotThrow()
        {
            try
            {
                Assert.Throws<ArgumentException>(() => { });
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Equal("(No exception was thrown)", exception.Actual);
            }
        }

        [Fact]
        public static void ExpectExceptionButCodeThrowsDerivedException()
        {
            try
            {
                Action testCode = () => { throw new InvalidOperationException(); };

                Assert.Throws<Exception>(testCode);
            }
            catch (XunitException exception)
            {
                Assert.Equal("Assert.Throws() Failure", exception.UserMessage);
            }
        }

        [Fact]
        public static void UnexpectedExceptionCapturedAsInnerException()
        {
            try
            {
                Action testCode = () => { throw new InvalidOperationException(); };

                Assert.Throws<Exception>(testCode);
            }
            catch (XunitException exception)
            {
                Assert.IsType<InvalidOperationException>(exception.InnerException);
            }
        }

        [Fact]
        public static void StackTraceForThrowsIsOriginalThrowNotAssertThrows()
        {
            try
            {
                Assert.Throws<InvalidCastException>(() => ThrowingMethod());
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Contains("Throws_Generic_Action.ThrowingMethod", exception.StackTrace);
                Assert.DoesNotContain("Xunit.Assert.Throws", exception.StackTrace);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowingMethod()
        {
            throw new ArgumentException();
        }

        [Fact]
        public static void GotExpectedException()
        {
            Action testCode = () => { throw new ArgumentException(); };

            var ex = Assert.Throws<ArgumentException>(testCode);

            Assert.NotNull(ex);
        }
    }

    public class Throws_Generic_Func
    {
        [Fact]
        public static void GuardClause()
        {
            Func<object> testCode = () => Task.Run(() => 0);

            var ex = Record.Exception(() => Assert.Throws<Exception>(testCode));

            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal("You must call Assert.ThrowsAsync, Assert.DoesNotThrowAsync, or Record.ExceptionAsync when testing async code.", ex.Message);
        }

        [Fact]
        public static void ExpectExceptionButCodeDoesNotThrow()
        {
            var accessor = new StubAccessor();

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
        public static void ExpectExceptionButCodeThrowsDerivedException()
        {
            var accessor = new StubAccessor();

            try
            {
                Assert.Throws<Exception>(() => accessor.FailingProperty);
            }
            catch (XunitException exception)
            {
                Assert.Equal("Assert.Throws() Failure", exception.UserMessage);
            }
        }

        [Fact]
        public static void StackTraceForThrowsIsOriginalThrowNotAssertThrows()
        {
            var accessor = new StubAccessor();

            try
            {
                Assert.Throws<InvalidCastException>(() => accessor.FailingProperty);
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Contains("StubAccessor.get_FailingProperty", exception.StackTrace);
                Assert.DoesNotContain("Xunit.Assert.Throws", exception.StackTrace);
            }
        }

        [Fact]
        public static void GotExpectedException()
        {
            var accessor = new StubAccessor();

            var ex = Assert.Throws<InvalidOperationException>(() => accessor.FailingProperty);

            Assert.NotNull(ex);
        }
    }

    public class ThrowsAsync_Generic
    {
        [Fact]
        public static async void ExpectExceptionButCodeDoesNotThrow()
        {
            try
            {
                await Assert.ThrowsAsync<ArgumentException>(() => Task.Run(() => { }));
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Equal("(No exception was thrown)", exception.Actual);
            }
        }

        [Fact]
        public static async void ExpectExceptionButCodeThrowsDerivedException()
        {
            try
            {
                Func<Task> testCode = () => Task.Run(() => { throw new InvalidOperationException(); });

                await Assert.ThrowsAsync<Exception>(testCode);
            }
            catch (XunitException exception)
            {
                Assert.Equal("Assert.Throws() Failure", exception.UserMessage);
            }
        }

        [Fact]
        public static async void UnexpectedExceptionCapturedAsyncAsInnerException()
        {
            try
            {
                Func<Task> testCode = () => Task.Run(() => { throw new InvalidOperationException(); });

                await Assert.ThrowsAsync<Exception>(testCode);
            }
            catch (XunitException exception)
            {
                Assert.IsType<InvalidOperationException>(exception.InnerException);
            }
        }

        [Fact]
        public static async void GotExpectedException()
        {
            Func<Task> testCode = () => Task.Run(() => { throw new ArgumentException(); });

            var ex = await Assert.ThrowsAsync<ArgumentException>(testCode);

            Assert.NotNull(ex);
        }
    }

    public class ThrowsAny_Generic_Action
    {
        [Fact]
        public static void ExpectExceptionButCodeDoesNotThrow()
        {
            try
            {
                Assert.ThrowsAny<ArgumentException>(() => { });
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Equal("(No exception was thrown)", exception.Actual);
            }
        }

        [Fact]
        public static void StackTraceForThrowsIsOriginalThrowNotAssertThrows()
        {
            try
            {
                Assert.ThrowsAny<InvalidCastException>(() => ThrowingMethod());
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Contains("ThrowsAny_Generic_Action.ThrowingMethod", exception.StackTrace);
                Assert.DoesNotContain("Xunit.Assert.ThrowsAny", exception.StackTrace);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowingMethod()
        {
            throw new ArgumentException();
        }

        [Fact]
        public static void GotExpectedException()
        {
            Action testCode = () => { throw new ArgumentException(); };

            var ex = Assert.ThrowsAny<ArgumentException>(testCode);

            Assert.NotNull(ex);
        }

        [Fact]
        public static void GotDerivedException()
        {
            Action testCode = () => { throw new ArgumentException(); };

            var ex = Assert.ThrowsAny<Exception>(testCode);

            Assert.NotNull(ex);
        }
    }

    public class ThrowsAny_Generic_Func
    {
        [Fact]
        public static void GuardClause()
        {
            Func<object> testCode = () => Task.Run(() => 0);

            var ex = Record.Exception(() => Assert.ThrowsAny<Exception>(testCode));

            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal("You must call Assert.ThrowsAsync, Assert.DoesNotThrowAsync, or Record.ExceptionAsync when testing async code.", ex.Message);
        }

        [Fact]
        public static void ExpectExceptionButCodeDoesNotThrow()
        {
            var accessor = new StubAccessor();

            try
            {
                Assert.ThrowsAny<ArgumentException>(() => accessor.SuccessfulProperty);
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Equal("(No exception was thrown)", exception.Actual);
            }
        }

        [Fact]
        public static void StackTraceForThrowsIsOriginalThrowNotAssertThrows()
        {
            var accessor = new StubAccessor();

            try
            {
                Assert.ThrowsAny<InvalidCastException>(() => accessor.FailingProperty);
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Contains("StubAccessor.get_FailingProperty", exception.StackTrace);
                Assert.DoesNotContain("Xunit.Assert.ThrowsAny", exception.StackTrace);
            }
        }

        [Fact]
        public static void GotExpectedException()
        {
            var accessor = new StubAccessor();

            var ex = Assert.ThrowsAny<InvalidOperationException>(() => accessor.FailingProperty);

            Assert.NotNull(ex);
        }

        [Fact]
        public static void GotDerivedException()
        {
            var accessor = new StubAccessor();

            var ex = Assert.ThrowsAny<Exception>(() => accessor.FailingProperty);

            Assert.NotNull(ex);
        }
    }

    public class ThrowsAnyAsync_Generic
    {
        [Fact]
        public static async void ExpectExceptionButCodeDoesNotThrow()
        {
            try
            {
                await Assert.ThrowsAnyAsync<ArgumentException>(() => Task.Run(() => { }));
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Equal("(No exception was thrown)", exception.Actual);
            }
        }

        [Fact]
        public static async void GotExpectedException()
        {
            Func<Task> testCode = () => Task.Run(() => { throw new ArgumentException(); });

            var ex = await Assert.ThrowsAnyAsync<ArgumentException>(testCode);

            Assert.NotNull(ex);
        }

        [Fact]
        public static async void GotDerivedException()
        {
            try
            {
                Func<Task> testCode = () => Task.Run(() => { throw new InvalidOperationException(); });

                await Assert.ThrowsAnyAsync<Exception>(testCode);
            }
            catch (XunitException exception)
            {
                Assert.Equal("Assert.Throws() Failure", exception.UserMessage);
            }
        }
    }

    public class Throws_NonGeneric_Action
    {
        [Fact]
        public static void ExpectExceptionButCodeDoesNotThrow()
        {
            try
            {
                Assert.Throws(typeof(ArgumentException), () => { });
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Equal("(No exception was thrown)", exception.Actual);
            }
        }

        [Fact]
        public static void ExpectExceptionButCodeThrowsDerivedException()
        {
            try
            {
                Assert.Throws(typeof(Exception), () => { throw new InvalidOperationException(); });
            }
            catch (XunitException exception)
            {
                Assert.Equal("Assert.Throws() Failure", exception.UserMessage);
            }
        }

        [Fact]
        public static void GotExpectedException()
        {
            var ex = Assert.Throws(typeof(ArgumentException), () => { throw new ArgumentException(); });

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
        }
    }

    public class Throws_NonGeneric_Func
    {
        [Fact]
        public static void GuardClause()
        {
            Func<object> testCode = () => Task.Run(() => { });

            var ex = Record.Exception(() => Assert.Throws(typeof(Exception), testCode));

            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal("You must call Assert.ThrowsAsync, Assert.DoesNotThrowAsync, or Record.ExceptionAsync when testing async code.", ex.Message);
        }

        [Fact]
        public static void ExpectExceptionButCodeDoesNotThrow()
        {
            var accessor = new StubAccessor();

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
        public static void ExpectExceptionButCodeThrowsDerivedException()
        {
            var accessor = new StubAccessor();

            try
            {
                Assert.Throws(typeof(Exception), () => accessor.FailingProperty);
            }
            catch (XunitException exception)
            {
                Assert.Equal("Assert.Throws() Failure", exception.UserMessage);
            }
        }

        [Fact]
        public static void GotExpectedException()
        {
            var accessor = new StubAccessor();

            var ex = Assert.Throws(typeof(InvalidOperationException), () => accessor.FailingProperty);

            Assert.NotNull(ex);
            Assert.IsType<InvalidOperationException>(ex);
        }
    }

    public class ThrowsAsync_NonGeneric
    {
        [Fact]
        public static async void ExpectExceptionButCodeDoesNotThrow()
        {
            try
            {
                Func<Task> testCode = () => Task.Run(() => { });

                await Assert.ThrowsAsync(typeof(ArgumentException), testCode);
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Equal("(No exception was thrown)", exception.Actual);
            }
        }

        [Fact]
        public static async void ExpectExceptionButCodeThrowsDerivedException()
        {
            try
            {
                Func<Task> testCode = () => Task.Run(() => { throw new InvalidOperationException(); });

                await Assert.ThrowsAsync(typeof(Exception), testCode);
            }
            catch (XunitException exception)
            {
                Assert.Equal("Assert.Throws() Failure", exception.UserMessage);
            }
        }

        [Fact]
        public static async void GotExpectedException()
        {
            Func<Task> testCode = () => Task.Run(() => { throw new ArgumentException(); });

            var ex = await Assert.ThrowsAsync(typeof(ArgumentException), testCode);

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
        }
    }

    public class ThrowsArgument_Action
    {
        [Fact]
        public static void ExpectExceptionButCodeDoesNotThrow()
        {
            Action testCode = () => { };

            var ex = Record.Exception(() => Assert.Throws<ArgumentException>("paramName", testCode));

            var throwsEx = Assert.IsType<ThrowsException>(ex);
            Assert.Equal("(No exception was thrown)", throwsEx.Actual);
        }

        [Fact]
        public static void ExpectExceptionButCodeThrowsDerivedException()
        {
            Action testCode = () => { throw new InvalidOperationException(); };

            var ex = Record.Exception(() => Assert.Throws<ArgumentException>("paramName", testCode));

            Assert.IsType<ThrowsException>(ex);
            Assert.Contains("Assert.Throws() Failure" + Environment.NewLine +
                            "Expected: typeof(System.ArgumentException)" + Environment.NewLine +
                            "Actual:   typeof(System.InvalidOperationException)", ex.Message);
        }

        [Fact]
        public static void StackTraceForThrowsIsOriginalThrowNotAssertThrows()
        {
            Action testCode = () => ThrowingMethod();

            var ex = Record.Exception(() => Assert.Throws<ArgumentException>("paramName", testCode));

            Assert.Contains("ThrowsArgument_Action.ThrowingMethod", ex.StackTrace);
            Assert.DoesNotContain("Xunit.Assert.Throws", ex.StackTrace);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowingMethod()
        {
            throw new InvalidCastException();
        }

        [Fact]
        public static void GotExpectedException()
        {
            Action testCode = () => { throw new ArgumentException("message", "paramName"); };

            var ex = Assert.Throws<ArgumentException>("paramName", testCode);

            Assert.NotNull(ex);
        }

        [Fact]
        public static void MismatchedParameterName()
        {
            Action testCode = () => { throw new ArgumentException("message", "paramName2"); };

            var ex = Record.Exception(() => Assert.Throws<ArgumentException>("paramName", testCode));

            var eqEx = Assert.IsType<EqualException>(ex);
            Assert.Equal("paramName", eqEx.Expected);
            Assert.Equal("paramName2", eqEx.Actual);
        }
    }

    public class ThrowsArgument_Func
    {
        [Fact]
        public static void GuardClause()
        {
            Func<object> testCode = () => Task.Run(() => { throw new ArgumentException("foo", "param"); });

            var ex = Record.Exception(() => Assert.Throws<ArgumentException>("param", testCode));

            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal("You must call Assert.ThrowsAsync, Assert.DoesNotThrowAsync, or Record.ExceptionAsync when testing async code.", ex.Message);
        }
    }

    public class ThrowsArgumentAsync
    {
        [Fact]
        public static async void ExpectExceptionButCodeDoesNotThrow()
        {
            Func<Task> testCode = () => Task.Run(() => { });

            var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>("paramName", testCode));

            var throwsEx = Assert.IsType<ThrowsException>(ex);
            Assert.Equal("(No exception was thrown)", throwsEx.Actual);
        }

        [Fact]
        public static async void ExpectExceptionButCodeThrowsDerivedException()
        {
            Func<Task> testCode = () => Task.Run(() => { throw new InvalidOperationException(); });

            var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>("paramName", testCode));

            Assert.IsType<ThrowsException>(ex);
            Assert.Contains("Assert.Throws() Failure" + Environment.NewLine +
                            "Expected: typeof(System.ArgumentException)" + Environment.NewLine +
                            "Actual:   typeof(System.InvalidOperationException)", ex.Message);
        }

        [Fact]
        public static async void StackTraceForThrowsIsOriginalThrowNotAssertThrows()
        {
            Func<Task> testCode = () => Task.Run(() => ThrowingMethod());

            var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>("paramName", testCode));

            Assert.Contains("ThrowsArgumentAsync.ThrowingMethod", ex.StackTrace);
            Assert.DoesNotContain("Xunit.Assert.ThrowsAsync", ex.StackTrace);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowingMethod()
        {
            throw new InvalidCastException();
        }

        [Fact]
        public static async void GotExpectedException()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>("paramName", () => Task.Run(() => { throw new ArgumentException("message", "paramName"); }));

            Assert.NotNull(ex);
        }

        [Fact]
        public static async void MismatchedParameterName()
        {
            Func<Task> testCode = () => Task.Run(() => { throw new ArgumentException("message", "paramName2"); });

            var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>("paramName", testCode));

            var eqEx = Assert.IsType<EqualException>(ex);
            Assert.Equal("paramName", eqEx.Expected);
            Assert.Equal("paramName2", eqEx.Actual);
        }
    }

    public class ThrowsArgumentNull_Action
    {
        [Fact]
        public static void ExpectExceptionButCodeDoesNotThrow()
        {
            Action testCode = () => { };

            var ex = Record.Exception(() => Assert.Throws<ArgumentNullException>("paramName", testCode));

            var throwsEx = Assert.IsType<ThrowsException>(ex);
            Assert.Equal("(No exception was thrown)", throwsEx.Actual);
        }

        [Fact]
        public static void ExpectExceptionButCodeThrowsDerivedException()
        {
            Action testCode = () => { throw new InvalidOperationException(); };

            var ex = Record.Exception(() => Assert.Throws<ArgumentNullException>("paramName", testCode));

            Assert.IsType<ThrowsException>(ex);
            Assert.Contains("Assert.Throws() Failure" + Environment.NewLine +
                            "Expected: typeof(System.ArgumentNullException)" + Environment.NewLine +
                            "Actual:   typeof(System.InvalidOperationException)", ex.Message);
        }

        [Fact]
        public static void StackTraceForThrowsIsOriginalThrowNotAssertThrows()
        {
            Action testCode = () => ThrowingMethod();

            var ex = Record.Exception(() => Assert.Throws<ArgumentNullException>("paramName", testCode));

            Assert.Contains("ThrowsArgumentNull_Action.ThrowingMethod", ex.StackTrace);
            Assert.DoesNotContain("Xunit.Assert.Throws", ex.StackTrace);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowingMethod()
        {
            throw new InvalidCastException();
        }

        [Fact]
        public static void GotExpectedException()
        {
            Action testCode = () => { throw new ArgumentNullException("paramName"); };

            var ex = Assert.Throws<ArgumentNullException>("paramName", testCode);

            Assert.NotNull(ex);
        }

        [Fact]
        public static void MismatchedParameterName()
        {
            Action testCode = () => { throw new ArgumentNullException("paramName2"); };

            var ex = Record.Exception(() => Assert.Throws<ArgumentNullException>("paramName", testCode));

            var eqEx = Assert.IsType<EqualException>(ex);
            Assert.Equal("paramName", eqEx.Expected);
            Assert.Equal("paramName2", eqEx.Actual);
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
