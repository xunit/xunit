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
	/// Occurs when a <see cref="IDiscoveryComplete"/> message is received.
	/// </summary>
	public event MessageHandler<IDiscoveryComplete>? DiscoveryCompleteEvent;

	/// <summary>
	/// Occurs when a <see cref="IDiscoveryStarting"/> message is received.
	/// </summary>
	public event MessageHandler<IDiscoveryStarting>? DiscoveryStartingEvent;

	/// <summary>
	/// Occurs when a <see cref="ITestCaseDiscovered"/> message is received.
	/// </summary>
	public event MessageHandler<ITestCaseDiscovered>? TestCaseDiscoveredEvent;

	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		return
			message.DispatchWhen(TestCaseDiscoveredEvent) &&
			message.DispatchWhen(DiscoveryCompleteEvent) &&
			message.DispatchWhen(DiscoveryStartingEvent);
	}
}
