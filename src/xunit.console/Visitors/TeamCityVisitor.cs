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
                              TeamCityEscape(testResult.Test.DisplayName),
                              (int)(testResult.ExecutionTime * 1000M),
                              ToFlowId(testResult.TestCollection.DisplayName));
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
                              TeamCityEscape(testFailed.Test.DisplayName),
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
                              TeamCityEscape(testSkipped.Test.DisplayName),
                              TeamCityEscape(testSkipped.Reason),
                              ToFlowId(testSkipped.TestCollection.DisplayName));
            LogFinish(testSkipped);

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            Console.WriteLine("##teamcity[testStarted name='{0}' flowId='{1}']",
                              TeamCityEscape(testStarting.Test.DisplayName),
                              ToFlowId(testStarting.TestCollection.DisplayName));

            return base.Visit(testStarting);
        }

        protected override bool Visit(IErrorMessage error)
        {
            WriteError("FATAL", error);

            return base.Visit(error);
        }

        protected override bool Visit(ITestAssemblyCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Assembly Cleanup Failure ({0})", cleanupFailure.TestAssembly.Assembly.AssemblyPath), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCaseCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Case Cleanup Failure ({0})", cleanupFailure.TestCase.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestClassCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Class Cleanup Failure ({0})", cleanupFailure.TestClass.Class.Name), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCollectionCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Collection Cleanup Failure ({0})", cleanupFailure.TestCollection.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Cleanup Failure ({0})", cleanupFailure.Test.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestMethodCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Method Cleanup Failure ({0})", cleanupFailure.TestMethod.Method.Name), cleanupFailure);

            return base.Visit(cleanupFailure);
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

        static void WriteError(string messageType, IFailureInformation failureInfo)
        {
            var message = String.Format("[{0}] {1}: {2}", messageType, failureInfo.ExceptionTypes[0], ExceptionUtility.CombineMessages(failureInfo));
            var stack = ExceptionUtility.CombineStackTraces(failureInfo);

            Console.WriteLine("##teamcity[message text='{0}' errorDetails='{1}' status='ERROR']", TeamCityEscape(message), TeamCityEscape(stack));
        }
    }
}