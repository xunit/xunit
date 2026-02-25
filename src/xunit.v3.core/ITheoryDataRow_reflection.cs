using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

partial interface ITheoryDataRow
{
	/// <summary>
	/// Gets a flag that indicates that the data row should only be run explicitly. If the value is <see langword="null"/>,
	/// then it inherits its explicitness from the value of <see cref="IFactAttribute.Explicit"/>.
	/// </summary>
	bool? Explicit { get; }

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
}
