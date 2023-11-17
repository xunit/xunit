using System.Threading.Tasks;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerReporterMessageHandler"/> that
/// supports <see cref="SilentReporter"/>.
/// </summary>
public sealed class SilentReporterMessageHandler : IRunnerReporterMessageHandler
{
	/// <inheritdoc/>
	public ValueTask DisposeAsync() =>
		default;

	/// <inheritdoc/>
	public bool OnMessage(_MessageSinkMessage message) =>
		true;
}
