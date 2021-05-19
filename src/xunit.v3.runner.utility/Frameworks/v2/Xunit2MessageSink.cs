using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// A message sink which implements both <see cref="IMessageSink"/> and <see cref="IMessageSinkWithTypes"/>
	/// which adapts (with <see cref="Xunit2MessageAdapter"/> and dispatches any incoming v2 messages to the
	/// given v3 message sink.
	/// </summary>
	public class Xunit2MessageSink : LongLivedMarshalByRefObject, IMessageSink, IMessageSinkWithTypes
	{
		readonly Xunit2MessageAdapter adapter;
		readonly _IMessageSink v3MessageSink;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit2MessageSink"/> class.
		/// </summary>
		/// <param name="v3MessageSink">The v3 message sink to which to report the messages</param>
		/// <param name="assemblyUniqueID">The unique ID of the assembly these message belong to</param>
		/// <param name="discoverer">The discoverer used to serialize test cases</param>
		public Xunit2MessageSink(
			_IMessageSink v3MessageSink,
			string? assemblyUniqueID = null,
			ITestFrameworkDiscoverer? discoverer = null)
		{
			this.v3MessageSink = Guard.ArgumentNotNull(nameof(v3MessageSink), v3MessageSink);

			adapter = new Xunit2MessageAdapter(assemblyUniqueID, discoverer);
		}


		/// <inheritdoc/>
		public void Dispose()
		{
			(v3MessageSink as IDisposable)?.Dispose();
		}

		static HashSet<string>? GetImplementedInterfaces(IMessageSinkMessage message)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			if (message is IMessageSinkMessageWithTypes messageWithTypes)
				return messageWithTypes.InterfaceTypes;

#if NETFRAMEWORK
			// Can't get the list of interfaces across the remoting boundary
			if (System.Runtime.Remoting.RemotingServices.IsTransparentProxy(message))
				return null;
#endif

			var result = new HashSet<string>(message.GetType().GetInterfaces().Select(i => i.FullName!), StringComparer.OrdinalIgnoreCase);

			// TODO: Hack this to include the concrete type, while we transition from v2 to v3 so that we
			// can support our new message types which aren't interfaces.
			result.Add(message.GetType().FullName!);

			return result;
		}

		/// <inheritdoc/>
		public bool OnMessage(IMessageSinkMessage message)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			return OnMessageWithTypes(message, GetImplementedInterfaces(message));
		}

		/// <inheritdoc/>
		public bool OnMessageWithTypes(
			IMessageSinkMessage message,
			HashSet<string>? messageTypes)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			var v3Message = adapter.Adapt(message, messageTypes);
			return v3MessageSink.OnMessage(v3Message);
		}
	}
}
