using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerReporter" /> that supplements the default reporter
/// behavior by printing out the start and finish of each executing test.
/// </summary>
public class VerboseReporter : IRunnerReporter
{
	/// <inheritdoc/>
	public bool CanBeEnvironmentallyEnabled =>
		false;

	/// <inheritdoc/>
	public string Description =>
		"show verbose progress messages";

	/// <inheritdoc/>
	public bool ForceNoLogo =>
		false;

	/// <inheritdoc/>
	public bool IsEnvironmentallyEnabled =>
		false;

	/// <inheritdoc/>
	public string RunnerSwitch =>
		"verbose";

	/// <inheritdoc/>
	public ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
		IRunnerLogger logger,
		IMessageSink? diagnosticMessageSink) =>
#pragma warning disable CA2000 // The disposable object is returned via the ValueTask
			new(new VerboseReporterMessageHandler(logger));
#pragma warning restore CA2000
}
