using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.Common;

// TODO: These will be replaced by their counterparts in xunit.v3.common/v3/Messages once we replace the message sink.
namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestAssemblyExecutionStarting"/>.
	/// </summary>
	public class TestAssemblyExecutionStarting : ITestAssemblyExecutionStarting, IMessageSinkMessageWithTypes
	{
		static readonly HashSet<string> interfaceTypes = new HashSet<string>(typeof(TestAssemblyExecutionStarting).GetInterfaces().Select(x => x.FullName!));

		/// <summary>
		/// Initializes a new instance of the <see cref="TestAssemblyExecutionStarting"/> class.
		/// </summary>
		/// <param name="assembly">Information about the assembly that is being discovered</param>
		/// <param name="executionOptions">The execution options</param>
		public TestAssemblyExecutionStarting(
			XunitProjectAssembly assembly,
			ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(assembly), assembly);
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			Assembly = assembly;
			ExecutionOptions = executionOptions;
		}

		/// <inheritdoc/>
		public XunitProjectAssembly Assembly { get; }

		/// <inheritdoc/>
		public ITestFrameworkExecutionOptions ExecutionOptions { get; }

		/// <inheritdoc/>
		public HashSet<string> InterfaceTypes => interfaceTypes;
	}
}
