using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Used to decorate xUnit.net test assemblies to indicate parallelization options for
/// the test assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed partial class ParallelizationAttribute : Attribute
{
	/// <summary>
	/// Gets or sets the algorithm used for test parallelization.
	/// </summary>
	/// <remarks>
	/// The default value is <see cref="ParallelAlgorithm.Conservative"/>. This value will be ignored
	/// if <see cref="Mode"/> is <see cref="ParallelMode.None"/>.
	/// </remarks>
	public ParallelAlgorithm Algorithm { get; set; } = ParallelAlgorithm.Conservative;

	/// <summary>
	/// Determines how many tests can run in parallel with each other. If set to <c>0</c>, the system will
	/// use <see cref="Environment.ProcessorCount"/>. If set to a negative number, then there will
	/// be no limit to the number of threads (meaning, it uses the default thread pool).
	/// </summary>
	/// <remarks>
	/// The default value is <c>0</c>. This value will be ignored if <see cref="Mode"/>
	/// is <see cref="ParallelMode.None"/>.
	/// </remarks>
	public int MaxThreads { get; set; }

	/// <summary>
	/// Gets or sets the default parallelism mode for the test assembly.
	/// </summary>
	/// <remarks>
	/// The default value is <see cref="ParallelMode.Collections"/>.
	/// </remarks>
	public ParallelMode Mode { get; set; } = ParallelMode.Collections;
}
