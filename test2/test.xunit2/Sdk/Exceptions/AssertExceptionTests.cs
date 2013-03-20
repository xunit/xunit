using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xunit;
using Xunit.Sdk;

public class AssertExceptionTests
{
    [Fact]
    public void PreservesUserMessage()
    {
        AssertException ex = new AssertException("UserMessage");

        Assert.Equal("UserMessage", ex.UserMessage);
    }

    [Fact]
    public void UserMessageIsTheMessage()
    {
        AssertException ex = new AssertException("UserMessage");

        Assert.Equal(ex.UserMessage, ex.Message);
    }

    [Fact]
    public void SerializesCustomProperties()
    {
        var originalException = new TestableAssertException("User Message", "Stack Trace");

        var deserializedException = SerializationUtility.SerializeAndDeserialize(originalException);

        Assert.Equal(originalException.StackTrace, deserializedException.StackTrace);
        Assert.Equal(originalException.UserMessage, deserializedException.UserMessage);
    }

    [Serializable]
    class TestableAssertException : AssertException
    {
        public TestableAssertException(string userMessage, string stackTrace)
            : base(userMessage, stackTrace) { }

        protected TestableAssertException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}