#pragma warning disable CA1003 // The properties here are not intended to be .NET events

using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// Class that maps test framework discovery messages to events.
/// </summary>
public class DiscoveryEventSink : _IMessageSink
{
	/// <summary>
	/// Occurs when a <see cref="_DiscoveryComplete"/> message is received.
	/// </summary>
	public event MessageHandler<_DiscoveryComplete>? DiscoveryCompleteEvent;

	/// <summary>
	/// Occurs when a <see cref="_DiscoveryStarting"/> message is received.
	/// </summary>
	public event MessageHandler<_DiscoveryStarting>? DiscoveryStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="_TestCaseDiscovered"/> message is received.
	/// </summary>
	public event MessageHandler<_TestCaseDiscovered>? TestCaseDiscoveredEvent;

	/// <inheritdoc/>
	public bool OnMessage(_MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		return
			message.DispatchWhen(TestCaseDiscoveredEvent) &&
			message.DispatchWhen(DiscoveryCompleteEvent) &&
			message.DispatchWhen(DiscoveryStartingEvent);
	}
}
