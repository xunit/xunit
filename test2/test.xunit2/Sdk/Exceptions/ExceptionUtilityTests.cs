using System;
using Xunit;
using Xunit.Sdk;

public class ExceptionUtilityTests
{
    public class GetMessage
    {
        [Fact]
        public void XunitException()
        {
            var ex = new AssertException("This is the message");

            var result = ExceptionUtility.GetMessage(ex);

            Assert.Equal("This is the message", result);
        }

        [Fact]
        public void NonXunitException()
        {
            var ex = new Exception("This is the message");

            var result = ExceptionUtility.GetMessage(ex);

            Assert.Equal("System.Exception : This is the message", result);
        }

        [Fact]
        public void NonXunitExceptionWithInnerExceptions()
        {
            var inner = new DivideByZeroException("inner exception");
            var outer = new Exception("outer exception", inner);

            var result = ExceptionUtility.GetMessage(outer);

            Assert.Equal("System.Exception : outer exception" + Environment.NewLine +
                         "---- System.DivideByZeroException : inner exception", result);
        }
    }

    public class GetStackTrace
    {
        [Fact]
        public void XunitException()
        {
            var ex = Record.Exception(() => { throw new AssertException(); });

            var result = ExceptionUtility.GetStackTrace(ex);

            Assert.DoesNotContain(typeof(Record).FullName, result);
            Assert.DoesNotContain(typeof(AssertException).FullName, result);
            Assert.Contains("at ExceptionUtilityTests.GetStackTrace", result);
        }

        [Fact]
        public void NonXunitException()
        {
            var ex = Record.Exception(() => { throw new Exception(); });

            var result = ExceptionUtility.GetStackTrace(ex);

            Assert.DoesNotContain(typeof(Record).FullName, result);
            Assert.DoesNotContain(typeof(AssertException).FullName, result);
            Assert.Contains("at ExceptionUtilityTests.GetStackTrace", result);
        }

        [Fact]
        public void NonXunitExceptionWithInnerExceptions()
        {
            var inner = Record.Exception(() => { throw new DivideByZeroException(); });
            var outer = Record.Exception(() => { throw new Exception("message", inner); });

            var result = ExceptionUtility.GetStackTrace(outer);

            Assert.Collection(result.Split(new[] { Environment.NewLine }, StringSplitOptions.None),
                line => Assert.Contains("at ExceptionUtilityTests.GetStackTrace", line),
                line => Assert.Equal("----- Inner Stack Trace -----", line),
                line => Assert.Contains("at ExceptionUtilityTests.GetStackTrace", line)
            );
        }
    }
}