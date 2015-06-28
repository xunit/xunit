using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// A message sent to implementations of <see cref="IRunnerReporter"/> when
    /// discovery is starting for a test assembly.
    /// </summary>
    public interface ITestAssemblyDiscoveryStarting : IMessageSinkMessage
    {
        /// <summary>
        /// Gets information about the assembly being discovered.
        /// </summary>
        XunitProjectAssembly Assembly { get; }

        /// <summary>
        /// Gets the options that will be used during discovery.
        /// </summary>
        ITestFrameworkDiscoveryOptions DiscoveryOptions { get; }

        /// <summary>
        /// Gets the options that will be used during execution.
        /// </summary>
        ITestFrameworkExecutionOptions ExecutionOptions { get; }
    }
}
