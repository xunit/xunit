using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// This class wraps an instance of <see cref="IMessageSink"/>, and whenever it returns <c>false </c> from
/// <see cref="IMessageSink.OnMessage"/>, it request cancellation from the <paramref name="testProcess"/>.
/// </summary>
/// <param name="testProcess">The test process</param>
/// <param name="innerSink">The inner sink that's being wrapped</param>
internal sealed class DelegatingProcessCancellationSink(
	ITestProcess testProcess,
	IMessageSink? innerSink) :
		IMessageSink
{
	public bool OnMessage(IMessageSinkMessage message)
	{
		var @continue = innerSink?.OnMessage(message) ?? true;
		if (!@continue)
			testProcess.Cancel(forceCancellation: false);

		return @continue;
	}
}
