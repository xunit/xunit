using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// Reports that runner is about to start execution for a test assembly.
/// </summary>
public class TestAssemblyExecutionStarting : _MessageSinkMessage
{
	XunitProjectAssembly? assembly;
	_ITestFrameworkExecutionOptions? executionOptions;

	/// <summary>
	/// Gets information about the assembly being executed.
	/// </summary>
	public XunitProjectAssembly Assembly
	{
		get => this.ValidateNullablePropertyValue(assembly, nameof(Assembly));
		set => assembly = Guard.ArgumentNotNull(value, nameof(Assembly));
	}

	/// <summary>
	/// Gets the options that will be used during execution.
	/// </summary>
	public _ITestFrameworkExecutionOptions ExecutionOptions
	{
		get => this.ValidateNullablePropertyValue(executionOptions, nameof(ExecutionOptions));
		set => executionOptions = Guard.ArgumentNotNull(value, nameof(ExecutionOptions));
	}

	/// <summary>
	/// Gets the seed value used for randomization. If <c>null</c>, then the test framework does not
	/// support setting a randomization seed. (For stock versions of xUnit.net, support for settable
	/// randomization seeds started with v3.)
	/// </summary>
	public int? Seed { get; set; }
}
