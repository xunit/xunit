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
	/// The default value is typically <see cref="ParallelAlgorithm.Conservative"/>. This value will
	/// be ignored if <see cref="Mode"/> is <see cref="ParallelMode.None"/>.
	/// </remarks>
	ParallelAlgorithm Algorithm { get; }

	/// <summary>
	/// Determines how many tests can run in parallel with each other. If the value is <c>0</c>, the
	/// system will use <see cref="Environment.ProcessorCount"/>. If the value is a negative number,
	/// then there will be no limit to the number of threads.
	/// </summary>
	/// <remarks>
	/// The default value is typically <c>0</c>. This value will be ignored if <see cref="Mode"/>
	/// is <see cref="ParallelMode.None"/>.
	/// </remarks>
	public int MaxThreads { get; }

	/// <summary>
	/// Gets the default parallelism mode for the test assembly.
	/// </summary>
	/// <remarks>
	/// The default value is typically <see cref="ParallelMode.Collections"/>.
	/// </remarks>
	ParallelMode Mode { get; }
}
