using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.v2;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="IMessageSink" /> and <see cref="IMessageSinkWithTypes" /> that
	/// supports <see cref="VerboseReporter" />.
	/// </summary>
	public class VerboseReporterMessageHandler : DefaultRunnerReporterWithTypesMessageHandler
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

				Logger.LogMessage($"    {Escape(args.Message.Test.DisplayName)} [STARTING]");
			};

			Execution.TestFinishedEvent += args =>
			{
				Guard.ArgumentNotNull(nameof(args), args);

				Logger.LogMessage($"    {Escape(args.Message.Test.DisplayName)} [FINISHED] Time: {args.Message.ExecutionTime}s");
			};
		}
	}
}
