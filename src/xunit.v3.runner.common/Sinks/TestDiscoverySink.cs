using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IMessageSinkWithTypes"/> designed for test discovery for a
    /// single test assembly. The <see cref="Finished"/> event is triggered when discovery is complete.
    /// </summary>
    public class TestDiscoverySink : LongLivedMarshalByRefObject, IMessageSink, IMessageSinkWithTypes
    {
        readonly Func<bool> cancelThunk;
        readonly DiscoveryEventSink discoverySink = new DiscoveryEventSink();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDiscoverySink"/> class.
        /// </summary>
        /// <param name="cancelThunk">An optional thunk which can be used to control cancellation.</param>
        public TestDiscoverySink(Func<bool> cancelThunk = null)
        {
            this.cancelThunk = cancelThunk ?? (() => false);

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
            => OnMessageWithTypes(message, MessageSinkAdapter.GetImplementedInterfaces(message));

        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
            => discoverySink.OnMessageWithTypes(message, messageTypes)
            && !cancelThunk();
    }
}
