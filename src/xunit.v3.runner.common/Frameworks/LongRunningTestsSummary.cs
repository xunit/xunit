using System;
using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents information about long running tests from <see cref="ExecutionSink"/>.
/// </summary>
/// <param name="configuredLongRunningTime">Configured notification time</param>
/// <param name="testCases">Tests</param>
public class LongRunningTestsSummary(
	TimeSpan configuredLongRunningTime,
	IDictionary<ITestCaseMetadata, TimeSpan> testCases)
{
	/// <inheritdoc/>
	public TimeSpan ConfiguredLongRunningTime { get; } = configuredLongRunningTime;

	/// <inheritdoc/>
	public IDictionary<ITestCaseMetadata, TimeSpan> TestCases { get; } = Guard.ArgumentNotNull(testCases);
}
