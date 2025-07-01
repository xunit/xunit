using System;
using System.Globalization;
using System.Linq;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Indicates the method display options for test methods.
/// </summary>
[Flags]
public enum TestMethodDisplayOptions
{
	/// <summary>
	/// Indicates no additional method display options.
	/// </summary>
	/// <remarks>This is the default configuration option.</remarks>
	None = 0x00,

	/// <summary>
	/// Replace underscores in display names with a space.
	/// </summary>
	ReplaceUnderscoreWithSpace = 0x01,

	/// <summary>
	/// Replace well-known monikers with their equivalent operator.
	/// <list type="bullet">
	/// <item><description><c>lt</c> becomes <c>&lt;</c></description></item>
	/// <item><description><c>le</c> becomes <c>&lt;=</c></description></item>
	/// <item><description><c>eq</c> becomes <c>=</c></description></item>
	/// <item><description><c>ne</c> becomes <c>!=</c></description></item>
	/// <item><description><c>gt</c> becomes <c>&gt;</c></description></item>
	/// <item><description><c>ge</c> becomes <c>&gt;=</c></description></item>
	/// </list>
	/// </summary>
	UseOperatorMonikers = 0x02,

	/// <summary>
	/// Replace supported escape sequences with their equivalent character.
	/// <list type="table">
	/// <listheader>
	///  <term>Encoding</term>
	///  <description>Format</description>
	/// </listheader>
	/// <item><term>ASCII</term><description>X hex-digit hex-digit (ex: X2C)</description></item>
	/// <item><term>Unicode</term><description>U hex-digit hex-digit hex-digit hex-digit (ex: U00A9)</description></item>
	/// </list>
	/// </summary>
	UseEscapeSequences = 0x04,

	/// <summary>
	/// Replaces the period delimiter used in namespace and type references with a comma.
	/// </summary>
	/// <remarks>This option is only honored if the <see cref="TestMethodDisplay.ClassAndMethod"/> setting is also enabled.</remarks>
	ReplacePeriodWithComma = 0x08,

	/// <summary>
	/// Enables all method display options.
	/// </summary>
	All = ReplaceUnderscoreWithSpace | UseOperatorMonikers | UseEscapeSequences | ReplacePeriodWithComma
}

/// <summary>
/// Extension methods for <see cref="TestMethodDisplayOptions"/>
/// </summary>
public static class TestMethodDisplayOptionsExtensions
{
	static readonly int[] validFlags =
		new[] {
			TestMethodDisplayOptions.ReplaceUnderscoreWithSpace,
			TestMethodDisplayOptions.UseOperatorMonikers,
			TestMethodDisplayOptions.UseEscapeSequences,
			TestMethodDisplayOptions.ReplaceUnderscoreWithSpace,
		}
		.Select(v => Convert.ToInt32(v, CultureInfo.InvariantCulture))
		.ToArray();

	/// <summary>
	/// Determines if the value is a valid enum value.
	/// </summary>
	public static bool IsValid(this TestMethodDisplayOptions value) =>
		EnumUtility.ContainsValidFlags((int)value, validFlags);
}
