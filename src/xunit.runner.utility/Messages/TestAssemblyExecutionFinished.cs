using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyExecutionFinished"/>.
    /// </summary>
    public class TestAssemblyExecutionFinished : ITestAssemblyExecutionFinished, IMessageSinkMessageWithTypes
    {
        static readonly HashSet<string> interfaceTypes = new HashSet<string>(typeof(TestAssemblyExecutionFinished).GetInterfaces().Select(x => x.FullName));

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyExecutionFinished"/> class.
        /// </summary>
        /// <param name="assembly">Information about the assembly that is being discovered</param>
        /// <param name="executionOptions">The execution options</param>
        /// <param name="executionSummary">The execution summary</param>
        public TestAssemblyExecutionFinished(XunitProjectAssembly assembly,
                                             ITestFrameworkExecutionOptions executionOptions,
                                             ExecutionSummary executionSummary)
        {
            Assembly = assembly;
            ExecutionOptions = executionOptions;
            ExecutionSummary = executionSummary;
        }

        /// <inheritdoc/>
        public XunitProjectAssembly Assembly { get; private set; }

        /// <inheritdoc/>
        public ITestFrameworkExecutionOptions ExecutionOptions { get; private set; }

        /// <inheritdoc/>
        public ExecutionSummary ExecutionSummary { get; private set; }

        /// <inheritdoc/>
        public HashSet<string> InterfaceTypes => interfaceTypes;
    }
}
