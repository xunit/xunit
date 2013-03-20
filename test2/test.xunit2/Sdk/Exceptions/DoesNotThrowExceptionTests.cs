using System;
using System.Runtime.Serialization;
using Xunit;
using Xunit.Sdk;

public class DoesNotThrowExceptionTests
{
    [Fact(Skip = "Serialization is broken...")]
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