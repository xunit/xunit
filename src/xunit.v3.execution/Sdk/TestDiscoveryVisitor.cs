using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    class TestDiscoveryVisitor : IMessageSink, IDisposable
    {
        public TestDiscoveryVisitor()
        {
            Finished = new ManualResetEvent(initialState: false);
            TestCases = new List<ITestCase>();
        }

        public ManualResetEvent Finished { get; }

        public List<ITestCase> TestCases { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Finished.Dispose();
        }

        /// <inheritdoc/>
        public bool OnMessage(IMessageSinkMessage message)
        {
            if (message is ITestCaseDiscoveryMessage discoveryMessage)
                TestCases.Add(discoveryMessage.TestCase);

            if (message is IDiscoveryCompleteMessage)
                Finished.Set();

            return true;
        }
    }
}
