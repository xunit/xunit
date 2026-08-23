namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class StringExtensions
{
	/// <summary/>
	public static bool ContainsIgnoreCase(
		this string? value,
		string subString) =>
#if NETCOREAPP
			value?.Contains(subString, StringComparison.OrdinalIgnoreCase) == true;
#else
			value?.ToUpperInvariant().Contains(Guard.ArgumentNotNull(subString).ToUpperInvariant()) == true;
#endif

	/// <summary/>
	public static bool ContainsOrdinal(
		this string? value,
		string subString) =>
#if NETCOREAPP
			value?.Contains(subString, StringComparison.Ordinal) == true;
#else
			value?.Contains(subString) == true;
#endif

	/// <summary/>
	public static string? FormatTestCaseIndex(int? index) =>
		index is null || index.Value == 0
			? null
			: $"_{index.Value.ToString("D3", CultureInfo.CurrentCulture)}";

	/// <summary/>
	public static int IndexOfOrdinal(
		this string? str,
		char value) =>
#if NETCOREAPP
			str?.IndexOf(value, StringComparison.Ordinal) ?? -1;
#else
			str?.IndexOf(value) ?? -1;
#endif

	/// <summary/>
	public static string Indent(
		this string value,
		string indentation,
		string? newline = null) =>
			value
				.ReplaceOrdinal("\r\n", "\n")
				.ReplaceOrdinal("\r", "\n")
				.ReplaceOrdinal("\n", (newline ?? Environment.NewLine) + indentation);

	/// <summary/>
	public static string Quoted(this string? value) =>
		value is null ? "null" : '"' + value + '"';

	/// <summary/>
	public static string QuotedWithTrim(
		this string? value,
		int? maxLength = null)
	{
		if (value is null)
			return "null";

		maxLength ??= ArgumentFormatter.MaxStringLength;

#if NETCOREAPP
		return '"' + (value.Length > maxLength ? string.Concat(value.AsSpan(0, maxLength.Value), ArgumentFormatter.Ellipsis) : value) + '"';
#else
		return '"' + (value.Length > maxLength ? value.Substring(0, maxLength.Value) + ArgumentFormatter.Ellipsis : value) + '"';
#endif
	}

	/// <summary/>
	public static string ReplaceOrdinal(
		this string str,
		string oldValue,
		string newValue) =>
#if NETCOREAPP
			Guard.ArgumentNotNull(str).Replace(oldValue, newValue, StringComparison.Ordinal);
#else
			Guard.ArgumentNotNull(str).Replace(oldValue, newValue);
#endif

	/// <summary/>
	public static IList<string> SplitAtOuterCommas(
		this string value,
		bool trimWhitespace = false)
	{
		Guard.ArgumentNotNull(value);

		var results = new List<string>();

		var startIndex = 0;
		var endIndex = 0;
		var depth = 0;

		for (; endIndex < value.Length; ++endIndex)
		{
			switch (value[endIndex])
			{
				case '[':
					++depth;
					break;

				case ']':
					--depth;
					break;

				case ',':
					if (depth == 0 && (endIndex == 0 || value[endIndex - 1] != '\\'))
					{
						results.Add(
							trimWhitespace
								? SubstringTrim(value, startIndex, endIndex - startIndex)
								: value.Substring(startIndex, endIndex - startIndex)
						);

						startIndex = endIndex + 1;
					}
					break;

				default:
					break;
			}
		}

		if (depth != 0 || startIndex >= endIndex)
			results.Clear();
		else
			results.Add(
				trimWhitespace
					? SubstringTrim(value, startIndex, endIndex - startIndex)
					: value.Substring(startIndex, endIndex - startIndex)
			);

		return results;
	}

	static string SubstringTrim(
		string str,
		int startIndex,
		int length)
	{
		var endIndex = startIndex + length;

		while (startIndex < endIndex && char.IsWhiteSpace(str[startIndex]))
			startIndex++;

		while (endIndex > startIndex && char.IsWhiteSpace(str[endIndex - 1]))
			endIndex--;

		return str.Substring(startIndex, endIndex - startIndex);
	}
}
