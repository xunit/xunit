using System;
using System.Collections.Generic;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Represents information about long running tests from <see cref="DelegatingLongRunningTestDetectionSink"/>.
	/// </summary>
	public class LongRunningTestsSummary
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LongRunningTestsSummary"/> class.
		/// </summary>
		/// <param name="configuredLongRunningTime">Configured notification time</param>
		/// <param name="testCases">Tests</param>
		public LongRunningTestsSummary(
			TimeSpan configuredLongRunningTime,
			IDictionary<_ITestCaseMetadata, TimeSpan> testCases)
		{
			Guard.ArgumentNotNull(nameof(testCases), testCases);

			ConfiguredLongRunningTime = configuredLongRunningTime;
			TestCases = testCases;
		}

		/// <inheritdoc/>
		public TimeSpan ConfiguredLongRunningTime { get; }

		/// <inheritdoc/>
		public IDictionary<_ITestCaseMetadata, TimeSpan> TestCases { get; }
	}
}
