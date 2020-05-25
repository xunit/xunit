using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Adapts an implementation of <see cref="IMessageSinkWithTypes"/> to provide an implementation
    /// of <see cref="IMessageSink"/>.
    /// </summary>
    public class MessageSinkAdapter : LongLivedMarshalByRefObject, IMessageSink, IMessageSinkWithTypes
    {
        readonly IMessageSinkWithTypes inner;

        MessageSinkAdapter(IMessageSinkWithTypes inner)
        {
            this.inner = inner;
        }

        /// <inheritdoc/>
        public void Dispose() { }    // Assume the thing we're wrapping gets disposed elsewhere

        /// <summary>
        /// Returns the implemented interface types, if known.
        /// </summary>
        /// <param name="message">The message interfaces to retrieve.</param>
        /// <returns>The hash set of interfaces, if known; <c>null</c>, otherwise.</returns>
        public static HashSet<string> GetImplementedInterfaces(IMessageSinkMessage message)
        {
            if (message is IMessageSinkMessageWithTypes messageWithTypes)
                return messageWithTypes.InterfaceTypes;

#if NETFRAMEWORK
            // Can't get the list of interfaces across the remoting boundary
            if (System.Runtime.Remoting.RemotingServices.IsTransparentProxy(message))
                return null;
#endif

            return new HashSet<string>(message.GetType().GetInterfaces().Select(i => i.FullName), StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public bool OnMessage(IMessageSinkMessage message)
            => OnMessageWithTypes(message, GetImplementedInterfaces(message));

        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
            => inner.OnMessageWithTypes(message, messageTypes);

        /// <summary>
        /// Determines whether the given sink is already an implementation of <see cref="IMessageSink"/>,
        /// and if not, creates a wrapper to adapt it.
        /// </summary>
        /// <param name="sink">The sink to test, and potentially adapt.</param>
        public static IMessageSink Wrap(IMessageSinkWithTypes sink)
            => sink == null ? null : (sink as IMessageSink ?? new MessageSinkAdapter(sink));
    }
}
