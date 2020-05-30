using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Adapts an implementation of <see cref="IMessageSink"/> to provide an implementation
    /// of <see cref="IMessageSinkWithTypes"/> (albeit one without the typical performance
    /// benefits associated with the latter interface).
    /// </summary>
    public class MessageSinkWithTypesAdapter : LongLivedMarshalByRefObject, IMessageSink, IMessageSinkWithTypes
    {
        readonly IMessageSink inner;

        MessageSinkWithTypesAdapter(IMessageSink inner)
        {
            this.inner = inner;
        }

        /// <inheritdoc/>
        public void Dispose() { }    // Assume the thing we're wrapping gets disposed elsewhere

        /// <inheritdoc/>
        public bool OnMessage(IMessageSinkMessage message)
            => inner.OnMessage(message);

        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
            => OnMessage(message);

        /// <summary>
        /// Determines whether the given sink is already an implementation of <see cref="IMessageSinkWithTypes"/>,
        /// and if not, creates a wrapper to adapt it.
        /// </summary>
        /// <param name="sink">The sink to test, and potentially adapt.</param>
        public static IMessageSinkWithTypes Wrap(IMessageSink sink)
            => sink == null ? null : (sink as IMessageSinkWithTypes ?? new MessageSinkWithTypesAdapter(sink));
    }
}
