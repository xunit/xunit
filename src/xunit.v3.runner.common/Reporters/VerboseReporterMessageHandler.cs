using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="_IMessageSink" /> that supports <see cref="VerboseReporter" />.
	/// </summary>
	public class VerboseReporterMessageHandler : DefaultRunnerReporterMessageHandler
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VerboseReporterMessageHandler" /> class.
		/// </summary>
		/// <param name="logger">The logger used to report messages</param>
		public VerboseReporterMessageHandler(IRunnerLogger logger)
			: base(logger)
		{
			Execution.TestStartingEvent += args =>
			{
				Guard.ArgumentNotNull(nameof(args), args);

				Logger.LogMessage($"    {Escape(args.Message.TestDisplayName)} [STARTING]");
			};

			Execution.TestFinishedEvent += args =>
			{
				Guard.ArgumentNotNull(nameof(args), args);

				var metadata = MetadataCache.TryGetTestMetadata(args.Message);
				if (metadata != null)
					Logger.LogMessage($"    {Escape(metadata.TestDisplayName)} [FINISHED] Time: {args.Message.ExecutionTime}s");
				else
					Logger.LogMessage($"    <unknown test> [FINISHED] Time: {args.Message.ExecutionTime}s");
			};
		}
	}
}
