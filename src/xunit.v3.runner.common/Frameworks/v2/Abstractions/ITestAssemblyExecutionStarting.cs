using Xunit.Abstractions;
using Xunit.Runner.Common;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// A message sent to implementations of <see cref="IRunnerReporter"/> when
	/// execution is starting for a test assembly.
	/// </summary>
	public interface ITestAssemblyExecutionStarting : IMessageSinkMessage
	{
		/// <summary>
		/// Gets information about the assembly being discovered.
		/// </summary>
		XunitProjectAssembly Assembly { get; }

		/// <summary>
		/// Gets the options that will be used during execution.
		/// </summary>
		ITestFrameworkExecutionOptions ExecutionOptions { get; }
	}
}
