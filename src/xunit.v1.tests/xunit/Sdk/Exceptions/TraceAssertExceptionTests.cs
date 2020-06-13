using System;
using System.Diagnostics;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class TraceAssertExceptionTests
    {
        [Fact]
        public void TraceAssertFailureWithFullDetails()
        {
            TraceAssertException ex = Assert.Throws<TraceAssertException>(() => Trace.Assert(false, "message", "detailed message"));

            Assert.Equal("message", ex.AssertMessage);
            Assert.Equal("detailed message", ex.AssertDetailedMessage);
            Assert.Equal("Debug.Assert() Failure : message" + Environment.NewLine + "Detailed Message:" + Environment.NewLine + "detailed message", ex.Message);
        }

        [Fact]
        public void TraceAssertFailureWithNoDetailedMessage()
        {
            TraceAssertException ex = Assert.Throws<TraceAssertException>(() => Trace.Assert(false, "message"));

            Assert.Equal("message", ex.AssertMessage);
            Assert.Equal("", ex.AssertDetailedMessage);
            Assert.Equal("Debug.Assert() Failure : message", ex.Message);
        }

        [Fact]
        public void TraceAssertFailureWithNoMessage()
        {
            if (!IsRunningOnMono()) // Mono does "non-standard" things with the message when it's empty
            {
                TraceAssertException ex = Assert.Throws<TraceAssertException>(() => Trace.Assert(false));

                Assert.Equal("", ex.AssertMessage);

                Assert.Equal("", ex.AssertDetailedMessage);
                Assert.Equal("Debug.Assert() Failure", ex.Message);
            }
        }

        bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        [Fact]
        public void SerializesCustomProperties()
        {
            var originalException = new TraceAssertException("Assert Message", "Detailed Message");

            var deserializedException = SerializationUtility.SerializeAndDeserialize(originalException);

            Assert.Equal(originalException.AssertMessage, deserializedException.AssertMessage);
            Assert.Equal(originalException.AssertDetailedMessage, deserializedException.AssertDetailedMessage);
        }
    }
}
