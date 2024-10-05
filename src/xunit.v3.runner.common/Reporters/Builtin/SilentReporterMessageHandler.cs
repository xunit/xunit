using System.Threading.Tasks;
using Xunit.Sdk;

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
	public bool OnMessage(IMessageSinkMessage message) =>
		true;
}
