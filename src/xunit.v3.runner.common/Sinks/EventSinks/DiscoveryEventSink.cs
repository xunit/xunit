using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Class that maps test framework discovery messages to events.
	/// </summary>
	public class DiscoveryEventSink : _IMessageSink
	{
		/// <summary>
		/// Occurs when a <see cref="_DiscoveryComplete"/> message is received.
		/// </summary>
		public event MessageHandler<_DiscoveryComplete>? DiscoveryCompleteMessageEvent;

		/// <summary>
		/// Occurs when a <see cref="_DiscoveryStarting"/> message is received.
		/// </summary>
		public event MessageHandler<_DiscoveryStarting>? DiscoveryStartingMessageEvent;

		/// <summary>
		/// Occurs when a <see cref="ITestCaseDiscoveryMessage"/> message is received.
		/// </summary>
		public event MessageHandler<ITestCaseDiscoveryMessage>? TestCaseDiscoveryMessageEvent;

		/// <inheritdoc/>
		public bool OnMessage(IMessageSinkMessage message)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			return
				message.Dispatch(null, TestCaseDiscoveryMessageEvent) &&
				message.Dispatch(null, DiscoveryCompleteMessageEvent) &&
				message.Dispatch(null, DiscoveryStartingMessageEvent);
		}
	}
}
