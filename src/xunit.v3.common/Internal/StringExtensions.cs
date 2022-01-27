namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
static class StringExtensions
{
	public static string Quoted(this string? value) =>
		value == null ? "null" : "\"" + value + "\"";
}
