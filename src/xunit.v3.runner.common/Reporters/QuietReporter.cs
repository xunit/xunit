using Xunit.Abstractions;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="IRunnerReporter" /> that emits only error or warning messages.
	/// </summary>
	public class QuietReporter : IRunnerReporter
	{
		/// <inheritdoc/>
		public string Description => "do not show progress messages";

		/// <inheritdoc/>
		public bool IsEnvironmentallyEnabled => false;

		/// <inheritdoc/>
		public string RunnerSwitch => "quiet";

		/// <inheritdoc/>
		public IMessageSink CreateMessageHandler(IRunnerLogger logger) =>
			new QuietReporterMessageHandler(logger);
	}
}
