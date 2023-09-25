using System.Threading.Tasks;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// The default implementation of <see cref="IRunnerReporter"/>, used by runners when there is no other
/// overridden reporter. It returns an instance of <see cref="DefaultRunnerReporterMessageHandler"/>.
/// </summary>
[HiddenRunnerReporter]
public class DefaultRunnerReporter : IRunnerReporter
{
	/// <inheritdoc/>
	public virtual string Description => string.Empty;

	/// <inheritdoc/>
	public bool ForceNoLogo => false;

	/// <inheritdoc/>
	public virtual bool IsEnvironmentallyEnabled => false;

	/// <inheritdoc/>
	public virtual string? RunnerSwitch => null;

	/// <inheritdoc/>
	public virtual ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
		IRunnerLogger logger,
		_IMessageSink? diagnosticMessageSink) =>
#pragma warning disable CA2000 // The disposable object is returned via the ValueTask
			new(new DefaultRunnerReporterMessageHandler(logger));
#pragma warning restore CA2000
}
