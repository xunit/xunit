using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace test.harness.uwp
{
    class MockTestDiscoveryEventHandler : ITestDiscoveryEventsHandler2
    {
        public void HandleDiscoveredTests(IEnumerable<TestCase> discoveredTestCases)
        {
            
        }

        public void HandleDiscoveryComplete(DiscoveryCompleteEventArgs discoveryCompleteEventArgs, IEnumerable<TestCase> lastChunk)
        {
            
        }

        public void HandleLogMessage(TestMessageLevel level, string message)
        {
            
        }

        public void HandleRawMessage(string rawMessage)
        {
            
        }
    }
}
