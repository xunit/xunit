using System;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class AfterTestExceptionTests
    {
        [Fact]
        public void SingleException()
        {
            Exception ex = Record.Exception(delegate { throw new Exception("Exception Message"); });

            Exception result = Record.Exception(delegate { throw new AfterTestException(ex); });

            Assert.Equal("One or more exceptions were thrown from After methods during test cleanup", result.Message);
            Assert.Contains("System.Exception thrown: Exception Message", result.StackTrace);
            Assert.Contains("AfterTestExceptionTests", result.StackTrace);
            Assert.Contains("Xunit.Record.Exception", result.StackTrace);
        }

        [Fact]
        public void MultipleExceptions()
        {
            Exception ex1 = Record.Exception(delegate { throw new Exception("Exception Message"); });
            Exception ex2 = Record.Exception(delegate { throw new InvalidOperationException("Invalid Operation Message"); });

            Exception result = Record.Exception(delegate { throw new AfterTestException(ex1, ex2); });

            Assert.Equal("One or more exceptions were thrown from After methods during test cleanup", result.Message);
            Assert.Contains("System.Exception thrown: Exception Message", result.StackTrace);
            Assert.Contains("System.InvalidOperationException thrown: Invalid Operation Message", result.StackTrace);
        }

        [Fact]
        public void SerializesCustomProperties()
        {
            var originalInnerException = new AssertException("User Message");
            var originalException = new AfterTestException(originalInnerException);

            var deserializedException = SerializationUtility.SerializeAndDeserialize(originalException);

            var deserializedInnerException = Assert.Single(deserializedException.AfterExceptions);
            Assert.Equal(originalInnerException.Message, deserializedInnerException.Message);
            var deserializedAssertException = Assert.IsType<AssertException>(deserializedInnerException);
            Assert.Equal(originalInnerException.UserMessage, deserializedAssertException.UserMessage);
        }
    }
}
