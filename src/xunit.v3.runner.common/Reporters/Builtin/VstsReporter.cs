using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerReporter" /> that reports results to Azure DevOps/VSTS.
/// This is auto-enabled by the presence of four required environment variables: "VSTS_ACCESS_TOKEN",
/// "SYSTEM_TEAMFOUNDATIONCOLLECTIONURI", "SYSTEM_TEAMPROJECT", and "BUILD_BUILDID".
/// </summary>
public class VstsReporter : IRunnerReporter
{
	/// <inheritdoc/>
	public bool CanBeEnvironmentallyEnabled =>
		true;

	/// <inheritdoc/>
	public string Description =>
		"Azure DevOps/VSTS CI support";

	/// <inheritdoc/>
	public bool ForceNoLogo =>
		false;

	/// <inheritdoc/>
	public bool IsEnvironmentallyEnabled =>
		!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VSTS_ACCESS_TOKEN")) &&
		!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI")) &&
		!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SYSTEM_TEAMPROJECT")) &&
		!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("BUILD_BUILDID"));

	/// <inheritdoc/>
	public string? RunnerSwitch =>
		null;

	/// <inheritdoc/>
	public ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
		IRunnerLogger logger,
		IMessageSink? diagnosticMessageSink)
	{
		var collectionUri = Guard.NotNull("Environment variable SYSTEM_TEAMFOUNDATIONCOLLECTIONURI is not set", Environment.GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI"));
		var teamProject = Guard.NotNull("Environment variable SYSTEM_TEAMPROJECT is not set", Environment.GetEnvironmentVariable("SYSTEM_TEAMPROJECT"));
		var accessToken = Guard.NotNull("Environment variable VSTS_ACCESS_TOKEN is not set", Environment.GetEnvironmentVariable("VSTS_ACCESS_TOKEN"));
		var buildId = Convert.ToInt32(Guard.NotNull("Environment variable BUILD_BUILDID is not set", Environment.GetEnvironmentVariable("BUILD_BUILDID")), CultureInfo.InvariantCulture);

		var baseUri = string.Format(CultureInfo.InvariantCulture, "{0}{1}/_apis/test/runs", collectionUri, teamProject);

#pragma warning disable CA2000 // The disposable object is returned via the ValueTask
		return new(new VstsReporterMessageHandler(logger, baseUri, accessToken, buildId));
#pragma warning restore CA2000
	}
}
