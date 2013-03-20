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

        [Fact]
        public void AggregateException()
        {
            var inner1 = new DivideByZeroException("inner #1");
            var inner2 = new NotImplementedException("inner #2");
            var inner3 = new AssertException("this is crazy");
            var outer = new AggregateException(inner1, inner2, inner3);

            var result = ExceptionUtility.GetMessage(outer);

            Assert.Equal<object>("System.AggregateException : One or more errors occurred." + Environment.NewLine
                               + "---- System.DivideByZeroException : inner #1" + Environment.NewLine
                               + "---- System.NotImplementedException : inner #2" + Environment.NewLine
                               + "---- this is crazy", result);
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

        [Fact]
        public void AggregateException()
        {
            var inner1 = Record.Exception(() => { throw new DivideByZeroException(); });
            var inner2 = Record.Exception(() => { throw new NotImplementedException("inner #2"); });
            var inner3 = Record.Exception(() => { throw new AssertException("this is crazy"); });
            var outer = Record.Exception(() => { throw new AggregateException(inner1, inner2, inner3); });

            var result = ExceptionUtility.GetStackTrace(outer);

            Assert.Collection(result.Split(new[] { Environment.NewLine }, StringSplitOptions.None),
                line => Assert.Contains("at ExceptionUtilityTests.GetStackTrace", line),
                line => Assert.Equal("----- Inner Stack Trace #1 (System.DivideByZeroException) -----", line),
                line => Assert.Contains("at ExceptionUtilityTests.GetStackTrace", line),
                line => Assert.Equal("----- Inner Stack Trace #2 (System.NotImplementedException) -----", line),
                line => Assert.Contains("at ExceptionUtilityTests.GetStackTrace", line),
                line => Assert.Equal("----- Inner Stack Trace #3 (Xunit.Sdk.AssertException) -----", line),
                line => Assert.Contains("at ExceptionUtilityTests.GetStackTrace", line)
            );
        }
    }
}