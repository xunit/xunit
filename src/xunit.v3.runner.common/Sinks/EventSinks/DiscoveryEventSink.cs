#pragma warning disable CA1003 // The properties here are not intended to be .NET events

using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Class that maps test framework discovery messages to events.
/// </summary>
public class DiscoveryEventSink : IMessageSink
{
	/// <summary>
	/// Occurs when a <see cref="DiscoveryComplete"/> message is received.
	/// </summary>
	public event MessageHandler<DiscoveryComplete>? DiscoveryCompleteEvent;

	/// <summary>
	/// Occurs when a <see cref="DiscoveryStarting"/> message is received.
	/// </summary>
	public event MessageHandler<DiscoveryStarting>? DiscoveryStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="TestCaseDiscovered"/> message is received.
	/// </summary>
	public event MessageHandler<TestCaseDiscovered>? TestCaseDiscoveredEvent;

	/// <inheritdoc/>
	public bool OnMessage(MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		return
			message.DispatchWhen(TestCaseDiscoveredEvent) &&
			message.DispatchWhen(DiscoveryCompleteEvent) &&
			message.DispatchWhen(DiscoveryStartingEvent);
	}
}
