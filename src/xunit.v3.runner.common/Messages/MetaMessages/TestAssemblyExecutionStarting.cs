using Xunit.Internal;
using Xunit.Sdk;

// TODO: Should this have an interface, to match up with the core messages usage?

namespace Xunit.Runner.Common;

/// <summary>
/// Reports that runner is about to start execution for a test assembly. This message will
/// arrive before the test framework's <see cref="ITestAssemblyStarting"/> message, and
/// contains the project metadata associated with the discovery.
/// </summary>
/// <remarks>
/// This message does not support serialization or deserialization.
/// </remarks>
public class TestAssemblyExecutionStarting : IMessageSinkMessage
{
	XunitProjectAssembly? assembly;
	ITestFrameworkExecutionOptions? executionOptions;

	/// <summary>
	/// Gets information about the assembly being executed.
	/// </summary>
	public required XunitProjectAssembly Assembly
	{
		get => this.ValidateNullablePropertyValue(assembly, nameof(Assembly));
		set => assembly = Guard.ArgumentNotNull(value, nameof(Assembly));
	}

	/// <summary>
	/// Gets the options that will be used during execution.
	/// </summary>
	public required ITestFrameworkExecutionOptions ExecutionOptions
	{
		get => this.ValidateNullablePropertyValue(executionOptions, nameof(ExecutionOptions));
		set => executionOptions = Guard.ArgumentNotNull(value, nameof(ExecutionOptions));
	}

	/// <summary>
	/// Gets the seed value used for randomization. If <c>null</c>, then the test framework does not
	/// support setting a randomization seed. (For stock versions of xUnit.net, support for settable
	/// randomization seeds started with v3.)
	/// </summary>
	public required int? Seed { get; set; }

	/// <inheritdoc/>
	public string? ToJson() =>
		null;
}
