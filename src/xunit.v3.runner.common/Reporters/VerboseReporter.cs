using System.Threading.Tasks;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="IRunnerReporter" /> that supplements the default reporter
	/// behavior by printing out the start and finish of each executing test.
	/// </summary>
	public class VerboseReporter : IRunnerReporter
	{
		/// <inheritdoc/>
		public string Description => "show verbose progress messages";

		/// <inheritdoc/>
		public bool ForceNoLogo => false;

		/// <inheritdoc/>
		public bool IsEnvironmentallyEnabled => false;

		/// <inheritdoc/>
		public string RunnerSwitch => "verbose";

		/// <inheritdoc/>
		public ValueTask<_IMessageSink> CreateMessageHandler(
			IRunnerLogger logger,
			_IMessageSink diagnosticMessageSink) =>
				new(new VerboseReporterMessageHandler(logger));

		/// <inheritdoc/>
		public ValueTask DisposeAsync() => default;
	}
}
