﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Default implementation of <see cref="ILongRunningTestNotificationMessage"/>.
    /// </summary>
    public class LongRunningTestNotification : ILongRunningTestNotificationMessage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LongRunningTestNotification" /> class.
        /// </summary>
        /// <param name="configuredLongRunningTime">Configured notification time</param>
        /// <param name="testCases">Tests</param>
        public LongRunningTestNotification(TimeSpan configuredLongRunningTime, IDictionary<ITestCase, TimeSpan> testCases)
        {
            if (testCases == null) throw new ArgumentNullException(nameof(testCases));
            ConfiguredLongRunningTime = configuredLongRunningTime;
            TestCases = testCases;
        }

        /// <inheritdoc/>
        public TimeSpan ConfiguredLongRunningTime { get; }

        /// <inheritdoc/>
        public IDictionary<ITestCase, TimeSpan> TestCases { get; }
    }
}