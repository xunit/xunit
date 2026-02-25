using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test assembly from xUnit.net v3.
/// </summary>
/// <remarks>
/// This interface is shared between reflection-based and code generation-based tests.
/// </remarks>
public interface ICoreTestAssembly : ITestAssembly
{
	/// <summary>
	/// Gets the assembly of this test assembly.
	/// </summary>
	/// <remarks>
	/// This should only be used to execute a test assembly. All reflection should be abstracted here
	/// instead for better testability.
	/// </remarks>
	Assembly Assembly { get; }

	/// <summary>
	/// Gets a flag which indicates whether the user has requested that parallelization be disabled.
	/// </summary>
	/// <remarks>
	/// If this returns <see langword="null"/>, the default value will be used (typically <see langword="false"/>).
	/// </remarks>
	bool? DisableParallelization { get; }

	/// <summary>
	/// Gets the maximum number of threads to use when running tests in parallel. If this returns a
	/// positive integer, that is the maximum number of threads; if it returns -1, that indicates that
	/// unlimited threads should be allowed.
	/// </summary>
	/// <remarks>
	/// If this returns <see langword="null"/>, the default value will be used (typically <see cref="Environment.ProcessorCount"/>).
	/// </remarks>
	int? MaxParallelThreads { get; }

	/// <summary>
	/// Gets the algorithm used for parallelism.
	/// </summary>
	/// <remarks>
	/// If this returns <see langword="null"/>, the default value will be used (typically <see cref="ParallelAlgorithm.Conservative"/>).<br />
	/// <br />
	/// This will only be relevant if <see cref="DisableParallelization"/> returns <see langword="false"/>.
	/// </remarks>
	ParallelAlgorithm? ParallelAlgorithm { get; }

	/// <summary>
	/// Gets the target framework the test assembly was compiled against. Will be in a
	/// form like <c>".NETFramework,Version=v4.7.2"</c> or <c>".NETCoreApp,Version=v8.0"</c>.
	/// </summary>
	string TargetFramework { get; }

	/// <summary>
	/// Gets the test case orderer for the test assembly, if present.
	/// </summary>
	ITestCaseOrderer? TestCaseOrderer { get; }

	/// <summary>
	/// Gets the test class orderer for the test assembly, if present.
	/// </summary>
	ITestClassOrderer? TestClassOrderer { get; }

	/// <summary>
	/// Gets the test collection orderer for the test assembly, if present.
	/// </summary>
	ITestCollectionOrderer? TestCollectionOrderer { get; }

	/// <summary>
	/// Gets the test method orderer for the test assembly, if present.
	/// </summary>
	ITestMethodOrderer? TestMethodOrderer { get; }

	/// <summary>
	/// Gets the assembly version.
	/// </summary>
	Version Version { get; }
}
