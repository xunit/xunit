namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class StringExtensions
{
	/// <summary/>
	public static string Quoted(this string? value) =>
		value is null ? "null" : '"' + value + '"';

	/// <summary/>
	public static string QuotedWithTrim(
		this string? value,
		int maxLength = ArgumentFormatter.MAX_STRING_LENGTH) =>
			value is null
				? "null"
				: '"' + (value.Length > maxLength ? value.Substring(0, maxLength) + ArgumentFormatter.Ellipsis : value) + '"';
}
