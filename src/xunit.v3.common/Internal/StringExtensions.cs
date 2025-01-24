using System.Collections.Generic;

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
		int? maxLength = null)
	{
		if (value is null)
			return "null";

		maxLength ??= ArgumentFormatter.MaxStringLength;

		return '"' + (value.Length > maxLength ? value.Substring(0, maxLength.Value) + ArgumentFormatter.Ellipsis : value) + '"';
	}

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
					if (depth == 0)
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
