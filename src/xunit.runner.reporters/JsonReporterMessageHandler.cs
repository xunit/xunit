using System;
using System.Collections.Generic;
using System.Threading;

namespace Xunit.Runner.Reporters
{
    public class JsonReporterMessageHandler : TestMessageSink
    {
        readonly Func<string, string> flowIdMapper;
        readonly Dictionary<string, string> flowMappings = new Dictionary<string, string>();
        readonly ReaderWriterLockSlim flowMappingsLock = new ReaderWriterLockSlim();

        public JsonReporterMessageHandler(IRunnerLogger logger)
            : this(logger, _ => Guid.NewGuid().ToString("N"))
        { }

        public JsonReporterMessageHandler(IRunnerLogger logger, Func<string, string> flowIdMapper)
        {
            this.flowIdMapper = flowIdMapper;

            ErrorMessageEvent += args => logger.LogImportantMessage(args.Message.ToJson());
            TestAssemblyCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
            TestClassCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
            TestCaseCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
            TestCollectionCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
            TestCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
            TestMethodCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());

            TestCollectionFinishedEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
            TestCollectionStartingEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
            TestFailedEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
            TestPassedEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
            TestSkippedEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
            TestStartingEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
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
