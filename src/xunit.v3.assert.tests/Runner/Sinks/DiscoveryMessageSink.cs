using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit
{
    public class DiscoveryMessageSink : IMessageSink
    {
        private readonly Func<bool> cancelThunk;

        public DiscoveryMessageSink(Func<bool> cancelThunk = null)
        {
            this.cancelThunk = cancelThunk ?? (() => false);
        }

        /// <summary>
        /// The list of discovered test cases.
        /// </summary>
        public List<ITestCase> TestCases { get; } = new List<ITestCase>();

        /// <summary>
        /// Gets an event which is signaled once discovery is finished.
        /// </summary>
        public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

        public bool OnMessage(IMessageSinkMessage message)
        {
            if (message is ITestCaseDiscoveryMessage discoveryMessage)
            {
                TestCases.Add(discoveryMessage.TestCase);
            }
            else if (message is IDiscoveryCompleteMessage discoveryCompleteMessage)
            {
                Finished.Set();
            }

            return !cancelThunk();
        }
    }
}
