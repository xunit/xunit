using System;
using Xunit.Abstractions;
using Xunit.Runner.Common;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IMessageSink" /> and <see cref="IMessageSinkWithTypes" /> that
    /// supports <see cref="JsonReporter" />.
    /// </summary>
    public class JsonReporterMessageHandler : FlowMappedTestMessageSink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonReporterMessageHandler" /> class.
        /// </summary>
        /// <param name="logger">The logger used to report messages</param>
        /// <param name="flowIdMapper">Optional code which maps a test collection name to a flow ID
        /// (the default behavior generates a new GUID for each test collection)</param>
        public JsonReporterMessageHandler(IRunnerLogger logger,
                                          Func<string, string> flowIdMapper = null)
            : base(flowIdMapper)
        {
            Diagnostics.ErrorMessageEvent += args => logger.LogImportantMessage(args.Message.ToJson());
            Execution.TestAssemblyCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
            Execution.TestClassCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
            Execution.TestCaseCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
            Execution.TestCollectionCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
            Execution.TestCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
            Execution.TestMethodCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());

            Execution.TestCollectionFinishedEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
            Execution.TestCollectionStartingEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
            Execution.TestFailedEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
            Execution.TestPassedEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
            Execution.TestSkippedEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
            Execution.TestStartingEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
        }
    }
}
