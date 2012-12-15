using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class DoesNotThrowTests
{
    [Fact]
    public void CorrectExceptionType()
    {
        DoesNotThrowException ex =
            Assert.Throws<DoesNotThrowException>(
                () => Assert.DoesNotThrow(
                    () => { throw new NotImplementedException("Exception Message"); }));

        Assert.Equal("Assert.DoesNotThrow() Failure", ex.UserMessage);
        Assert.Equal("(No exception)", ex.Expected);
        Assert.Equal("System.NotImplementedException: Exception Message", ex.Actual);
    }

    public class DoesNotThrowNoReturnValue
    {
        [Fact]
        public void CodeDoesNotThrow()
        {
            bool methodCalled = false;

            Assert.DoesNotThrow(() => methodCalled = true);

            Assert.True(methodCalled);
        }

        [Fact]
        public void CodeThrows()
        {
            var ex = Record.Exception(() => Assert.DoesNotThrow(() => ThrowingMethod()));

            Assert.IsType<DoesNotThrowException>(ex);
            Assert.Contains("NotImplementedException", ex.Message);
        }

        void ThrowingMethod()
        {
            throw new NotImplementedException();
        }
    }

    public class DoesNotThrowTask
    {
        [Fact]
        public void CodeDoesNotThrow()
        {
            bool methodCalled = false;

            Assert.DoesNotThrow(Task.Factory.StartNew(() => methodCalled = true));

            Assert.True(methodCalled);
        }

        [Fact]
        public void CodeThrows()
        {
            var ex = Record.Exception(() => Assert.DoesNotThrow(Task.Factory.StartNew(ThrowingMethod)));

            Assert.IsType<DoesNotThrowException>(ex);
            Assert.Contains("NotImplementedException", ex.Message);
        }

        void ThrowingMethod()
        {
            throw new NotImplementedException();
        }
    }

    public class DoesNotThrowWithReturnValue
    {
        [Fact]
        public void CodeDoesNotThrow()
        {
            bool methodCalled = false;

            Assert.DoesNotThrow(() => { methodCalled = true; return 0; });

            Assert.True(methodCalled);
        }

        [Fact]
        public void CodeThrows()
        {
            var ex = Record.Exception(() => Assert.DoesNotThrow(() => ThrowingMethod()));

            Assert.IsType<DoesNotThrowException>(ex);
            Assert.Contains("NotImplementedException", ex.Message);
        }

        int ThrowingMethod()
        {
            throw new NotImplementedException();
        }
    }
}