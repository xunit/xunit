using System;
using System.Runtime.Serialization;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class ThrowsExceptionTests
    {
        [Fact]
        public void SerializesCustomProperties()
        {
            var originalException = new TestableThrowsException("Stack Trace");

            var deserializedException = SerializationUtility.SerializeAndDeserialize(originalException);

            Assert.Equal(originalException.StackTrace, deserializedException.StackTrace);
        }

        [Serializable]
        class TestableThrowsException : ThrowsException
        {
            public TestableThrowsException(string stackTrace)
                : base(typeof(object), "Actual", "Actual Message", stackTrace) { }

            protected TestableThrowsException(SerializationInfo info, StreamingContext context)
                : base(info, context) { }
        }
    }
}
