using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IMessageSinkWithTypes"/> designed for test discovery for a
    /// single test assembly. The <see cref="Finished"/> event is triggered when discovery is complete.
    /// </summary>
    public class TestDiscoverySink : LongLivedMarshalByRefObject, IMessageSink, IMessageSinkWithTypes
    {
        readonly DiscoveryEventSink discoverySink = new DiscoveryEventSink();

        /// <summary/>
        public TestDiscoverySink()
        {
            discoverySink.TestCaseDiscoveryMessageEvent += args => TestCases.Add(args.Message.TestCase);
            discoverySink.DiscoveryCompleteMessageEvent += args => Finished.Set();
        }

        /// <summary>
        /// The list of discovered test cases.
        /// </summary>
        public List<ITestCase> TestCases { get; } = new List<ITestCase>();

        /// <summary>
        /// Gets an event which is signaled once discovery is finished.
        /// </summary>
        public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

        /// <inheritdoc/>
        public void Dispose()
            => ((IDisposable)Finished).Dispose();

        bool IMessageSink.OnMessage(IMessageSinkMessage message)
            => discoverySink.OnMessageWithTypes(message, MessageSinkAdapter.GetImplementedInterfaces(message));

        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
            => discoverySink.OnMessageWithTypes(message, messageTypes);
    }
}
