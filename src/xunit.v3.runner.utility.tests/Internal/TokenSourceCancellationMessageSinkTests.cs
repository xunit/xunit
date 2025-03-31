using System.Threading;
using Xunit;
using Xunit.Internal;
using Xunit.v3;

public class TokenSourceCancellationMessageSinkTests
{
	[Fact]
	public void True_NoCancellation()
	{
		var cts = new CancellationTokenSource();
		var spySink = SpyMessageSink.Create(returnResult: true);
		var sink = new TokenSourceCancellationMessageSink(spySink, cts);

		sink.OnMessage(new DiagnosticMessage("message"));

		Assert.False(cts.IsCancellationRequested);
	}

	[Fact]
	public void False_Cancellation()
	{
		var cts = new CancellationTokenSource();
		var spySink = SpyMessageSink.Create(returnResult: false);
		var sink = new TokenSourceCancellationMessageSink(spySink, cts);

		sink.OnMessage(new DiagnosticMessage("message"));

		Assert.True(cts.IsCancellationRequested);
	}
}
