using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// A message sent to implementations of <see cref="IExecutionSink"/> when
    /// an idle test is detected
    /// </summary>
    public interface ILongRunningTestNotificationMessage : IMessageSinkMessage
    {
        /// <summary>
        /// Gets the configured detection time that triggered this message
        /// </summary>
        TimeSpan ConfiguredLongRunningTime { get; }

        /// <summary>
        /// Gets the test cases that have exceeded the configured run time
        /// </summary>
        List<ITestCase> TestCases { get; }
    }
}
