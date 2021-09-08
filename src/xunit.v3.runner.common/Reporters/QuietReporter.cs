using System.Threading.Tasks;
using Xunit.v3;

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
		public bool ForceNoLogo => false;

		/// <inheritdoc/>
		public bool IsEnvironmentallyEnabled => false;

		/// <inheritdoc/>
		public string RunnerSwitch => "quiet";

		/// <inheritdoc/>
		public ValueTask<_IMessageSink> CreateMessageHandler(
			IRunnerLogger logger,
			_IMessageSink diagnosticMessageSink) =>
				new(new QuietReporterMessageHandler(logger));

		/// <inheritdoc/>
		public ValueTask DisposeAsync() => default;
	}
}
