using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    class TestDiscoveryVisitor : LongLivedMarshalByRefObject, IMessageSink, IDisposable
    {
        public TestDiscoveryVisitor()
        {
            Finished = new ManualResetEvent(false);
            TestCases = new List<ITestCase>();
        }

        /// <summary>
        /// This event is triggered when the completion message has been seen.
        /// </summary>
        public ManualResetEvent Finished { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            ((IDisposable)Finished).Dispose();
        }

        public List<ITestCase> TestCases { get; private set; }

        /// <inheritdoc/>
        public virtual bool OnMessage(IMessageSinkMessage message)
        {
            var discoveryMessage = message as ITestCaseDiscoveryMessage;
            if (discoveryMessage != null)
            {
                return Visit(discoveryMessage);
            }

            var completeMessage = message as IDiscoveryCompleteMessage;
            if (message is IDiscoveryCompleteMessage)
            {
                var result = Visit(completeMessage);
                Finished.Set();
                return result;
            }

            return true;
        }

        protected virtual bool Visit(IDiscoveryCompleteMessage completeMessage)
        {
            return true;
        }

        protected virtual bool Visit(ITestCaseDiscoveryMessage testCaseDiscovered)
        {
            TestCases.Add(testCaseDiscovered.TestCase);
            return true;
        }
    }
}