using System.Threading.Tasks;
using Xunit.v3;

namespace Xunit.Runner.Common
{
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
		public virtual ValueTask<_IMessageSink> CreateMessageHandler(
			IRunnerLogger logger,
			_IMessageSink diagnosticMessageSink) =>
				new(new DefaultRunnerReporterMessageHandler(logger));

		/// <inheritdoc/>
		public ValueTask DisposeAsync() => default;
	}
}
