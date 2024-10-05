using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// The default implementation of <see cref="IRunnerReporter"/>, used by runners when there is no other
/// overridden reporter. It returns an instance of <see cref="DefaultRunnerReporterMessageHandler"/>.
/// </summary>
public class DefaultRunnerReporter : IRunnerReporter
{
	/// <inheritdoc/>
	public virtual bool CanBeEnvironmentallyEnabled =>
		false;

	/// <inheritdoc/>
	public virtual string Description =>
		"show standard progress messages";

	/// <inheritdoc/>
	public virtual bool ForceNoLogo =>
		false;

	/// <inheritdoc/>
	public virtual bool IsEnvironmentallyEnabled =>
		false;

	/// <inheritdoc/>
	public virtual string? RunnerSwitch =>
		"default";

	/// <inheritdoc/>
	public virtual ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
		IRunnerLogger logger,
		IMessageSink? diagnosticMessageSink) =>
#pragma warning disable CA2000 // The disposable object is returned via the ValueTask
			new(new DefaultRunnerReporterMessageHandler(logger));
#pragma warning restore CA2000
}
