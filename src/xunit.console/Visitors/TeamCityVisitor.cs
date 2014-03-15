using System;
using System.Collections.Concurrent;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace Xunit.ConsoleClient
{
    public class TeamCityVisitor : XmlTestExecutionVisitor
    {
        readonly ConcurrentDictionary<string, string> flowMappings = new ConcurrentDictionary<string, string>();
        readonly Func<string, string> flowIdMapper;

        public TeamCityVisitor(XElement assembliesElement, Func<bool> cancelThunk)
            : this(assembliesElement, cancelThunk, _ => Guid.NewGuid().ToString("N")) { }

        public TeamCityVisitor(XElement assembliesElement, Func<bool> cancelThunk, Func<string, string> flowIdMapper)
            : base(assembliesElement, cancelThunk)
        {
            this.flowIdMapper = flowIdMapper;
        }

        void LogFinish(ITestResultMessage testResult)
        {
            Console.WriteLine("##teamcity[testFinished name='{0}' duration='{1}' flowId='{2}']",
                              TeamCityEscape(testResult.TestDisplayName),
                              (int)(testResult.ExecutionTime * 1000M),
                              ToFlowId(testResult.TestCollection.DisplayName));
        }

        protected override bool Visit(IErrorMessage error)
        {
            Console.Error.WriteLine("{0}: {1}", error.ExceptionTypes[0], Escape(ExceptionUtility.CombineMessages(error)));
            Console.Error.WriteLine(ExceptionUtility.CombineStackTraces(error));

            return base.Visit(error);
        }

        protected override bool Visit(ITestCollectionFinished testCollectionFinished)
        {
            // Base class does computation of results, so call it first.
            var result = base.Visit(testCollectionFinished);

            Console.WriteLine("##teamcity[testSuiteFinished name='{0}' flowId='{1}']",
                              TeamCityEscape(testCollectionFinished.TestCollection.DisplayName),
                              ToFlowId(testCollectionFinished.TestCollection.DisplayName));

            return result;
        }

        protected override bool Visit(ITestCollectionStarting testCollectionStarting)
        {
            Console.WriteLine("##teamcity[testSuiteStarted name='{0}' flowId='{1}']",
                              TeamCityEscape(testCollectionStarting.TestCollection.DisplayName),
                              ToFlowId(testCollectionStarting.TestCollection.DisplayName));

            return base.Visit(testCollectionStarting);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            Console.WriteLine("##teamcity[testFailed name='{0}' details='{1}|r|n{2}' flowId='{3}']",
                              TeamCityEscape(testFailed.TestDisplayName),
                              TeamCityEscape(ExceptionUtility.CombineMessages(testFailed)),
                              TeamCityEscape(ExceptionUtility.CombineStackTraces(testFailed)),
                              ToFlowId(testFailed.TestCollection.DisplayName));
            LogFinish(testFailed);

            return base.Visit(testFailed);
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            LogFinish(testPassed);

            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            Console.WriteLine("##teamcity[testIgnored name='{0}' message='{1}' flowId='{2}']",
                              TeamCityEscape(testSkipped.TestDisplayName),
                              TeamCityEscape(testSkipped.Reason),
                              ToFlowId(testSkipped.TestCollection.DisplayName));
            LogFinish(testSkipped);

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            Console.WriteLine("##teamcity[testStarted name='{0}' flowId='{1}']",
                              TeamCityEscape(testStarting.TestDisplayName),
                              ToFlowId(testStarting.TestCollection.DisplayName));

            return base.Visit(testStarting);
        }

        static string TeamCityEscape(string value)
        {
            if (value == null)
                return String.Empty;

            return value.Replace("|", "||")
                        .Replace("'", "|'")
                        .Replace("\r", "|r")
                        .Replace("\n", "|n")
                        .Replace("]", "|]")
                        .Replace("[", "|[")
                        .Replace("\u0085", "|x")
                        .Replace("\u2028", "|l")
                        .Replace("\u2029", "|p");
        }

        string ToFlowId(string testCollectionName)
        {
            return flowMappings.GetOrAdd(testCollectionName, flowIdMapper);
        }
    }
}