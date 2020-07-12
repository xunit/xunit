using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
using System.Collections.Generic;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit
#endif
{
#if XUNIT_FRAMEWORK
    /// <summary>
    /// An implementation of <see cref="IMessageSink"/> that ignores all messages.
    /// </summary>
    public class NullMessageSink : IMessageSink
#else
    /// <summary>
    /// An implementation of <see cref="IMessageSink"/> and <see cref="IMessageSinkWithTypes"/>
    /// that ignores all messages.
    /// </summary>
    public class NullMessageSink : LongLivedMarshalByRefObject, IMessageSink, IMessageSinkWithTypes
#endif
    {
        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public bool OnMessage(IMessageSinkMessage message) => true;

#if !XUNIT_FRAMEWORK
        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string>? messageTypes) => true;
#endif
    }
}
