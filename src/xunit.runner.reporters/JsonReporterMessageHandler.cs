using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class JsonReporterMessageHandler : TestMessageVisitor2
    {
        readonly Func<string, string> flowIdMapper;
        readonly Dictionary<string, string> flowMappings = new Dictionary<string, string>();
        readonly ReaderWriterLockSlim flowMappingsLock = new ReaderWriterLockSlim();
        readonly IRunnerLogger logger;

        public JsonReporterMessageHandler(IRunnerLogger logger)
            : this(logger, _ => Guid.NewGuid().ToString("N"))
        { }

        public JsonReporterMessageHandler(IRunnerLogger logger, Func<string, string> flowIdMapper)
        {
            this.logger = logger;
            this.flowIdMapper = flowIdMapper;
            ErrorMessageEvent += HandleErrorMessage;
            TestAssemblyCleanupFailureEvent += HandleTestAssemblyCleanupFailure;
            TestClassCleanupFailureEvent += HandleTestClassCleanupFailure;
            TestCaseCleanupFailureEvent += HandleTestCaseCleanupFailure;
            TestCollectionCleanupFailureEvent += HandleTestCollectionCleanupFailure;
            TestCollectionFinishedEvent += HandleTestCollectionFinished;
            TestCollectionStartingEvent += HandleTestCollectionStarting;
            TestCleanupFailureEvent += HandleTestCleanupFailure;
            TestFailedEvent += HandleTestFailed;
            TestMethodCleanupFailureEvent += HandleTestMethodCleanupFailure;
            TestPassedEvent += HandleTestPassed;
            TestSkippedEvent += HandleTestSkipped;
            TestStartingEvent += HandleTestStarting;
        }

        protected virtual void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
        {
            var error = args.Message;
            logger.LogImportantMessage(error.ToJson());
        }

        protected virtual void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            logger.LogImportantMessage(cleanupFailure.ToJson());
        }

        protected virtual void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            logger.LogImportantMessage(cleanupFailure.ToJson());
        }

        protected virtual void HandleTestClassCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            logger.LogImportantMessage(cleanupFailure.ToJson());
        }

        protected virtual void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            logger.LogImportantMessage(cleanupFailure.ToJson());
        }

        protected virtual void HandleTestCollectionFinished(MessageHandlerArgs<ITestCollectionFinished> args)
        {
            var testCollectionFinished = args.Message;
            logger.LogImportantMessage(testCollectionFinished.ToJson(ToFlowId(testCollectionFinished.TestCollection.DisplayName)));
        }

        protected virtual void HandleTestCollectionStarting(MessageHandlerArgs<ITestCollectionStarting> args)
        {
            var testCollectionStarting = args.Message;
            logger.LogImportantMessage(testCollectionStarting.ToJson(ToFlowId(testCollectionStarting.TestCollection.DisplayName)));
        }

        protected virtual void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            logger.LogImportantMessage(cleanupFailure.ToJson());
        }

        protected virtual void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            var testFailed = args.Message;
            logger.LogImportantMessage(testFailed.ToJson(ToFlowId(testFailed.TestCollection.DisplayName)));
        }

        protected virtual void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            logger.LogImportantMessage(cleanupFailure.ToJson());
        }

        protected virtual void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
        {
            var testPassed = args.Message;
            logger.LogImportantMessage(testPassed.ToJson(ToFlowId(testPassed.TestCollection.DisplayName)));
        }

        protected virtual void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            var testSkipped = args.Message;
            logger.LogImportantMessage(testSkipped.ToJson(ToFlowId(testSkipped.TestCollection.DisplayName)));
        }

        protected virtual void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
        {
            var testStarting = args.Message;
            logger.LogImportantMessage(testStarting.ToJson(ToFlowId(testStarting.TestCollection.DisplayName)));
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
