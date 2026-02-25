using System.ComponentModel;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

partial interface ITheoryDataRow
{
	/// <summary>
	/// Gets a flag that indicates that the data row should only be run explicitly. If the value is <see langword="null"/>,
	/// then it inherits its explicitness from the value of <see cref="FactAttribute"/>.Explicit.
	/// </summary>
	bool? Explicit { get; }

	/// <summary>
	/// SkipType is not used in Native AOT; <see cref="SkipUnless"/> and <see cref="SkipWhen "/> already incorporate
	/// the type during code generation.
	/// </summary>
	[Obsolete("SkipType is not used in Native AOT; SkipUnless and SkipWhen already incorporate the type during code generation", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	Type? SkipType { get; }

	/// <summary>
	/// Gets a function which indicates whether the test should be skipped (<see langword="false"/>)
	/// or not (<see langword="true"/>).
	/// </summary>
	/// <remarks>
	/// This property cannot be set if <see cref="SkipWhen"/> is set. Setting both will
	/// result in a failed test.
	/// </remarks>
	Func<bool>? SkipUnless { get; }

	/// <summary>
	/// Gets a function which indicates whether the test should be skipped (<see langword="true"/>)
	/// or not (<see langword="false"/>).
	/// </summary>
	/// <remarks>
	/// This property cannot be set if <see cref="SkipUnless"/> is set. Setting both will
	/// result in a failed test.
	/// </remarks>
	Func<bool>? SkipWhen { get; }

	/// <summary>
	/// Gets the display name for the test (replacing the default behavior, which would be to
	/// use <see cref="DataAttribute.TestDisplayName"/> or <see cref="FactAttribute"/>.DisplayName,
	/// or falling back to the default test display name based on <see cref="TestMethodDisplay"/>
	/// and <see cref="TestMethodDisplayOptions"/> in the configuration file).
	/// </summary>
	string? TestDisplayName { get; }
}
