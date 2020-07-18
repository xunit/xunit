using System;
using Xunit.Abstractions;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="IRunnerReporter" /> that reports results to TeamCity. This
	/// is auto-enabled by the presence of the "TEAMCITY_PROJECT_NAME" environment variable.
	/// </summary>
	public class TeamCityReporter : IRunnerReporter
	{
		/// <inheritdoc />
		public string Description => "TeamCity CI support [normally auto-enabled]";

		/// <inheritdoc />
		public bool IsEnvironmentallyEnabled =>
			!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME"));

		/// <inheritdoc />
		public string RunnerSwitch => "teamcity";

		/// <inheritdoc />
		public IMessageSink CreateMessageHandler(IRunnerLogger logger) =>
			new TeamCityReporterMessageHandler(logger);
	}
}
