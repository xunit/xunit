using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Class that maps test framework discovery messages to events.
    /// </summary>
    public class DiscoveryEventSink : LongLivedMarshalByRefObject, IMessageSinkWithTypes
    {
        /// <summary>
        /// Occurs when a <see cref="IDiscoveryCompleteMessage"/> message is received.
        /// </summary>
        public event MessageHandler<IDiscoveryCompleteMessage> DiscoveryCompleteMessageEvent;

        /// <summary>
        /// Occurs when a <see cref="ITestCaseDiscoveryMessage"/> message is received.
        /// </summary>
        public event MessageHandler<ITestCaseDiscoveryMessage> TestCaseDiscoveryMessageEvent;

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> typeNames)
            => message.Dispatch(typeNames, TestCaseDiscoveryMessageEvent)
            && message.Dispatch(typeNames, DiscoveryCompleteMessageEvent);
    }
}
