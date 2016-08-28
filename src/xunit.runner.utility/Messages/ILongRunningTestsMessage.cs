using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// A message sent to implementations of <see cref="IExecutionSink"/> when one or more tests
    /// have been running longer than the configured "long running time" value.
    /// </summary>
    public interface ILongRunningTestsMessage : IMessageSinkMessage
    {
        /// <summary>
        /// Gets the configured detection time that triggered this message.
        /// </summary>
        TimeSpan ConfiguredLongRunningTime { get; }

        /// <summary>
        /// Gets the test cases that have exceeded the configured run time.
        /// </summary>
        IDictionary<ITestCase, TimeSpan> TestCases { get; }
    }
}
