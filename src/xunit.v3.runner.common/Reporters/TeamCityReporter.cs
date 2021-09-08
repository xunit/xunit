using System;
using System.Threading.Tasks;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="IRunnerReporter" /> that reports results to TeamCity. This
	/// is auto-enabled by the presence of the "TEAMCITY_PROJECT_NAME" environment variable.
	/// </summary>
	public class TeamCityReporter : IRunnerReporter
	{
		/// <inheritdoc/>
		public string Description => "TeamCity CI support [normally auto-enabled]";

		/// <inheritdoc/>
		public bool ForceNoLogo => false;

		/// <inheritdoc/>
		public bool IsEnvironmentallyEnabled =>
			!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME"));

		/// <inheritdoc/>
		public string RunnerSwitch => "teamcity";

		/// <inheritdoc/>
		public ValueTask<_IMessageSink> CreateMessageHandler(
			IRunnerLogger logger,
			_IMessageSink diagnosticMessageSink) =>
				new(new TeamCityReporterMessageHandler(logger));

		/// <inheritdoc/>
		public ValueTask DisposeAsync() => default;
	}
}
