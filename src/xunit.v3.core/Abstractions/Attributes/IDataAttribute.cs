using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base interface that all data attributes (that is, data providers for theories) are
/// expected to implement. Data attributes are valid on methods only.
/// </summary>
public interface IDataAttribute
{
	/// <summary>
	/// Gets a value that determines whether the data rows provided by this data
	/// provider should be considered explicit or not. If <see langword="true"/>, then the data
	/// rows will all be considered explicit; if <see langword="false"/>, then the data rows
	/// will all be considered not explicit; if <see langword="null"/>, then the data rows will
	/// inherit their explicitness from <see cref="IFactAttribute.Explicit"/>.
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
	/// Gets a skip reason for all the data rows provided by this data provider. If
	/// not <see langword="null"/>, then all rows will be skipped with the given reason; if <see langword="null"/>,
	/// then the rows will inherit their skip reason from <see cref="IFactAttribute.Skip"/>.
	/// </summary>
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
	/// result in a failed test.
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
	/// result in a failed test.
	/// To ensure compile-time safety and easier refactoring, use the <see langword="nameof"/> operator
	/// e.g., <c>SkipWhen = nameof(IsConditionMet)</c>.
	/// </remarks>
	string? SkipWhen { get; }

	/// <summary>
	/// Gets the test display name for the test (replacing the default behavior, which
	/// would be to use <see cref="IFactAttribute.DisplayName"/>, or falling back to
	/// generating display names based on <see cref="TestMethodDisplay"/> and
	/// <see cref="TestMethodDisplayOptions"/>).
	/// </summary>
	string? TestDisplayName { get; }

	/// <summary>
	/// Gets a value to determine if the data rows provided by this data provider should
	/// include a timeout (in milliseconds). If greater than zero, the data rows will have
	/// the given timeout; if zero or less, the data rows will not have a timeout; if <see langword="null"/>,
	/// the data rows will inherit their timeout from <see cref="IFactAttribute.Timeout"/>.
	/// </summary>
	int? Timeout { get; }

	/// <summary>
	/// Gets a set of traits for the associated data. The data is provided as an array
	/// of string values that are alternating keys and values (e.g.,
	/// <c>["key1", "value1", "key2", "value2"]</c>).
	/// </summary>
	/// <remarks>
	/// This is structured as an array because attribute initializers don't support dictionaries. Note:
	/// Setting an odd number of values will throw away the unmatched key at the end of the list. If you
	/// seem to be missing your a key/value pair or have misaligned keys and values, make sure you have
	/// an even number of strings alternating between keys and values.
	/// </remarks>
	string[]? Traits { get; }

	/// <summary>
	/// Returns the data to be used to test the theory.
	/// </summary>
	/// <param name="testMethod">The test method the data attribute is attached to</param>
	/// <param name="disposalTracker">The disposal tracker used to dispose the data</param>
	ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
		MethodInfo testMethod,
		DisposalTracker disposalTracker);

	/// <summary>
	/// Returns <see langword="true"/> if the data attribute supports enumeration during
	/// discovery; <see langword="false"/> otherwise. Data attributes with expensive computational
	/// costs and/or randomized data sets should return <see langword="false"/>.
	/// </summary>
	bool SupportsDiscoveryEnumeration();
}
