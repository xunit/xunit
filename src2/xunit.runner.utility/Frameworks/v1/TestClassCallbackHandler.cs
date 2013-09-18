using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    public class TestClassCallbackHandler : XmlNodeCallbackHandler
    {
        readonly Dictionary<string, Predicate<XmlNode>> handlers;
        readonly IMessageSink messageSink;
        readonly IList<Xunit1TestCase> testCases;
        readonly RunSummary testCaseResults;

        Xunit1TestCase lastTestCase;

        public TestClassCallbackHandler(IList<Xunit1TestCase> testCases, IMessageSink messageSink)
            : base(lastNodeName: "class")
        {
            this.handlers = new Dictionary<string, Predicate<XmlNode>> { { "class", OnClass }, { "start", OnStart }, { "test", OnTest } };
            this.messageSink = messageSink;
            this.testCases = testCases;
            this.testCaseResults = new RunSummary();

            TestClassResults = new RunSummary();
        }

        public RunSummary TestClassResults { get; private set; }

        Xunit1TestCase FindTestCase(string typeName, string methodName)
        {
            return testCases.FirstOrDefault(tc => tc.Class.Name == typeName && tc.Method.Name == methodName);
        }

        bool OnClass(XmlNode xml)
        {
            SendTestCaseMessagesWhenAppropriate(null);

            TestClassResults.Time = Decimal.Parse(xml.Attributes["time"].Value);
            TestClassResults.Total = Int32.Parse(xml.Attributes["total"].Value);
            TestClassResults.Failed = Int32.Parse(xml.Attributes["failed"].Value);
            TestClassResults.Skipped = Int32.Parse(xml.Attributes["skipped"].Value);
            return true;
        }

        bool OnStart(XmlNode xml)
        {
            var testCase = FindTestCase(xml.Attributes["type"].Value, xml.Attributes["method"].Value);
            SendTestCaseMessagesWhenAppropriate(testCase);
            return messageSink.OnMessage(new TestStarting(testCase, xml.Attributes["name"].Value));
        }

        bool OnTest(XmlNode xml)
        {
            var @continue = true;
            var testCase = FindTestCase(xml.Attributes["type"].Value, xml.Attributes["method"].Value);
            var timeAttribute = xml.Attributes["time"];
            var time = timeAttribute == null ? 0M : Decimal.Parse(timeAttribute.Value);
            var displayName = xml.Attributes["name"].Value;
            ITestCaseMessage resultMessage = null;

            testCaseResults.Total++;
            testCaseResults.Time += time;

            switch (xml.Attributes["result"].Value)
            {
                case "Pass":
                    resultMessage = new TestPassed(testCase, displayName, time);
                    break;

                case "Fail":
                    {
                        testCaseResults.Failed++;
                        var failure = xml.SelectSingleNode("failure");
                        resultMessage = new TestFailed(testCase, displayName, time, failure.Attributes["exception-type"].Value,
                                                       failure.SelectSingleNode("message").InnerText, failure.SelectSingleNode("stack-trace").InnerText);
                        break;
                    }

                case "Skip":
                    testCaseResults.Skipped++;
                    resultMessage = new TestSkipped(testCase, displayName, xml.SelectSingleNode("reason/message").InnerText);
                    break;
            }

            if (resultMessage != null)
                @continue = messageSink.OnMessage(resultMessage) && @continue;

            @continue = messageSink.OnMessage(new TestFinished(testCase, displayName, time)) && @continue;
            return @continue;
        }

        protected override bool OnXmlNode(XmlNode xml)
        {
            Predicate<XmlNode> handler;
            if (handlers.TryGetValue(xml.Name, out handler))
                TestClassResults.Continue = handler(xml) && TestClassResults.Continue;

            return TestClassResults.Continue;
        }

        void SendTestCaseMessagesWhenAppropriate(Xunit1TestCase current)
        {
            if (current != lastTestCase && lastTestCase != null)
            {
                TestClassResults.Continue = messageSink.OnMessage(new TestCaseFinished(lastTestCase, testCaseResults.Time, testCaseResults.Total, testCaseResults.Failed, testCaseResults.Skipped)) && TestClassResults.Continue;
                testCaseResults.Reset();
            }

            lastTestCase = current;

            if (current != null)
                TestClassResults.Continue = messageSink.OnMessage(new TestCaseStarting(current)) && TestClassResults.Continue;
        }
    }
}