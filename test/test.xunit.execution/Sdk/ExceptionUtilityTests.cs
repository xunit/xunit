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
            var ex = new XunitException("This is the message");

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
            var inner3 = new XunitException("this is crazy");
            var outer = new AggregateException(inner1, inner2, inner3);

            var result = ExceptionUtility.GetMessage(outer);

            Assert.Equal("System.AggregateException : One or more errors occurred." + Environment.NewLine
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
            Action testCode = () => { throw new XunitException(); };
            var ex = Record.Exception(testCode);

            var result = ExceptionUtility.GetStackTrace(ex);

            Assert.DoesNotContain(typeof(Record).FullName, result);
            Assert.DoesNotContain(typeof(XunitException).FullName, result);
            Assert.Contains("at ExceptionUtilityTests.GetStackTrace", result);
        }

        [Fact]
        public void NonXunitException()
        {
            Action testCode = () => { throw new Exception(); };
            var ex = Record.Exception(testCode);

            var result = ExceptionUtility.GetStackTrace(ex);

            Assert.DoesNotContain(typeof(Record).FullName, result);
            Assert.DoesNotContain(typeof(XunitException).FullName, result);
            Assert.Contains("at ExceptionUtilityTests.GetStackTrace", result);
        }

        [Fact]
        public void NonXunitExceptionWithInnerExceptions()
        {
            Action innerTestCode = () => { throw new DivideByZeroException(); };
            var inner = Record.Exception(innerTestCode);
            Action outerTestCode = () => { throw new Exception("message", inner); };
            var outer = Record.Exception(outerTestCode);

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
            Action inner1TestCode = () => { throw new DivideByZeroException(); };
            var inner1 = Record.Exception(inner1TestCode);
            Action inner2TestCode = () => { throw new NotImplementedException("inner #2"); };
            var inner2 = Record.Exception(inner2TestCode);
            Action inner3TestCode = () => { throw new XunitException("this is crazy"); };
            var inner3 = Record.Exception(inner3TestCode);
            Action outerTestCode = () => { throw new AggregateException(inner1, inner2, inner3); };
            var outer = Record.Exception(outerTestCode);

            var result = ExceptionUtility.GetStackTrace(outer);

            Assert.Collection(result.Split(new[] { Environment.NewLine }, StringSplitOptions.None),
                line => Assert.Contains("at ExceptionUtilityTests.GetStackTrace", line),
                line => Assert.Equal("----- Inner Stack Trace #1 (System.DivideByZeroException) -----", line),
                line => Assert.Contains("at ExceptionUtilityTests.GetStackTrace", line),
                line => Assert.Equal("----- Inner Stack Trace #2 (System.NotImplementedException) -----", line),
                line => Assert.Contains("at ExceptionUtilityTests.GetStackTrace", line),
                line => Assert.Equal("----- Inner Stack Trace #3 (Xunit.Sdk.XunitException) -----", line),
                line => Assert.Contains("at ExceptionUtilityTests.GetStackTrace", line)
            );
        }
    }
}