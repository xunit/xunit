using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// A handler that dispatches v1 Executor messages from running a test class.
    /// </summary>
    public class TestClassCallbackHandler : XmlNodeCallbackHandler
    {
        readonly Dictionary<string, Predicate<XmlNode>> handlers;
        readonly IMessageSink messageSink;
        readonly IList<Xunit1TestCase> testCases;
        readonly Xunit1RunSummary testCaseResults;
        readonly Xunit1RunSummary testMethodResults;

        Xunit1TestCase lastTestCase;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassCallbackHandler" /> class.
        /// </summary>
        /// <param name="testCases">The test cases that are being run.</param>
        /// <param name="messageSink">The message sink to call with the translated results.</param>
        public TestClassCallbackHandler(IList<Xunit1TestCase> testCases, IMessageSink messageSink)
            : base(lastNodeName: "class")
        {
            this.messageSink = messageSink;
            this.testCases = testCases;

            handlers = new Dictionary<string, Predicate<XmlNode>> { { "class", OnClass }, { "start", OnStart }, { "test", OnTest } };
            testCaseResults = new Xunit1RunSummary();
            testMethodResults = new Xunit1RunSummary();
            TestClassResults = new Xunit1RunSummary();
        }

        /// <summary>
        /// Gets the test class results, after the execution has completed.
        /// </summary>
        public Xunit1RunSummary TestClassResults { get; private set; }

        Xunit1TestCase FindTestCase(string typeName, string methodName)
        {
            return testCases.FirstOrDefault(tc => tc.Class.Name == typeName && tc.Method.Name == methodName);
        }

        bool OnClass(XmlNode xml)
        {
            SendTestCaseMessagesWhenAppropriate(null);

            var @continue = true;
            XmlNode failureNode;
            if ((failureNode = xml.SelectSingleNode("failure")) != null)
            {
                var failureInformation = Xunit1ExceptionUtility.ConvertToFailureInformation(failureNode);

                var errorMessage = new ErrorMessage(failureInformation.ExceptionTypes,
                                                    failureInformation.Messages,
                                                    failureInformation.StackTraces,
                                                    failureInformation.ExceptionParentIndices);
                @continue = messageSink.OnMessage(errorMessage);
            }

            TestClassResults.Time = Decimal.Parse(xml.Attributes["time"].Value, CultureInfo.InvariantCulture);
            TestClassResults.Total = Int32.Parse(xml.Attributes["total"].Value, CultureInfo.InvariantCulture);
            TestClassResults.Failed = Int32.Parse(xml.Attributes["failed"].Value, CultureInfo.InvariantCulture);
            TestClassResults.Skipped = Int32.Parse(xml.Attributes["skipped"].Value, CultureInfo.InvariantCulture);
            return @continue && TestClassResults.Continue;
        }

        bool OnStart(XmlNode xml)
        {
            var testCase = FindTestCase(xml.Attributes["type"].Value, xml.Attributes["method"].Value);
            SendTestCaseMessagesWhenAppropriate(testCase);
            return messageSink.OnMessage(new TestStarting(testCase, xml.Attributes["name"].Value)) && TestClassResults.Continue;
        }

        bool OnTest(XmlNode xml)
        {
            var @continue = true;
            var testCase = FindTestCase(xml.Attributes["type"].Value, xml.Attributes["method"].Value);
            var timeAttribute = xml.Attributes["time"];
            var time = timeAttribute == null ? 0M : Decimal.Parse(timeAttribute.Value, CultureInfo.InvariantCulture);
            var outputElement = xml.SelectSingleNode("output");
            var output = outputElement == null ? String.Empty : outputElement.InnerText;
            var displayName = xml.Attributes["name"].Value;
            ITestCaseMessage resultMessage = null;

            testCaseResults.Total++;
            testCaseResults.Time += time;

            switch (xml.Attributes["result"].Value)
            {
                case "Pass":
                    resultMessage = new TestPassed(testCase, displayName, time, output);
                    break;

                case "Fail":
                    {
                        testCaseResults.Failed++;
                        var failure = xml.SelectSingleNode("failure");
                        var failureInformation = Xunit1ExceptionUtility.ConvertToFailureInformation(failure);
                        resultMessage = new TestFailed(testCase, displayName, time, output,
                                                       failureInformation.ExceptionTypes,
                                                       failureInformation.Messages,
                                                       failureInformation.StackTraces,
                                                       failureInformation.ExceptionParentIndices);
                        break;
                    }

                case "Skip":
                    testCaseResults.Skipped++;
                    if (testCase != lastTestCase)
                    {
                        SendTestCaseMessagesWhenAppropriate(testCase);
                        @continue = messageSink.OnMessage(new TestStarting(testCase, displayName)) && @continue;
                    }
                    resultMessage = new TestSkipped(testCase, displayName, xml.SelectSingleNode("reason/message").InnerText);
                    break;
            }

            if (resultMessage != null)
                @continue = messageSink.OnMessage(resultMessage) && @continue;

            @continue = messageSink.OnMessage(new TestFinished(testCase, displayName, time, output)) && @continue;
            return @continue && TestClassResults.Continue;
        }

        /// <inheritdoc/>
        public override bool OnXmlNode(XmlNode xml)
        {
            Predicate<XmlNode> handler;
            if (handlers.TryGetValue(xml.Name, out handler))
                TestClassResults.Continue = handler(xml) && TestClassResults.Continue;

            return TestClassResults.Continue;
        }

        void SendTestCaseMessagesWhenAppropriate(Xunit1TestCase current)
        {
            var results = TestClassResults;

            if (current != lastTestCase && lastTestCase != null)
            {
                results.Continue = messageSink.OnMessage(new TestCaseFinished(lastTestCase, testCaseResults.Time, testCaseResults.Total, testCaseResults.Failed, testCaseResults.Skipped)) && results.Continue;
                testMethodResults.Aggregate(testCaseResults);
                testCaseResults.Reset();

                if (current == null || lastTestCase.Method.Name != current.Class.Name)
                {
                    results.Continue = messageSink.OnMessage(new TestMethodFinished(lastTestCase.TestCollection, lastTestCase.Class.Name, lastTestCase.Method.Name, testMethodResults.Time, testMethodResults.Total, testMethodResults.Failed, testMethodResults.Skipped)) && results.Continue;
                    testMethodResults.Reset();
                }
            }

            if (current != null)
            {
                if (lastTestCase == null || lastTestCase.Method.Name != current.Method.Name)
                    results.Continue = messageSink.OnMessage(new TestMethodStarting(current.TestCollection, current.Class.Name, current.Method.Name)) && results.Continue;

                results.Continue = messageSink.OnMessage(new TestCaseStarting(current)) && results.Continue;
            }

            lastTestCase = current;
        }
    }
}