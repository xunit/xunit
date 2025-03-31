using NSubstitute;
using Xunit;
using Xunit.Internal;
using Xunit.v3;

public class ProcessCancellationMessageSinkTests
{
	[Fact]
	public void True_NoCancellation()
	{
		var callCounter = 0;
		var process = Substitute.For<ITestProcessBase>();
		process.WhenForAnyArgs(p => p.Cancel(true)).Do(_ => callCounter++);
		var spySink = SpyMessageSink.Create(returnResult: true);
		var sink = new ProcessCancellationMessageSink(spySink, process);

		sink.OnMessage(new DiagnosticMessage("message"));

		Assert.Equal(0, callCounter);
	}

	[Fact]
	public void False_Cancellation()
	{
		var callCounter = 0;
		var process = Substitute.For<ITestProcessBase>();
		process.WhenForAnyArgs(p => p.Cancel(true)).Do(_ => callCounter++);
		var spySink = SpyMessageSink.Create(returnResult: false);
		var sink = new ProcessCancellationMessageSink(spySink, process);

		sink.OnMessage(new DiagnosticMessage("message"));

		Assert.Equal(1, callCounter);
	}
}
