using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Represents a single row of theory data, along with optional metadata attached to the row.
/// </summary>
public interface ITheoryDataRow
{
	/// <summary>
	/// Gets a flag that indicates that the data row should only be run explicitly. If the value is <see langword="null"/>,
	/// then it inherits its explicitness from the value of <see cref="IFactAttribute.Explicit"/>.
	/// </summary>
	bool? Explicit { get; }

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
	/// Gets the type to retrieve <see cref="SkipUnless"/> or <see cref="SkipWhen"/> from. If not set,
	/// then the property will be retrieved from the unit test class.
	/// </summary>
	Type? SkipType { get; }

	/// <summary>
	/// Gets the name of a public static property on the test class which returns <see cref="bool"/>
	/// to indicate whether the test should be skipped (<see langword="false"/>) or not (<see langword="true"/>).
	/// </summary>
	/// <remarks>
	/// This property cannot be set if <see cref="SkipWhen"/> is set. Setting both will
	/// result in a failed test.<br />
	/// <br />
	/// To ensure compile-time safety and easier refactoring, use the <see langword="nameof"/> operator,
	/// e.g., <c>SkipUnless = nameof(IsConditionMet)</c>.
	/// </remarks>
	string? SkipUnless { get; }

	/// <summary>
	/// Gets the name of a public static property on the test class which returns <see cref="bool"/>
	/// to indicate whether the test should be skipped (<see langword="true"/>) or not (<see langword="false"/>).
	/// </summary>
	/// <remarks>
	/// This property cannot be set if <see cref="SkipUnless"/> is set. Setting both will
	/// result in a failed test.<br />
	/// <br />
	/// To ensure compile-time safety and easier refactoring, use the <see langword="nameof"/> operator,
	/// e.g., <c>SkipWhen = nameof(IsConditionMet)</c>.
	/// </remarks>
	string? SkipWhen { get; }

	/// <summary>
	/// Gets the display name for the test (replacing the default behavior, which would be to
	/// use <see cref="IDataAttribute.TestDisplayName"/> or <see cref="IFactAttribute.DisplayName"/>,
	/// or falling back to the default test display name based on <see cref="TestMethodDisplay"/>
	/// and <see cref="TestMethodDisplayOptions"/> in the configuration file).
	/// </summary>
	string? TestDisplayName { get; }

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
