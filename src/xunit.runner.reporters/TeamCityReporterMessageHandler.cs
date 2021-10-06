using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class TeamCityReporterMessageHandler : TestMessageSink
    {
        readonly TeamCityDisplayNameFormatter displayNameFormatter;
        readonly Func<string, string> flowIdMapper;
        readonly Dictionary<string, string> flowMappings = new Dictionary<string, string>();
        readonly ReaderWriterLockSlim flowMappingsLock = new ReaderWriterLockSlim();
        readonly IRunnerLogger logger;

        public TeamCityReporterMessageHandler(IRunnerLogger logger,
                                              Func<string, string> flowIdMapper = null,
                                              TeamCityDisplayNameFormatter displayNameFormatter = null)
        {
            this.logger = logger;
            this.flowIdMapper = flowIdMapper ?? (_ => Guid.NewGuid().ToString("N"));
            this.displayNameFormatter = displayNameFormatter ?? new TeamCityDisplayNameFormatter();

            Diagnostics.ErrorMessageEvent += HandleErrorMessage;

            Execution.TestAssemblyCleanupFailureEvent += HandleTestAssemblyCleanupFailure;
            Execution.TestCaseCleanupFailureEvent += HandleTestCaseCleanupFailure;
            Execution.TestClassCleanupFailureEvent += HandleTestCaseCleanupFailure;
            Execution.TestCollectionCleanupFailureEvent += HandleTestCollectionCleanupFailure;
            Execution.TestCollectionFinishedEvent += HandleTestCollectionFinished;
            Execution.TestCollectionStartingEvent += HandleTestCollectionStarting;
            Execution.TestCleanupFailureEvent += HandleTestCleanupFailure;
            Execution.TestFailedEvent += HandleTestFailed;
            Execution.TestMethodCleanupFailureEvent += HandleTestMethodCleanupFailure;
            Execution.TestPassedEvent += HandleTestPassed;
            Execution.TestSkippedEvent += HandleTestSkipped;
            Execution.TestStartingEvent += HandleTestStarting;
        }

        protected virtual void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
        {
            var error = args.Message;
            LogError("FATAL ERROR", error);
        }

        protected virtual void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            LogError($"Test Assembly Cleanup Failure ({cleanupFailure.TestAssembly.Assembly.AssemblyPath})", cleanupFailure);
        }

        protected virtual void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            LogError($"Test Case Cleanup Failure ({cleanupFailure.TestCase.DisplayName})", cleanupFailure);
        }

        protected virtual void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            LogError($"Test Class Cleanup Failure ({cleanupFailure.TestClass.Class.Name})", cleanupFailure);
        }

        protected virtual void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            LogError($"Test Collection Cleanup Failure ({cleanupFailure.TestCollection.DisplayName})", cleanupFailure);
        }

        protected virtual void HandleTestCollectionFinished(MessageHandlerArgs<ITestCollectionFinished> args)
        {
            var testCollectionFinished = args.Message;
            logger.LogImportantMessage($"##teamcity[testSuiteFinished name='{Escape(displayNameFormatter.DisplayName(testCollectionFinished.TestCollection))}' flowId='{ToFlowId(testCollectionFinished.TestCollection.DisplayName)}']");
        }

        protected virtual void HandleTestCollectionStarting(MessageHandlerArgs<ITestCollectionStarting> args)
        {
            var testCollectionStarting = args.Message;
            logger.LogImportantMessage($"##teamcity[testSuiteStarted name='{Escape(displayNameFormatter.DisplayName(testCollectionStarting.TestCollection))}' flowId='{ToFlowId(testCollectionStarting.TestCollection.DisplayName)}']");
        }

        protected virtual void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            LogError($"Test Cleanup Failure ({cleanupFailure.Test.DisplayName})", cleanupFailure);
        }

        protected virtual void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            var testFailed = args.Message;
            logger.LogImportantMessage($"##teamcity[testFailed name='{Escape(displayNameFormatter.DisplayName(testFailed.Test))}' details='{Escape(ExceptionUtility.CombineMessages(testFailed))}|r|n{Escape(ExceptionUtility.CombineStackTraces(testFailed))}' flowId='{ToFlowId(testFailed.TestCollection.DisplayName)}']");
            LogFinish(testFailed);
        }

        protected virtual void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            LogError($"Test Method Cleanup Failure ({cleanupFailure.TestMethod.Method.Name})", cleanupFailure);
        }

        protected virtual void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
        {
            var testPassed = args.Message;
            LogFinish(testPassed);
        }

        protected virtual void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            var testSkipped = args.Message;
            logger.LogImportantMessage($"##teamcity[testIgnored name='{Escape(displayNameFormatter.DisplayName(testSkipped.Test))}' message='{Escape(testSkipped.Reason)}' flowId='{ToFlowId(testSkipped.TestCollection.DisplayName)}']");
            LogFinish(testSkipped);
        }

        protected virtual void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
        {
            var testStarting = args.Message;
            logger.LogImportantMessage($"##teamcity[testStarted name='{Escape(displayNameFormatter.DisplayName(testStarting.Test))}' flowId='{ToFlowId(testStarting.TestCollection.DisplayName)}']");
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
                logger.LogImportantMessage($"##teamcity[testStdOut name='{formattedName}' out='{Escape(testResult.Output)}' flowId='{ToFlowId(testResult.TestCollection.DisplayName)}' tc:tags='tc:parseServiceMessagesInside']");

            logger.LogImportantMessage($"##teamcity[testFinished name='{formattedName}' duration='{(int)(testResult.ExecutionTime * 1000M)}' flowId='{ToFlowId(testResult.TestCollection.DisplayName)}']");
        }

        static bool IsAscii(char ch) => ch <= '\x007f';

        static string Escape(string value)
        {
            var sb = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                var ch = value[i];

                switch (ch)
                {
                    case '|':
                        sb.Append("||");
                        break;
                    case '\'':
                        sb.Append("|'");
                        break;
                    case '\n':
                        sb.Append("|n");
                        break;
                    case '\r':
                        sb.Append("|r");
                        break;
                    case '[':
                        sb.Append("|[");
                        break;
                    case ']':
                        sb.Append("|]");
                        break;
                    default:
                        if (IsAscii(ch))
                            sb.Append(ch);
                        else
                        {
                            sb.Append("|0x");
                            sb.Append(((int)ch).ToString("x4"));
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        string ToFlowId(string testCollectionName)
        {
            string result;

            flowMappingsLock.EnterReadLock();

            try
            {
                if (flowMappings.TryGetValue(testCollectionName, out result))
                    return result;
            }
            finally
            {
                flowMappingsLock.ExitReadLock();
            }

            flowMappingsLock.EnterWriteLock();

            try
            {
                if (!flowMappings.TryGetValue(testCollectionName, out result))
                {
                    result = flowIdMapper(testCollectionName);
                    flowMappings[testCollectionName] = result;
                }

                return result;
            }
            finally
            {
                flowMappingsLock.ExitWriteLock();
            }
        }
    }
}
