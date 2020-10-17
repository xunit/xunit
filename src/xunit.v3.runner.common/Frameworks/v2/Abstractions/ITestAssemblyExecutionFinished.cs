using Xunit.Abstractions;
using Xunit.Runner.Common;

// TODO: These will be replaced by their counterparts in xunit.v3.common/v3/Messages once we replace the message sink.
namespace Xunit.Runner.v2
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
