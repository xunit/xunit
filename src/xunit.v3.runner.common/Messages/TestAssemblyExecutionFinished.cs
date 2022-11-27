using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// Reports that runner is about to start execution for a test assembly.
/// </summary>
public class TestAssemblyExecutionFinished : _MessageSinkMessage
{
	XunitProjectAssembly? assembly;
	_ITestFrameworkExecutionOptions? executionOptions;
	ExecutionSummary? executionSummary;

	/// <summary>
	/// Gets information about the assembly being executed.
	/// </summary>
	public XunitProjectAssembly Assembly
	{
		get => this.ValidateNullablePropertyValue(assembly, nameof(Assembly));
		set => assembly = Guard.ArgumentNotNull(value, nameof(Assembly));
	}

	/// <summary>
	/// Gets the options that was used during execution.
	/// </summary>
	public _ITestFrameworkExecutionOptions ExecutionOptions
	{
		get => this.ValidateNullablePropertyValue(executionOptions, nameof(ExecutionOptions));
		set => executionOptions = Guard.ArgumentNotNull(value, nameof(ExecutionOptions));
	}

	/// <summary>
	/// Gets the summary of the execution results for the test assembly.
	/// </summary>
	public ExecutionSummary ExecutionSummary
	{
		get => this.ValidateNullablePropertyValue(executionSummary, nameof(ExecutionSummary));
		set => executionSummary = Guard.ArgumentNotNull(value, nameof(ExecutionSummary));
	}
}
