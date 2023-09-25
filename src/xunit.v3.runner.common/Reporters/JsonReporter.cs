using System.Threading.Tasks;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerReporter" /> that reports results as individual JSON
/// objects on the console.
/// </summary>
public class JsonReporter : IRunnerReporter
{
	/// <inheritdoc/>
	public string Description => "show progress messages in JSON format";

	/// <inheritdoc/>
	public bool ForceNoLogo => true;

	/// <inheritdoc/>
	public bool IsEnvironmentallyEnabled => false;

	/// <inheritdoc/>
	public string? RunnerSwitch => "json";


	/// <inheritdoc/>
	public ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
		IRunnerLogger logger,
		_IMessageSink? diagnosticMessageSink) =>
#pragma warning disable CA2000 // The disposable object is returned via the ValueTask
			new(new JsonReporterMessageHandler(logger));
#pragma warning restore CA2000
}
