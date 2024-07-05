using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Reports that runner is about to start execution for a test assembly. This message will
/// arrive after the test framework's "assembly finished" message (i.e., for the default
/// test framework, <see cref="TestAssemblyFinished"/>), and contains the project metadata
/// associated with the execution.
/// </summary>
public class TestAssemblyExecutionFinished : MessageSinkMessage
{
	XunitProjectAssembly? assembly;
	ITestFrameworkExecutionOptions? executionOptions;
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
	public ITestFrameworkExecutionOptions ExecutionOptions
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
