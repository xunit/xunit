using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public class DelegatingExecutionSummarySinkTests
{
    readonly IMessageSinkWithTypes innerSink;
    readonly IMessageSinkMessage testMessage;

    public DelegatingExecutionSummarySinkTests()
    {
        innerSink = Substitute.For<IMessageSinkWithTypes>();
        innerSink.OnMessageWithTypes(null, null).ReturnsForAnyArgs(true);

        testMessage = Substitute.For<IMessageSinkMessage>();
    }

    public class Cancellation : DelegatingExecutionSummarySinkTests
    {
        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var sink = new DelegatingExecutionSummarySink(innerSink, () => true);

            var result = sink.OnMessage(testMessage);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var sink = new DelegatingExecutionSummarySink(innerSink, () => false);

            var result = sink.OnMessage(testMessage);

            Assert.True(result);
        }
    }
}
