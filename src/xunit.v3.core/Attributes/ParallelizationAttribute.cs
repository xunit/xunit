#pragma warning disable CA1024  // The GetXyz() methods exist to preserve nullability, so we can differentiate unset values (and attribute don't support nullable properties)

using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Used to decorate xUnit.net test assemblies to indicate parallelization options for
/// the test assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed partial class ParallelizationAttribute : Attribute
{
	ParallelAlgorithm? algorithm;
	int? maxThreads;
	ParallelMode? mode;

	/// <summary>
	/// Sets the algorithm used for test parallelization.
	/// </summary>
	public ParallelAlgorithm Algorithm
	{
		[Obsolete("Please call GetAlgorithm to preserve nullability", error: true)]
		get => throw new InvalidOperationException("Please call GetAlgorithm to preserve nullability");
		set => algorithm = value;
	}

	/// <summary>
	/// Set how many tests can run in parallel with each other.
	/// </summary>
	public int MaxThreads
	{
		[Obsolete("Please call GetMaxThreads to preserve nullability", error: true)]
		get => throw new InvalidOperationException("Please call GetMaxThreads to preserve nullability");
		set => maxThreads = value;
	}

	/// <summary>
	/// Sets the default parallelism mode for the test assembly.
	/// </summary>
	public ParallelMode Mode
	{
		[Obsolete("Please call GetMode to preserve nullability", error: true)]
		get => throw new InvalidOperationException("Please call GetMode to preserve nullability");
		set => mode = value;
	}

	/// <summary>
	/// Gets the algorithm used for test parallelization.
	/// </summary>
	/// <remarks>
	/// If <see langword="null"/>, the system will use <see cref="ParallelAlgorithm.Conservative"/>. This value
	/// will be ignored if <see cref="GetMode"/> returns <see cref="ParallelMode.None"/>.
	/// </remarks>
	public ParallelAlgorithm? GetAlgorithm() =>
		algorithm;

	/// <summary>
	/// Determines how many tests can run in parallel with each other.
	/// </summary>
	/// <remarks>
	/// If <see langword="null"/> or <c>0</c>, the system will use <see cref="Environment.ProcessorCount"/>; if
	/// a negative number, then there will be no limit to the number of threads (meaning, it uses the default
	/// thread pool). This value will be ignored if <see cref="GetMode"/> returns <see cref="ParallelMode.None"/>.
	/// </remarks>
	public int? GetMaxThreads() =>
		maxThreads;

	/// <summary>
	/// Gets the default parallelism mode for the test assembly.
	/// </summary>
	/// <remarks>
	/// If <see langword="null"/>, the system will use <see cref="ParallelMode.Collections"/>.
	/// </remarks>
	public ParallelMode? GetMode() =>
		mode;
}
