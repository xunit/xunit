using System;
using System.Threading.Tasks;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="IRunnerReporter" /> that reports results to AppVeyor. This
	/// is auto-enabled by the presence of the "APPVEYOR_API_URL" environment variable, which points
	/// to the AppVeyor API endpoint that is used to report tests.
	/// </summary>
	public class AppVeyorReporter : IRunnerReporter
	{
		/// <inheritdoc/>
		public string Description => "AppVeyor CI support";

		/// <inheritdoc/>
		public bool IsEnvironmentallyEnabled =>
			!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APPVEYOR_API_URL"));

		/// <inheritdoc/>
		public string? RunnerSwitch => null;

		/// <inheritdoc/>
		public ValueTask<_IMessageSink> CreateMessageHandler(
			IRunnerLogger logger,
			_IMessageSink diagnosticMessageSink)
		{
			var baseUri = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");
			return new ValueTask<_IMessageSink>(
				baseUri == null
					? new DefaultRunnerReporterMessageHandler(logger)
					: new AppVeyorReporterMessageHandler(logger, baseUri)
			);
		}

		/// <inheritdoc/>
		public ValueTask DisposeAsync() => default;
	}
}
