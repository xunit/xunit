using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class ExceptionUtilityTests
{
    class FailureInformation : IFailureInformation, IEnumerable
    {
        List<int> exceptionParentIndices = new List<int>();
        List<string> exceptionTypes = new List<string>();
        List<string> messages = new List<string>();
        List<string> stackTraces = new List<string>();

        public string[] ExceptionTypes
        {
            get { return exceptionTypes.ToArray(); }
        }

        public string[] Messages
        {
            get { return messages.ToArray(); }
        }

        public string[] StackTraces
        {
            get { return stackTraces.ToArray(); }
        }

        public int[] ExceptionParentIndices
        {
            get { return exceptionParentIndices.ToArray(); }
        }

        public void Add(Exception ex, int index = -1)
        {
            Add(ex.GetType(), ex.Message, ex.StackTrace, index);
        }

        public void Add(Type exceptionType, string message = null, string stackTrace = null, int index = -1)
        {
            exceptionTypes.Add(exceptionType.FullName);
            messages.Add(message);
            stackTraces.Add(stackTrace);
            exceptionParentIndices.Add(index);
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class CombineMessages
    {
        [Fact]
        public void XunitException()
        {
            var failureInfo = new FailureInformation { new XunitException("This is the message") };

            var result = ExceptionUtility.CombineMessages(failureInfo);

            Assert.Equal("This is the message", result);
        }

        [Fact]
        public void NonXunitException()
        {
            var failureInfo = new FailureInformation { new Exception("This is the message") };

            var result = ExceptionUtility.CombineMessages(failureInfo);

            Assert.Equal("System.Exception : This is the message", result);
        }

        [Fact]
        public void NonXunitExceptionWithInnerExceptions()
        {
            var failureInfo = new FailureInformation {
                { new Exception("outer exception"), -1 },
                { new DivideByZeroException("inner exception"), 0 },
                { new XunitException("inner inner exception"), 1 }
            };

            var result = ExceptionUtility.CombineMessages(failureInfo);

            Assert.Equal("System.Exception : outer exception" + Environment.NewLine +
                         "---- System.DivideByZeroException : inner exception" + Environment.NewLine +
                         "-------- inner inner exception", result);
        }

        [Fact]
        public void AggregateException()
        {
            var failureInfo = new FailureInformation {
                { new AggregateException(), -1 },
                { new DivideByZeroException("inner #1"), 0 },
                { new NotImplementedException("inner #2"), 0 },
                { new XunitException("this is crazy"), 0 },
            };

            var result = ExceptionUtility.CombineMessages(failureInfo);

            Assert.Equal("System.AggregateException : One or more errors occurred." + Environment.NewLine
                       + "---- System.DivideByZeroException : inner #1" + Environment.NewLine
                       + "---- System.NotImplementedException : inner #2" + Environment.NewLine
                       + "---- this is crazy", result);
        }
    }

    public class CombineStackTraces
    {
        [Fact]
        public void XunitException()
        {
            Action testCode = () => { throw new XunitException(); };
            var ex = Record.Exception(testCode);
            var failureInfo = new FailureInformation { ex };

            var result = ExceptionUtility.CombineStackTraces(failureInfo);

            Assert.DoesNotContain(typeof(Record).FullName, result);
            Assert.DoesNotContain(typeof(XunitException).FullName, result);
            Assert.Contains("at ExceptionUtilityTests.CombineStackTraces", result);
        }

        [Fact]
        public void NonXunitException()
        {
            Action testCode = () => { throw new Exception(); };
            var ex = Record.Exception(testCode);
            var failureInfo = new FailureInformation { ex };

            var result = ExceptionUtility.CombineStackTraces(failureInfo);

            Assert.DoesNotContain(typeof(Record).FullName, result);
            Assert.DoesNotContain(typeof(XunitException).FullName, result);
            Assert.Contains("at ExceptionUtilityTests.CombineStackTraces", result);
        }

        [Fact]
        public void NonXunitExceptionWithInnerExceptions()
        {
            Action innerTestCode = () => { throw new DivideByZeroException(); };
            var inner = Record.Exception(innerTestCode);
            Action outerTestCode = () => { throw new Exception("message", inner); };
            var outer = Record.Exception(outerTestCode);
            var failureInfo = new FailureInformation { { outer, -1 }, { inner, 0 } };

            var result = ExceptionUtility.CombineStackTraces(failureInfo);

            Assert.Collection(result.Split(new[] { Environment.NewLine }, StringSplitOptions.None),
                line => Assert.Contains("at ExceptionUtilityTests.CombineStackTraces", line),
                line => Assert.Equal("----- Inner Stack Trace -----", line),
                line => Assert.Contains("at ExceptionUtilityTests.CombineStackTraces", line)
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
            var failureInfo = new FailureInformation { { outer, -1 }, { inner1, 0 }, { inner2, 0 }, { inner3, 0 } };

            var result = ExceptionUtility.CombineStackTraces(failureInfo);

            Assert.Collection(result.Split(new[] { Environment.NewLine }, StringSplitOptions.None),
                line => Assert.Contains("at ExceptionUtilityTests.CombineStackTraces", line),
                line => Assert.Equal("----- Inner Stack Trace #1 (System.DivideByZeroException) -----", line),
                line => Assert.Contains("at ExceptionUtilityTests.CombineStackTraces", line),
                line => Assert.Equal("----- Inner Stack Trace #2 (System.NotImplementedException) -----", line),
                line => Assert.Contains("at ExceptionUtilityTests.CombineStackTraces", line),
                line => Assert.Equal("----- Inner Stack Trace #3 (Xunit.Sdk.XunitException) -----", line),
                line => Assert.Contains("at ExceptionUtilityTests.CombineStackTraces", line)
            );
        }
    }
}