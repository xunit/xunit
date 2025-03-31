using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Internal;

// Wraps a message sink to signal process cancellation when OnMessage returns false

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public sealed class ProcessCancellationMessageSink(
	IMessageSink innerSink,
	ITestProcessBase testProcess) :
		IMessageSink
{
	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		var result = innerSink.OnMessage(message);
		if (!result)
			testProcess.Cancel(forceCancellation: false);

		return result;
	}
}
