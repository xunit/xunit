using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Adapts an implementation of <see cref="IMessageSinkWithTypes"/> to provide an implementation
	/// of <see cref="IMessageSink"/>.
	/// </summary>
	public class MessageSinkAdapter : LongLivedMarshalByRefObject, IMessageSink, IMessageSinkWithTypes, _IMessageSink
	{
		readonly IMessageSinkWithTypes inner;

		MessageSinkAdapter(IMessageSinkWithTypes inner)
		{
			this.inner = inner;
		}

		/// <inheritdoc/>
		public void Dispose()  // Assume the thing we're wrapping gets disposed elsewhere
		{ }

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
			Guard.ArgumentNotNull(nameof(messageTypes), messageTypes);

			return inner.OnMessageWithTypes(message, messageTypes);
		}

		/// <summary>
		/// Determines whether the given sink is already an implementation of <see cref="IMessageSink"/>,
		/// and if not, creates a wrapper to adapt it.
		/// </summary>
		/// <param name="sink">The sink to test, and potentially adapt.</param>
		public static IMessageSink Wrap(IMessageSinkWithTypes sink)
		{
			Guard.ArgumentNotNull(nameof(sink), sink);

			return sink as IMessageSink ?? new MessageSinkAdapter(sink);
		}

		/// <summary>
		/// Determines whether the given sink is already an implementation of <see cref="IMessageSink"/>,
		/// and if not, creates a wrapper to adapt it.
		/// </summary>
		/// <param name="sink">The sink to test, and potentially adapt.</param>
		// TODO: This should be temporary, once we move all to v3 the wrapping will go away.
		public static _IMessageSink WrapV3(IMessageSinkWithTypes sink)
		{
			Guard.ArgumentNotNull(nameof(sink), sink);

			return sink as _IMessageSink ?? new MessageSinkAdapter(sink);
		}

		/// <summary>
		/// Determines whether the given sink is already an implementation of <see cref="IMessageSink"/>,
		/// and if not, creates a wrapper to adapt it.
		/// </summary>
		/// <param name="sink">The sink to test, and potentially adapt.</param>
		public static IMessageSink? WrapMaybeNull(IMessageSinkWithTypes? sink) =>
			sink == null ? null : (sink as IMessageSink ?? new MessageSinkAdapter(sink));
	}
}
