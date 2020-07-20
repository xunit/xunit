using System;
using Xunit.Abstractions;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="IRunnerReporter" /> that reports results to AppVeyor. This
	/// is auto-enabled by the presence of the "APPVEYOR_API_URL" environment variable, which points
	/// to the AppVeyor API endpoint that is used to report tests.
	/// </summary>
	public class AppVeyorReporter : IRunnerReporter
	{
		/// <inheritdoc />
		public string Description => "AppVeyor CI support";

		/// <inheritdoc />
		public bool IsEnvironmentallyEnabled =>
			!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APPVEYOR_API_URL"));

		/// <inheritdoc />
		public string? RunnerSwitch => null;

		/// <inheritdoc />
		public IMessageSink CreateMessageHandler(IRunnerLogger logger)
		{
			var baseUri = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");
			return
				baseUri == null
					? new DefaultRunnerReporterWithTypesMessageHandler(logger)
					: new AppVeyorReporterMessageHandler(logger, baseUri);
		}
	}
}
