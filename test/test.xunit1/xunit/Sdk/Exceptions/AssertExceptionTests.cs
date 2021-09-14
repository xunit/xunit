using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
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

#if !DEBUG
        [Fact]
        public void DeveloperCanChooseWhichStackFrameItemsToExclude()
        {
            CustomException ex = Assert.Throws<CustomException>(() => { throw new CustomException(); });

            string stackTrace = ex.StackTrace;

            Assert.Empty(stackTrace);  // Everything was filtered out in our exception
            Assert.Equal(2, ex.StackFrames.Count);
            Assert.Contains("at Xunit1.AssertExceptionTests", ex.StackFrames[0]);
            Assert.Contains("at Xunit.Record.Exception", ex.StackFrames[1]);
        }
#endif

        class CustomException : AssertException
        {
            public List<string> StackFrames = new List<string>();

            protected override bool ExcludeStackFrame(string stackFrame)
            {
                StackFrames.Add(stackFrame);
                return true;
            }
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
}
