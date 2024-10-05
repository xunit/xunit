using System;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerReporter" /> that reports results to TeamCity. This
/// is auto-enabled by the presence of the "TEAMCITY_PROJECT_NAME" environment variable.
/// </summary>
public class TeamCityReporter : IRunnerReporter
{
	/// <inheritdoc/>
	public bool CanBeEnvironmentallyEnabled =>
		true;

	/// <inheritdoc/>
	public string Description =>
		"TeamCity CI support";

	/// <inheritdoc/>
	public bool ForceNoLogo =>
		false;

	/// <inheritdoc/>
	public bool IsEnvironmentallyEnabled =>
		!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME"));

	/// <inheritdoc/>
	public string RunnerSwitch =>
		"teamCity";

	/// <inheritdoc/>
	public ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
		IRunnerLogger logger,
		IMessageSink? diagnosticMessageSink) =>
#pragma warning disable CA2000 // The disposable object is returned via the ValueTask
			new(new TeamCityReporterMessageHandler(logger, Environment.GetEnvironmentVariable("TEAMCITY_PROCESS_FLOW_ID")));
#pragma warning restore CA2000
}
