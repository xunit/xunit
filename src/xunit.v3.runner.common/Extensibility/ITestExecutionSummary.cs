using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// A message sent to implementations of <see cref="IRunnerReporter"/> when
    /// execution of all test assemblies has completed.
    /// </summary>
    public interface ITestExecutionSummary : IMessageSinkMessage
    {
        /// <summary>
        /// Gets the clock time elapsed when running the tests. This may different significantly
        /// from the sum of the times reported in the summaries, if the runner chose to run
        /// the test assemblies in parallel.
        /// </summary>
        TimeSpan ElapsedClockTime { get; }

        /// <summary>
        /// Gets the summaries of all the tests run. The key is the display name of the test
        /// assembly; the value is the summary of test execution for that assembly.
        /// </summary>
        List<KeyValuePair<string, ExecutionSummary>> Summaries { get; }
    }
}
