using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// A message sent to implementations of <see cref="IRunnerReporter"/> when
    /// discovery is finished for a test assembly.
    /// </summary>
    public interface ITestAssemblyDiscoveryFinished : IMessageSinkMessage
    {
        /// <summary>
        /// Gets information about the assembly being discovered.
        /// </summary>
        XunitProjectAssembly Assembly { get; }

        /// <summary>
        /// Gets the options that were used during discovery.
        /// </summary>
        ITestFrameworkDiscoveryOptions DiscoveryOptions { get; }

        /// <summary>
        /// Gets the number of test cases that were discovered. This is the raw
        /// number of test cases found before filtering is applied by the runner.
        /// </summary>
        int TestCasesDiscovered { get; }

        /// <summary>
        /// Gets the number of test cases that will be run. This is the number of
        /// test cases found after filtering is applied by the runner.
        /// </summary>
        int TestCasesToRun { get; }
    }
}
