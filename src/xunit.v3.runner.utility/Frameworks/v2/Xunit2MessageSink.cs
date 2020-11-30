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
	/// which adapts and dispatches any incoming v2 messages to the given v3 message sink. It should be
	/// created with <see cref="Xunit2MessageSinkAdapter.Adapt"/>.
	/// </summary>
	public class Xunit2MessageSink : LongLivedMarshalByRefObject, IMessageSink, IMessageSinkWithTypes
	{
		readonly string assemblyUniqueID;
		readonly Func<string, IMessageSinkMessage, HashSet<string>?, _MessageSinkMessage> adapter;
		readonly _IMessageSink v3MessageSink;

		internal Xunit2MessageSink(
			string assemblyUniqueID,
			_IMessageSink v3MessageSink,
			Func<string, IMessageSinkMessage, HashSet<string>?, _MessageSinkMessage>? adapter = null)
		{
			this.assemblyUniqueID = Guard.ArgumentNotNull(nameof(assemblyUniqueID), assemblyUniqueID);
			this.v3MessageSink = Guard.ArgumentNotNull(nameof(v3MessageSink), v3MessageSink);
			this.adapter = adapter ?? Xunit2MessageAdapter.Adapt;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			(v3MessageSink as IDisposable)?.Dispose();
		}

		/// <summary>
		/// Returns the implemented interface types, if known.
		/// </summary>
		/// <param name="message">The message interfaces to retrieve.</param>
		/// <returns>The hash set of interfaces, if known; <c>null</c>, otherwise.</returns>
		public static HashSet<string>? GetImplementedInterfaces(IMessageSinkMessage message)
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

			var v3Message = adapter(assemblyUniqueID, message, messageTypes);
			return v3MessageSink.OnMessage(v3Message);
		}
	}
}
