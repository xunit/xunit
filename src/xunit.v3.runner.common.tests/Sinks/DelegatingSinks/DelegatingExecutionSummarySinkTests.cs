using NSubstitute;
using Xunit;
using Xunit.Runner.Common;
using Xunit.v3;

public class DelegatingExecutionSummarySinkTests
{
	readonly _IMessageSink innerSink;
	readonly _MessageSinkMessage testMessage;

	public DelegatingExecutionSummarySinkTests()
	{
		innerSink = Substitute.For<_IMessageSink>();
		innerSink.OnMessage(null!).ReturnsForAnyArgs(true);

		testMessage = new _MessageSinkMessage();
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
