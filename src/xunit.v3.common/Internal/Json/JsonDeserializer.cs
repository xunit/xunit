using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class JsonDeserializer
{
	static readonly HashSet<char> charsBoolean = ['t', 'r', 'u', 'e', 'f', 'a', 'l', 's'];
	static readonly HashSet<char> charsNull = ['n', 'u', 'l'];
	static readonly HashSet<char> charsNumber = ['-', '+', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', 'e', 'E'];
	static readonly HashSet<char> charsWhiteSpace = [' ', '\t', '\r', '\n'];
	static readonly Dictionary<char, char> escapesString = new()
	{
		{ '"', '"' },
		{ '\\', '\\' },
		{ '/', '/' },
		{ 'b', '\b' },
		{ 'f', '\f' },
		{ 'n', '\n' },
		{ 't', '\t' },
		{ 'r', '\r' },
	};

	static string RetrieveToken(
		string json,
		HashSet<char> chars,
		ref int idx)
	{
		var endIdx = idx;

		while (endIdx < json.Length && chars.Contains(json[endIdx]))
			++endIdx;

		var value = json.Substring(idx, endIdx - idx);
		idx = endIdx;

		return value;
	}

	static void SkipWhiteSpace(
		string json,
		ref int idx)
	{
		while (idx < json.Length && charsWhiteSpace.Contains(json[idx]))
			++idx;
	}

	/// <summary/>
	public static bool TryDeserialize(
		string json,
		out object? result)
	{
		Guard.ArgumentNotNull(json);

		var idx = 0;

		if (TryDeserialize(json, ref idx, out result))
		{
			SkipWhiteSpace(json, ref idx);
			if (idx == json.Length)
				return true;
		}

		result = default;
		return false;
	}

	static bool TryDeserialize(
		string json,
		ref int idx,
		out object? result)
	{
		result = default;

		while (true)
		{
			SkipWhiteSpace(json, ref idx);
			if (idx >= json.Length)
				return false;

			return json[idx] switch
			{
				't' or 'f' => TryDeserializeBoolean(json, ref idx, out result),
				'n' => TryDeserializeNull(json, ref idx),
				'-' or >= '0' and <= '9' => TryDeserializeNumber(json, ref idx, out result),
				'"' => TryDeserializeString(json, ref idx, out result),
				'[' => TryDeserializeArray(json, ref idx, out result),
				'{' => TryDeserializeObject(json, ref idx, out result),
				_ => false,
			};
		}
	}

	static bool TryDeserializeArray(
		string json,
		ref int idx,
		out object? result)
	{
		var values = new List<object?>();
		var success = TryParseCollection(']', json, ref idx, i =>
		{
			var success = TryDeserialize(json, ref i, out var value);
			if (success)
				values.Add(value);

			return (success, i);
		});

		result = success ? values.ToArray() : default;
		return success;
	}

	static bool TryDeserializeBoolean(
		string json,
		ref int idx,
		out object? result)
	{
		result = RetrieveToken(json, charsBoolean, ref idx) switch
		{
			"true" => true,
			"false" => false,
			_ => null,
		};

		return result is not null;
	}

	static bool TryDeserializeNull(
		string json,
		ref int idx) =>
			RetrieveToken(json, charsNull, ref idx) == "null";

	static bool TryDeserializeNumber(
		string json,
		ref int idx,
		out object? result)
	{
		var token = RetrieveToken(json, charsNumber, ref idx);
		var success = decimal.TryParse(token, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out var value);
		result = success ? value : default;
		return success;
	}

	static bool TryDeserializeObject(
		string json,
		ref int idx,
		out object? result)
	{
		var values = new Dictionary<string, object?>();
		var success = TryParseCollection('}', json, ref idx, i =>
		{
			// Deserialize the key
			var success = TryDeserialize(json, ref i, out var keyValue);
			if (success && keyValue is string key)
			{
				// Find the colon
				SkipWhiteSpace(json, ref i);
				if (i < json.Length && json[i++] == ':')
				{
					// Deserialize the value
					success = TryDeserialize(json, ref i, out var value);
					if (success)
						values[key] = value;
				}
			}

			return (success, i);
		});

		result = success ? values : default;
		return success;
	}

	static bool TryDeserializeString(
		string json,
		ref int idx,
		out object? result)
	{
		var stringValue = new StringBuilder();
		var success = true;

		// Skip the opening quote
		++idx;

		// Loop until we see the closing quote
		while (idx < json.Length && json[idx] != '"')
		{
			var ch = json[idx++];

			// No embedded newlines allowed
			if (ch == '\r' || ch == '\n')
			{
				success = false;
				break;
			}

			// Everything that's not escaped gets appended
			if (ch != '\\')
			{
				stringValue.Append(ch);
				continue;
			}

			// Make sure we have a 2nd character of the escape sequence
			if (idx >= json.Length)
			{
				success = false;
				break;
			}

			if (!escapesString.TryGetValue(json[idx++], out var escapedValue))
			{
				success = false;
				break;
			}

			stringValue.Append(escapedValue);
		}

		if (success && (idx >= json.Length || json[idx] != '"'))
			success = false;
		else
			++idx;

		result = success ? stringValue.ToString() : default;
		return success;
	}

	static bool TryParseCollection(
		char closing,
		string json,
		ref int idx,
		Func<int, (bool Success, int NewIndex)> elementParser)
	{
		var expectingMore = false;

		// Skip the opening character
		++idx;

		while (true)
		{
			SkipWhiteSpace(json, ref idx);
			if (idx >= json.Length)
				return false;

			// End of the collection?
			if (json[idx] == closing)
			{
				++idx;
				// Don't want , without data
				return !expectingMore;
			}

			var parseResult = elementParser(idx);
			idx = parseResult.NewIndex;
			if (!parseResult.Success)
				return false;

			// Skip any trailing whitespace after the value
			SkipWhiteSpace(json, ref idx);
			if (idx >= json.Length)
				return false;

			// Must be followed by a ',' (more data) or closing character (end of data)
			expectingMore = json[idx] == ',';
			if (expectingMore)
				++idx;
			else if (json[idx] != closing)
				return false;
		}
	}
}
