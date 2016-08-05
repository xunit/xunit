using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Adapts an implementation of <see cref="IMessageSink"/> to provide an implementation
    /// of <see cref="IMessageSinkWithTypes"/> (albeit one without the typical performance
    /// benefits associated with the latter interface).
    /// </summary>
    public class MessageSinkWithTypesAdapter : LongLivedMarshalByRefObject, IMessageSinkWithTypes
    {
        readonly IMessageSink inner;

        MessageSinkWithTypesAdapter(IMessageSink inner)
        {
            this.inner = inner;
        }

        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, string[] messageTypes)
        {
            return inner.OnMessage(message);
        }

        bool IMessageSink.OnMessage(IMessageSinkMessage message)
        {
            return OnMessageWithTypes(message, null);
        }

        /// <summary>
        /// Determines whether the given sink is already an implementation of <see cref="IMessageSinkWithTypes"/>,
        /// and if not, creates a wrapper to adapt it.
        /// </summary>
        /// <param name="sink">The sink to test, and potentially adapt.</param>
        public static IMessageSinkWithTypes Wrap(IMessageSink sink)
            => sink as IMessageSinkWithTypes ?? new MessageSinkWithTypesAdapter(sink);
    }
}
