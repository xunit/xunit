using Xunit.Internal;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerReporterMessageHandler" /> that supports <see cref="VerboseReporter" />.
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
			Guard.ArgumentNotNull(args);

			Logger.LogMessage("    {0} [STARTING]", Escape(args.Message.TestDisplayName));
		};

		Execution.TestFinishedEvent += args =>
		{
			Guard.ArgumentNotNull(args);

			var metadata = MetadataCache.TryGetTestMetadata(args.Message);
			if (metadata is not null)
				Logger.LogMessage("    {0} [FINISHED] Time: {1}s", Escape(metadata.TestDisplayName), args.Message.ExecutionTime);
			else
				Logger.LogMessage("    <unknown test> [FINISHED] Time: {0}s", args.Message.ExecutionTime);
		};

		Execution.TestNotRunEvent += args =>
		{
			Guard.ArgumentNotNull(args);

			var metadata = MetadataCache.TryGetTestMetadata(args.Message);
			if (metadata is not null)
				Logger.LogMessage("    {0} [NOT RUN]", Escape(metadata.TestDisplayName));
			else
				Logger.LogMessage("    <unknown test> [NOT RUN]");
		};
	}
}
