using System;
using System.Runtime.Serialization;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class DoesNotThrowExceptionTests
    {
        [Fact]
        public void SerializesCustomProperties()
        {
            var originalException = new TestableDoesNotThrowException("Stack Trace");

            var deserializedException = SerializationUtility.SerializeAndDeserialize(originalException);

            Assert.Equal(originalException.StackTrace, deserializedException.StackTrace);
        }

        [Serializable]
        class TestableDoesNotThrowException : DoesNotThrowException
        {
            public TestableDoesNotThrowException(string stackTrace)
                : base(stackTrace) { }

            protected TestableDoesNotThrowException(SerializationInfo info, StreamingContext context)
                : base(info, context) { }
        }
    }
}
