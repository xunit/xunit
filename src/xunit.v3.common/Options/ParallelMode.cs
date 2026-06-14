namespace Xunit.Sdk;

/// <summary>
/// Indicates the mode of parallelism used within a single test assembly.
/// </summary>
public enum ParallelMode
{
	/// <summary>
	/// Disables all parallelism within the test assembly.
	/// </summary>
	None = 0,

	/// <summary>
	/// Enables parallelism at the test collection level.
	/// </summary>
	/// <remarks>
	/// Test collections will be run in parallel against one another, but tests within
	/// the same test collection will be run serially.
	/// </remarks>
	Collections = 1,

	/// <summary>
	/// Enables full test parallelism.
	/// </summary>
	/// <remarks>
	/// Tests will be run in parallel against one another by default, regardless of test
	/// collection.
	/// </remarks>
	All = 255,
}

/// <summary>
/// Extension methods for <see cref="ParallelMode"/>
/// </summary>
public static class ParallelModeExtensions
{
	extension(ParallelMode)
	{
		/// <summary>
		/// Gets the valid values for <see cref="ParallelMode"/>.
		/// </summary>
		public static HashSet<ParallelMode> ValidValues =>
		[
			ParallelMode.None,
			ParallelMode.Collections,
			ParallelMode.All,
		];
	}

	/// <summary>
	/// Determines if the value is a valid enum value.
	/// </summary>
	public static bool IsValid(this ParallelMode value) =>
		ParallelMode.ValidValues.Contains(value);
}
