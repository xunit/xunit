namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class StringExtensions
{
	/// <summary/>
	public static string Quoted(this string? value) =>
		value == null ? "null" : @$"""{value}""";
}
