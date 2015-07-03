using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyExecutionStarting"/>.
    /// </summary>
    public class TestAssemblyExecutionStarting : ITestAssemblyExecutionStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyExecutionStarting"/> class.
        /// </summary>
        /// <param name="assembly">Information about the assembly that is being discovered</param>
        /// <param name="executionOptions">The execution options</param>
        public TestAssemblyExecutionStarting(XunitProjectAssembly assembly,
                                             ITestFrameworkExecutionOptions executionOptions)
        {
            Assembly = assembly;
            ExecutionOptions = executionOptions;
        }

        /// <inheritdoc/>
        public XunitProjectAssembly Assembly { get; private set; }

        /// <inheritdoc/>
        public ITestFrameworkExecutionOptions ExecutionOptions { get; private set; }
    }
}
