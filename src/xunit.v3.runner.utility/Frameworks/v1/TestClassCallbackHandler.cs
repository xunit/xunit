#if NETFRAMEWORK

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
        readonly IList<ITestCase> testCases;
        readonly Xunit1RunSummary testCaseResults = new Xunit1RunSummary();
        readonly Xunit1RunSummary testMethodResults = new Xunit1RunSummary();

        ITest currentTest;
        ITestCase lastTestCase;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassCallbackHandler" /> class.
        /// </summary>
        /// <param name="testCases">The test cases that are being run.</param>
        /// <param name="messageSink">The message sink to call with the translated results.</param>
        public TestClassCallbackHandler(IList<ITestCase> testCases, IMessageSink messageSink)
            : base(lastNodeName: "class")
        {
            this.messageSink = messageSink;
            this.testCases = testCases;

            handlers = new Dictionary<string, Predicate<XmlNode>> { { "class", OnClass }, { "start", OnStart }, { "test", OnTest } };

            TestClassResults = new Xunit1RunSummary();
        }

        /// <summary>
        /// Gets the test class results, after the execution has completed.
        /// </summary>
        public Xunit1RunSummary TestClassResults { get; private set; }

        ITestCase FindTestCase(string typeName, string methodName)
        {
            return testCases.FirstOrDefault(tc => tc.TestMethod.TestClass.Class.Name == typeName && tc.TestMethod.Method.Name == methodName);
        }

        bool OnClass(XmlNode xml)
        {
            SendTestCaseMessagesWhenAppropriate(null);

            var @continue = true;
            XmlNode failureNode;
            if ((failureNode = xml.SelectSingleNode("failure")) != null)
            {
                var failureInformation = Xunit1ExceptionUtility.ConvertToFailureInformation(failureNode);

                var errorMessage = new ErrorMessage(testCases,
                                                    failureInformation.ExceptionTypes,
                                                    failureInformation.Messages,
                                                    failureInformation.StackTraces,
                                                    failureInformation.ExceptionParentIndices);
                @continue = messageSink.OnMessage(errorMessage);
            }

            TestClassResults.Time = decimal.Parse(xml.Attributes["time"].Value, CultureInfo.InvariantCulture);
            TestClassResults.Total = int.Parse(xml.Attributes["total"].Value, CultureInfo.InvariantCulture);
            TestClassResults.Failed = int.Parse(xml.Attributes["failed"].Value, CultureInfo.InvariantCulture);
            TestClassResults.Skipped = int.Parse(xml.Attributes["skipped"].Value, CultureInfo.InvariantCulture);
            return @continue && TestClassResults.Continue;
        }

        bool OnStart(XmlNode xml)
        {
            var testCase = FindTestCase(xml.Attributes["type"].Value, xml.Attributes["method"].Value);
            currentTest = new Xunit1Test(testCase, xml.Attributes["name"].Value);
            SendTestCaseMessagesWhenAppropriate(testCase);
            return messageSink.OnMessage(new TestStarting(currentTest)) && TestClassResults.Continue;
        }

        bool OnTest(XmlNode xml)
        {
            var @continue = true;
            var testCase = FindTestCase(xml.Attributes["type"].Value, xml.Attributes["method"].Value);
            var timeAttribute = xml.Attributes["time"];
            var time = timeAttribute == null ? 0M : decimal.Parse(timeAttribute.Value, CultureInfo.InvariantCulture);
            var outputElement = xml.SelectSingleNode("output");
            var output = outputElement == null ? string.Empty : outputElement.InnerText;
            ITestCaseMessage resultMessage = null;

            if (currentTest == null)  // There is no <start> node for skipped tests, or with xUnit prior to v1.1
                currentTest = new Xunit1Test(testCase, xml.Attributes["name"].Value);

            testCaseResults.Total++;
            testCaseResults.Time += time;

            switch (xml.Attributes["result"].Value)
            {
                case "Pass":
                    resultMessage = new TestPassed(currentTest, time, output);
                    break;

                case "Fail":
                    {
                        testCaseResults.Failed++;
                        var failure = xml.SelectSingleNode("failure");
                        var failureInformation = Xunit1ExceptionUtility.ConvertToFailureInformation(failure);
                        resultMessage = new TestFailed(currentTest, time, output,
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
                        @continue = messageSink.OnMessage(new TestStarting(currentTest)) && @continue;
                    }
                    resultMessage = new TestSkipped(currentTest, xml.SelectSingleNode("reason/message").InnerText);
                    break;
            }

            // Since we don't get live output from xUnit.net v1, we just send a single output message just before
            // the result message (if there was any output).
            if (!string.IsNullOrEmpty(output))
                @continue = messageSink.OnMessage(new TestOutput(currentTest, output)) && @continue;

            if (resultMessage != null)
                @continue = messageSink.OnMessage(resultMessage) && @continue;

            @continue = messageSink.OnMessage(new TestFinished(currentTest, time, output)) && @continue;
            currentTest = null;

            return @continue && TestClassResults.Continue;
        }

        /// <inheritdoc/>
        public override bool OnXmlNode(XmlNode node)
        {
            Predicate<XmlNode> handler;
            if (handlers.TryGetValue(node.Name, out handler))
                TestClassResults.Continue = handler(node) && TestClassResults.Continue;

            return TestClassResults.Continue;
        }

        List<ITestCase> GetTestMethodTestCases(ITestMethod testMethod)
        {
            return testCases.Where(tc => tc.TestMethod.Method.Name == testMethod.Method.Name
                                      && tc.TestMethod.TestClass.Class.Name == testMethod.TestClass.Class.Name)
                            .ToList();
        }

        void SendTestCaseMessagesWhenAppropriate(ITestCase current)
        {
            var results = TestClassResults;

            if (current != lastTestCase && lastTestCase != null)
            {
                results.Continue = messageSink.OnMessage(new TestCaseFinished(lastTestCase, testCaseResults.Time, testCaseResults.Total, testCaseResults.Failed, testCaseResults.Skipped)) && results.Continue;
                testMethodResults.Aggregate(testCaseResults);
                testCaseResults.Reset();

                if (current == null || lastTestCase.TestMethod.Method.Name != current.TestMethod.Method.Name)
                {
                    var testMethodTestCases = GetTestMethodTestCases(lastTestCase.TestMethod);

                    results.Continue = messageSink.OnMessage(new TestMethodFinished(testMethodTestCases,
                                                                                    lastTestCase.TestMethod,
                                                                                    testMethodResults.Time,
                                                                                    testMethodResults.Total,
                                                                                    testMethodResults.Failed,
                                                                                    testMethodResults.Skipped)) && results.Continue;
                    testMethodResults.Reset();
                }
            }

            if (current != lastTestCase && current != null)
            {
                if (lastTestCase == null || lastTestCase.TestMethod.Method.Name != current.TestMethod.Method.Name)
                {
                    var testMethodTestCases = GetTestMethodTestCases(current.TestMethod);
                    results.Continue = messageSink.OnMessage(new TestMethodStarting(testMethodTestCases, current.TestMethod)) && results.Continue;
                }

                results.Continue = messageSink.OnMessage(new TestCaseStarting(current)) && results.Continue;
            }

            lastTestCase = current;
        }
    }
}

#endif
