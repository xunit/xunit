using System;
using System.Runtime.Serialization;
using Xunit;
using Xunit.Sdk;

public class XunitExceptionTests
{
    [Fact]
    public void PreservesUserMessage()
    {
        var ex = new XunitException("UserMessage");

        Assert.Equal("UserMessage", ex.UserMessage);
    }

    [Fact]
    public void UserMessageIsTheMessage()
    {
        var ex = new XunitException("UserMessage");

        Assert.Equal(ex.UserMessage, ex.Message);
    }

    [Fact]
    public void SerializesCustomProperties()
    {
        var originalException = new TestableXunitException("User Message", "Stack Trace");

        var deserializedException = SerializationUtility.SerializeAndDeserialize(originalException);

        Assert.Equal(originalException.StackTrace, deserializedException.StackTrace);
        Assert.Equal(originalException.UserMessage, deserializedException.UserMessage);
    }

    [Serializable]
    class TestableXunitException : XunitException
    {
        public TestableXunitException(string userMessage, string stackTrace)
            : base(userMessage, stackTrace) { }

        protected TestableXunitException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}