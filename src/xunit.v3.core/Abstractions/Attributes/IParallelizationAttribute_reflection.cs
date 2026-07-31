using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Used to decorate xUnit.net test assemblies to indicate parallelization options for
/// the test assembly.
/// </summary>
/// <remarks>This is only valid at the assembly level, and there can be only one.</remarks>
public interface IParallelizationAttribute
{
	/// <summary>
	/// Gets the algorithm used for test parallelization.
	/// </summary>
	/// <remarks>
	/// If <see langword="null"/>, the system will use <see cref="ParallelAlgorithm.Conservative"/>. This value
	/// will be ignored if <see cref="GetMode"/> returns <see cref="ParallelMode.None"/>.
	/// </remarks>
	ParallelAlgorithm? GetAlgorithm();

	/// <summary>
	/// Determines how many tests can run in parallel with each other.
	/// </summary>
	/// <remarks>
	/// If <see langword="null"/> or <c>0</c>, the system will use <see cref="Environment.ProcessorCount"/>; if
	/// a negative number, then there will be no limit to the number of threads (meaning, it uses the default
	/// thread pool). This value will be ignored if <see cref="GetMode"/> returns <see cref="ParallelMode.None"/>.
	/// </remarks>
	int? GetMaxThreads();

	/// <summary>
	/// Gets the default parallelism mode for the test assembly.
	/// </summary>
	/// <remarks>
	/// If <see langword="null"/>, the system will use <see cref="ParallelMode.Collections"/>.
	/// </remarks>
	ParallelMode? GetMode();
}
