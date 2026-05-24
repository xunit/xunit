using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents information about long running tests from <see cref="ExecutionSink"/>.
/// </summary>
/// <param name="configuredLongRunningTime">Configured notification time</param>
/// <param name="tests">Tests</param>
public class LongRunningTestsSummary(
	TimeSpan configuredLongRunningTime,
	IDictionary<ITestMetadata, TimeSpan> tests)
{
	/// <inheritdoc/>
	public TimeSpan ConfiguredLongRunningTime { get; } = configuredLongRunningTime;

	/// <inheritdoc/>
	public IDictionary<ITestMetadata, TimeSpan> Tests { get; } = Guard.ArgumentNotNull(tests);
}
