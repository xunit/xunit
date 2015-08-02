using System;
using System.Collections.Concurrent;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class TeamCityReporterMessageHandler : TestMessageVisitor
    {
        readonly TeamCityDisplayNameFormatter displayNameFormatter;
        readonly ConcurrentDictionary<string, string> flowMappings = new ConcurrentDictionary<string, string>();
        readonly Func<string, string> flowIdMapper;
        readonly IRunnerLogger logger;

        public TeamCityReporterMessageHandler(IRunnerLogger logger,
                                              Func<string, string> flowIdMapper = null,
                                              TeamCityDisplayNameFormatter displayNameFormatter = null)
        {
            this.logger = logger;
            this.flowIdMapper = flowIdMapper ?? (_ => Guid.NewGuid().ToString("N"));
            this.displayNameFormatter = displayNameFormatter ?? new TeamCityDisplayNameFormatter();
        }

        protected override bool Visit(IErrorMessage error)
        {
            LogError("FATAL ERROR", error);

            return base.Visit(error);
        }

        protected override bool Visit(ITestAssemblyCleanupFailure cleanupFailure)
        {
            LogError($"Test Assembly Cleanup Failure ({cleanupFailure.TestAssembly.Assembly.AssemblyPath})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCaseCleanupFailure cleanupFailure)
        {
            LogError($"Test Case Cleanup Failure ({cleanupFailure.TestCase.DisplayName})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestClassCleanupFailure cleanupFailure)
        {
            LogError($"Test Class Cleanup Failure ({cleanupFailure.TestClass.Class.Name})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCollectionCleanupFailure cleanupFailure)
        {
            LogError($"Test Collection Cleanup Failure ({cleanupFailure.TestCollection.DisplayName})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCollectionFinished testCollectionFinished)
        {
            logger.LogImportantMessage($"##teamcity[testSuiteFinished name='{Escape(displayNameFormatter.DisplayName(testCollectionFinished.TestCollection))}' flowId='{ToFlowId(testCollectionFinished.TestCollection.DisplayName)}']");

            return base.Visit(testCollectionFinished);
        }

        protected override bool Visit(ITestCollectionStarting testCollectionStarting)
        {
            logger.LogImportantMessage($"##teamcity[testSuiteStarted name='{Escape(displayNameFormatter.DisplayName(testCollectionStarting.TestCollection))}' flowId='{ToFlowId(testCollectionStarting.TestCollection.DisplayName)}']");

            return base.Visit(testCollectionStarting);
        }

        protected override bool Visit(ITestCleanupFailure cleanupFailure)
        {
            LogError($"Test Cleanup Failure ({cleanupFailure.Test.DisplayName})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            logger.LogImportantMessage($"##teamcity[testFailed name='{Escape(displayNameFormatter.DisplayName(testFailed.Test))}' details='{Escape(ExceptionUtility.CombineMessages(testFailed))}|r|n{Escape(ExceptionUtility.CombineStackTraces(testFailed))}' flowId='{ToFlowId(testFailed.TestCollection.DisplayName)}']");
            LogFinish(testFailed);

            return base.Visit(testFailed);
        }

        protected override bool Visit(ITestMethodCleanupFailure cleanupFailure)
        {
            LogError($"Test Method Cleanup Failure ({cleanupFailure.TestMethod.Method.Name})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            LogFinish(testPassed);

            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            logger.LogImportantMessage($"##teamcity[testIgnored name='{Escape(displayNameFormatter.DisplayName(testSkipped.Test))}' message='{Escape(testSkipped.Reason)}' flowId='{ToFlowId(testSkipped.TestCollection.DisplayName)}']");
            LogFinish(testSkipped);

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            logger.LogImportantMessage($"##teamcity[testStarted name='{Escape(displayNameFormatter.DisplayName(testStarting.Test))}' flowId='{ToFlowId(testStarting.TestCollection.DisplayName)}']");

            return base.Visit(testStarting);
        }

        // Helpers

        void LogError(string messageType, IFailureInformation failureInfo)
        {
            var message = $"[{messageType}] {failureInfo.ExceptionTypes[0]}: {ExceptionUtility.CombineMessages(failureInfo)}";
            var stack = ExceptionUtility.CombineStackTraces(failureInfo);

            logger.LogImportantMessage($"##teamcity[message text='{Escape(message)}' errorDetails='{Escape(stack)}' status='ERROR']");
        }

        void LogFinish(ITestResultMessage testResult)
        {
            var formattedName = Escape(displayNameFormatter.DisplayName(testResult.Test));

            if (!string.IsNullOrWhiteSpace(testResult.Output))
                logger.LogImportantMessage($"##teamcity[testStdOut name='{formattedName}' out='{Escape(testResult.Output)}']");

            logger.LogImportantMessage($"##teamcity[testFinished name='{formattedName}' duration='{(int)(testResult.ExecutionTime * 1000M)}' flowId='{ToFlowId(testResult.TestCollection.DisplayName)}']");
        }

        static string Escape(string value)
        {
            if (value == null)
                return string.Empty;

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
            => flowMappings.GetOrAdd(testCollectionName, flowIdMapper);
    }
}
