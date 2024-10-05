using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerReporter"/> that does not report any
/// messages. Typically only used in context with the VSTest adapter, to prevent
/// double reporting of messages
/// </summary>
public class SilentReporter : IRunnerReporter
{
	/// <inheritdoc/>
	public bool CanBeEnvironmentallyEnabled =>
		false;

	/// <inheritdoc/>
	public string Description =>
		"do not show any messages";

	/// <inheritdoc/>
	public bool ForceNoLogo =>
		true;

	/// <inheritdoc/>
	public bool IsEnvironmentallyEnabled =>
		false;

	/// <inheritdoc/>
	public string RunnerSwitch =>
		"silent";

#pragma warning disable CA2000 // The disposable object is returned via the ValueTask

	/// <inheritdoc/>
	public ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
		IRunnerLogger logger,
		IMessageSink? diagnosticMessageSink) =>
			new(new SilentReporterMessageHandler());

#pragma warning restore CA2000
}
