using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// A message sent to implementations of <see cref="IRunnerReporter"/> when
    /// execution is finished for a test assembly.
    /// </summary>
    public interface ITestAssemblyExecutionFinished : IMessageSinkMessage
    {
        /// <summary>
        /// Gets information about the assembly being discovered.
        /// </summary>
        XunitProjectAssembly Assembly { get; }

        /// <summary>
        /// Gets the options that were used during execution.
        /// </summary>
        ITestFrameworkExecutionOptions ExecutionOptions { get; }

        /// <summary>
        /// Gets the summary of the execution results for the test assembly.
        /// </summary>
        ExecutionSummary ExecutionSummary { get; }
    }
}
