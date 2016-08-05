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
    public class TestDiscoverySink : TestMessageSink, IDisposable
    {
        /// <summary/>
        public TestDiscoverySink()
        {
            TestCaseDiscoveryMessageEvent += HandleTestCaseDiscoveryMessage;
            DiscoveryCompleteMessageEvent += HandleDiscoveryCompleteMessage;
        }

        /// <summary>
        /// The list of discovered test cases.
        /// </summary>
        public List<ITestCase> TestCases { get; } = new List<ITestCase>();

        /// <summary>
        /// Gets an event which is signaled once discovery is finished.
        /// </summary>
        public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

        /// <summary>
        /// Called when <see cref="TestMessageSink.TestCaseDiscoveryMessageEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestCaseDiscoveryMessage(MessageHandlerArgs<ITestCaseDiscoveryMessage> args)
            => TestCases.Add(args.Message.TestCase);

        /// <summary>
        /// Called when <see cref="TestMessageSink.DiscoveryCompleteMessageEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleDiscoveryCompleteMessage(MessageHandlerArgs<IDiscoveryCompleteMessage> args)
            => Finished.Set();

        /// <inheritdoc/>
        public void Dispose()
            => ((IDisposable)Finished).Dispose();
    }
}
