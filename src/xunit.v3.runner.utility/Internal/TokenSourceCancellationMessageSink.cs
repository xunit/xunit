using System.Threading;
using Xunit.Sdk;

namespace Xunit.Internal;

// Wraps a message sink to trigger a cancellation token source if we get back a message sink result
// that indicates we should cancel

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public sealed class TokenSourceCancellationMessageSink(
	IMessageSink innerSink,
	CancellationTokenSource cancellationTokenSource) :
		IMessageSink
{
	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		var result = innerSink.OnMessage(message);
		if (!result)
			cancellationTokenSource.Cancel();

		return result;
	}
}
