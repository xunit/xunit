using System;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerReporter" /> that reports results to AppVeyor. This
/// is auto-enabled by the presence of the "APPVEYOR_API_URL" environment variable, which points
/// to the AppVeyor API endpoint that is used to report tests. It has no switch for manual
/// enablement, since the API endpoint is required.
/// </summary>
public class AppVeyorReporter : IRunnerReporter
{
	/// <inheritdoc/>
	public bool CanBeEnvironmentallyEnabled =>
		true;

	/// <inheritdoc/>
	public string Description =>
		"AppVeyor CI support";

	/// <inheritdoc/>
	public bool ForceNoLogo =>
		false;

	/// <inheritdoc/>
	public bool IsEnvironmentallyEnabled =>
		!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APPVEYOR_API_URL"));

	/// <inheritdoc/>
	public string? RunnerSwitch =>
		null;

	/// <inheritdoc/>
	public ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
		IRunnerLogger logger,
		IMessageSink? diagnosticMessageSink)
	{
		var baseUri = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");
#pragma warning disable CA2000 // The disposable object is returned via the ValueTask
		var handler =
			baseUri is null
				? new DefaultRunnerReporterMessageHandler(logger)
				: new AppVeyorReporterMessageHandler(logger, baseUri);
#pragma warning restore CA2000

		return new(handler);
	}
}
