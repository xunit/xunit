using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Default implementation of <see cref="ILongRunningTestsMessage"/>.
    /// </summary>
    public class LongRunningTestsMessage : ILongRunningTestsMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LongRunningTestsMessage"/> class.
        /// </summary>
        /// <param name="configuredLongRunningTime">Configured notification time</param>
        /// <param name="testCases">Tests</param>
        public LongRunningTestsMessage(TimeSpan configuredLongRunningTime, IDictionary<ITestCase, TimeSpan> testCases)
        {
            Guard.ArgumentNotNull(nameof(testCases), testCases);

            ConfiguredLongRunningTime = configuredLongRunningTime;
            TestCases = testCases;
        }

        /// <inheritdoc/>
        public TimeSpan ConfiguredLongRunningTime { get; }

        /// <inheritdoc/>
        public IDictionary<ITestCase, TimeSpan> TestCases { get; }
    }
}