using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// An implementation of <see cref="IMessageSink"/> and <see cref="T:Xunit.IMessageSinkWithTypes"/> that
    /// ignores all messages.
    /// </summary>
    public class NullMessageSink : LongLivedMarshalByRefObject, IMessageSink
#if !XUNIT_FRAMEWORK
        , IMessageSinkWithTypes
#endif
    {
        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public bool OnMessage(IMessageSinkMessage message)
            => true;

        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
            => true;
    }
}
