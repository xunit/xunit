using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class JsonReporterMessageHandler : TestMessageVisitor
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
        }

        protected override bool Visit(IErrorMessage error)
        {
            logger.LogImportantMessage(error.ToJson());

            return base.Visit(error);
        }

        protected override bool Visit(ITestAssemblyCleanupFailure cleanupFailure)
        {
            logger.LogImportantMessage(cleanupFailure.ToJson());

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCaseCleanupFailure cleanupFailure)
        {
            logger.LogImportantMessage(cleanupFailure.ToJson());

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestClassCleanupFailure cleanupFailure)
        {
            logger.LogImportantMessage(cleanupFailure.ToJson());

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCollectionCleanupFailure cleanupFailure)
        {
            logger.LogImportantMessage(cleanupFailure.ToJson());

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCollectionFinished testCollectionFinished)
        {
            logger.LogImportantMessage(testCollectionFinished.ToJson(ToFlowId(testCollectionFinished.TestCollection.DisplayName)));

            return base.Visit(testCollectionFinished);
        }

        protected override bool Visit(ITestCollectionStarting testCollectionStarting)
        {
            logger.LogImportantMessage(testCollectionStarting.ToJson(ToFlowId(testCollectionStarting.TestCollection.DisplayName)));

            return base.Visit(testCollectionStarting);
        }

        protected override bool Visit(ITestCleanupFailure cleanupFailure)
        {
            logger.LogImportantMessage(cleanupFailure.ToJson());

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            logger.LogImportantMessage(testFailed.ToJson(ToFlowId(testFailed.TestCollection.DisplayName)));

            return base.Visit(testFailed);
        }

        protected override bool Visit(ITestMethodCleanupFailure cleanupFailure)
        {
            logger.LogImportantMessage(cleanupFailure.ToJson());

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            logger.LogImportantMessage(testPassed.ToJson(ToFlowId(testPassed.TestCollection.DisplayName)));

            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            logger.LogImportantMessage(testSkipped.ToJson(ToFlowId(testSkipped.TestCollection.DisplayName)));

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            logger.LogImportantMessage(testStarting.ToJson(ToFlowId(testStarting.TestCollection.DisplayName)));

            return base.Visit(testStarting);
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
