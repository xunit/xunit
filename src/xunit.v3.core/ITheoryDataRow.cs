using Xunit.v3;

namespace Xunit;

/// <summary>
/// Represents a single row of theory data, along with optional metadata attached to the row.
/// </summary>
public partial interface ITheoryDataRow
{
	/// <summary>
	/// Determines whether the test from this theory data row should run in parallel against any other parallelizable tests.
	/// </summary>
	/// <remarks>
	/// If this value is either <see langword="true"/> or <see langword="false"/>, it overrides the setting from
	/// the data attribute; if this value is <see langword="null"/>, then the value from the data attribute is
	/// used instead.
	/// </remarks>
	bool? DisableParallelization { get; }

	/// <summary>
	/// Gets the label to use for the data row. This value is used to help format the display name
	/// of the test.
	/// </summary>
	/// <remarks>
	/// * If the value is <see langword="null"/> (or not set), use the default behavior: <c>MethodName(...argument list...)</c><br/>
	/// * If the value is an empty string, use just the method name: <c>MethodName</c><br/>
	/// * For any other values, appends the label: <c>MethodName [label]</c>
	/// </remarks>
	string? Label { get; }

	/// <summary>
	/// Gets the skip reason for the test. When <see langword="null"/> is returned, the test is
	/// not skipped.
	/// </summary>
	/// <remarks>
	/// Skipping is conditional based on whether <see cref="SkipWhen"/> or <see cref="SkipUnless"/>
	/// is set.
	/// </remarks>
	string? Skip { get; }

	/// <summary>
	/// A value greater than zero marks the test as having a timeout, and gets or sets the
	/// timeout (in milliseconds). A non-<see langword="null"/> value here overrides any inherited value
	/// from the <see cref="IDataAttribute"/> or the <see cref="ITheoryAttribute"/>.
	/// </summary>
	/// <remarks>
	/// WARNING: Using this with parallelization turned on will result in undefined behavior.
	/// Timeout is only supported when parallelization is disabled, either globally or with
	/// a parallelization-disabled test collection.
	/// </remarks>
	int? Timeout { get; }

	/// <summary>
	/// Gets the trait values associated with this theory data row. If there are none, you may either
	/// return a <see langword="null"/> or empty dictionary.
	/// </summary>
	Dictionary<string, HashSet<string>>? Traits { get; }

	/// <summary>
	/// Gets the theory data.
	/// </summary>
	object?[] GetData();
}
