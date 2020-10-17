using Xunit.Abstractions;
using Xunit.Runner.Common;
using Xunit.v3;

// TODO: These will be replaced by their counterparts in xunit.v3.common/v3/Messages once we replace the message sink.
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
		_ITestFrameworkExecutionOptions ExecutionOptions { get; }
	}
}
