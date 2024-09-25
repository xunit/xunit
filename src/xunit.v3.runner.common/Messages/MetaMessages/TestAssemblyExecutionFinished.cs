using Xunit.Internal;
using Xunit.Sdk;

// TODO: Should this have an interface, to match up with the core messages usage?

namespace Xunit.Runner.Common;

/// <summary>
/// Reports that runner is about to start execution for a test assembly. This message will
/// arrive after the test framework's <see cref="ITestAssemblyFinished"/> message, and
/// contains the project metadata associated with the execution.
/// </summary>
/// <remarks>
/// This message does not support serialization or deserialization.
/// </remarks>
public class TestAssemblyExecutionFinished : IMessageSinkMessage
{
	XunitProjectAssembly? assembly;
	ITestFrameworkExecutionOptions? executionOptions;
	ExecutionSummary? executionSummary;

	/// <summary>
	/// Gets information about the assembly being executed.
	/// </summary>
	public required XunitProjectAssembly Assembly
	{
		get => this.ValidateNullablePropertyValue(assembly, nameof(Assembly));
		set => assembly = Guard.ArgumentNotNull(value, nameof(Assembly));
	}

	/// <summary>
	/// Gets the options that was used during execution.
	/// </summary>
	public required ITestFrameworkExecutionOptions ExecutionOptions
	{
		get => this.ValidateNullablePropertyValue(executionOptions, nameof(ExecutionOptions));
		set => executionOptions = Guard.ArgumentNotNull(value, nameof(ExecutionOptions));
	}

	/// <summary>
	/// Gets the summary of the execution results for the test assembly.
	/// </summary>
	public required ExecutionSummary ExecutionSummary
	{
		get => this.ValidateNullablePropertyValue(executionSummary, nameof(ExecutionSummary));
		set => executionSummary = Guard.ArgumentNotNull(value, nameof(ExecutionSummary));
	}

	/// <inheritdoc/>
	public string? ToJson() =>
		null;
}
