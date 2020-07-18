using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Default implementation of <see cref="ITestAssemblyExecutionFinished"/>.
	/// </summary>
	public class TestAssemblyExecutionFinished : ITestAssemblyExecutionFinished, IMessageSinkMessageWithTypes
	{
		static readonly HashSet<string> interfaceTypes = new HashSet<string>(typeof(TestAssemblyExecutionFinished).GetInterfaces().Select(x => x.FullName!));

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
			Guard.ArgumentNotNull(nameof(assembly), assembly);
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);
			Guard.ArgumentNotNull(nameof(executionSummary), executionSummary);

			Assembly = assembly;
			ExecutionOptions = executionOptions;
			ExecutionSummary = executionSummary;
		}

		/// <inheritdoc/>
		public XunitProjectAssembly Assembly { get; }

		/// <inheritdoc/>
		public ITestFrameworkExecutionOptions ExecutionOptions { get; }

		/// <inheritdoc/>
		public ExecutionSummary ExecutionSummary { get; }

		/// <inheritdoc/>
		public HashSet<string> InterfaceTypes => interfaceTypes;
	}
}
