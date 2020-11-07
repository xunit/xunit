using System;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="_IMessageSink" /> that supports <see cref="JsonReporter" />.
	/// </summary>
	public class JsonReporterMessageHandler : FlowMappedTestMessageSink
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JsonReporterMessageHandler" /> class.
		/// </summary>
		/// <param name="logger">The logger used to report messages</param>
		/// <param name="flowIdMapper">Optional code which maps a test collection name to a flow ID
		/// (the default behavior generates a new GUID for each test collection)</param>
		public JsonReporterMessageHandler(IRunnerLogger logger, Func<string, string>? flowIdMapper = null)
			: base(flowIdMapper)
		{
			// TODO: This format was sparse and may not be easy to adapt for v3, so we'll start replacing the handlers
			// with the built-in JSON serialization.

			Diagnostics.ErrorMessageEvent += args => logger.LogImportantMessage(args.Message.ToJson());
			Execution.TestAssemblyCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
			Execution.TestClassCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
			Execution.TestCaseCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
			Execution.TestCollectionCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
			Execution.TestCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());
			Execution.TestMethodCleanupFailureEvent += args => logger.LogImportantMessage(args.Message.ToJson());

			Execution.TestCollectionFinishedEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
			Execution.TestCollectionStartingEvent += args => logger.LogImportantMessage(args.Message.Serialize());
			Execution.TestFailedEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
			Execution.TestPassedEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
			Execution.TestSkippedEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
			Execution.TestStartingEvent += args => logger.LogImportantMessage(args.Message.ToJson(ToFlowId(args.Message.TestCollection.DisplayName)));
		}
	}
}
