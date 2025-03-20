using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// A special-purpose untyped deserializer for JSON. JSON strings are returned as <see cref="string"/>,
/// JSON numbers are returned as <see cref="decimal"/>, JSON booleans are returns as <see cref="bool"/>,
/// JSON objects are returned as <c>IReadOnlyDictionary&lt;string, object?&gt;</c>, JSON arrays are
/// returned as <c>object?[]</c>, and JSON null values are returned as <c>null</c>. Static methods exist
/// here to help retrieve values from object dictionaries as well as convert to the commonly supported
/// data types (<see cref="bool"/>, <see cref="DateTimeOffset"/>, <see cref="decimal"/>, <see cref="Enum"/>,
/// <see cref="int"/>, <see cref="long"/>, <see cref="string"/>, and trait dictionaries (which are
/// decoded to <c>IReadOnlyDictionary&lt;string, IReadOnlyList&lt;string&gt;&gt;</c>), as well as arrays
/// of all the supported types (except trait dictionaries). Developers who need support for other types
/// are encouraged to encode and decode then as strings as needed (for examples, you can see how
/// <see cref="Enum"/> and <see cref="DateTimeOffset"/> values are handled here).
/// </summary>
/// <remarks>
/// These types are made public for third parties only for the purpose of serializing and
/// deserializing messages that are sent across the process boundary (that is, types which
/// implement <see cref="IMessageSinkMessage"/>). Any other usage is not supported.
/// </remarks>
public static class JsonDeserializer
{
	static readonly HashSet<char> charsBoolean = ['t', 'r', 'u', 'e', 'f', 'a', 'l', 's'];
	static readonly HashSet<char> charsNull = ['n', 'u', 'l'];
	static readonly HashSet<char> charsNumber = ['-', '+', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', 'e', 'E'];
	static readonly HashSet<char> charsWhiteSpace = [' ', '\t', '\r', '\n'];
	static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> emptyTraits = new Dictionary<string, IReadOnlyCollection<string>>();
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
		[StringSyntax(StringSyntaxAttribute.Json)]
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

		SkipWhiteSpace(json, ref idx);

		return
			idx < json.Length && json[idx] switch
			{
				't' or 'f' => TryDeserializeBoolean(json, ref idx, out result),
				'n' => TryDeserializeNull(json, ref idx),
				'-' or (>= '0' and <= '9') => TryDeserializeNumber(json, ref idx, out result),
				'"' => TryDeserializeString(json, ref idx, out result),
				'[' => TryDeserializeArray(json, ref idx, out result),
				'{' => TryDeserializeObject(json, ref idx, out result),
				_ => false,
			};
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
			if (ch is '\r' or '\n')
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

	static T[]? TryConvertArray<T>(
		object?[]? array,
		Func<object?, (bool Success, T Value)> converter)
	{
		if (array is null)
			return null;

		var result = new T[array.Length];

		for (var idx = 0; idx < array.Length; ++idx)
		{
			var item = array[idx];
			var conversion = converter(item);
			if (!conversion.Success)
				return null;
			result[idx] = conversion.Value;
		}

		return result;
	}

	/// <summary>
	/// Tries to get an untyped array value from a deserialized JSON object.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static object?[]? TryGetArray(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetArray(value) : null;

	/// <summary>
	/// Tries to get an untyped array value from a deserialized JSON value.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static object?[]? TryGetArray(object? value) =>
		value as object?[];

	/// <summary>
	/// Tries to get an array of <see cref="bool"/> values from a deserialized JSON
	/// object. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <see cref="bool"/>, then returns <c>null</c>.
	/// Null values in the array are not permitted.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static bool[]? TryGetArrayOfBoolean(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetArrayOfBoolean(value) : null;

	/// <summary>
	/// Tries to get an array of <see cref="bool"/> values from a deserialized JSON
	/// value. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <see cref="bool"/>, then returns <c>null</c>.
	/// Null values in the array are not permitted.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static bool[]? TryGetArrayOfBoolean(object? value) =>
		TryConvertArray(TryGetArray(value), v => TryGetBoolean(v) is bool convertedValue ? (true, convertedValue) : (false, default));

	/// <summary>
	/// Tries to get an array of <see cref="DateTimeOffset"/> values from a deserialized JSON
	/// object. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <see cref="DateTimeOffset"/>, then returns <c>null</c>.
	/// Null values in the array are not permitted.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static DateTimeOffset[]? TryGetArrayOfDateTimeOffset(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetArrayOfDateTimeOffset(value) : null;

	/// <summary>
	/// Tries to get an array of <see cref="DateTimeOffset"/> values from a deserialized JSON
	/// value. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <see cref="DateTimeOffset"/>, then returns <c>null</c>.
	/// Null values in the array are not permitted.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static DateTimeOffset[]? TryGetArrayOfDateTimeOffset(object? value) =>
		TryConvertArray(TryGetArray(value), v => TryGetDateTimeOffset(v) is DateTimeOffset convertedValue ? (true, convertedValue) : (false, default));

	/// <summary>
	/// Tries to get an array of <see cref="decimal"/> values from a deserialized JSON
	/// object. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <see cref="decimal"/>, then returns <c>null</c>.
	/// Null values in the array are not permitted.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static decimal[]? TryGetArrayOfDecimal(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetArrayOfDecimal(value) : null;

	/// <summary>
	/// Tries to get an array of <see cref="decimal"/> values from a deserialized JSON
	/// value. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <see cref="decimal"/>, then returns <c>null</c>.
	/// Null values in the array are not permitted.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static decimal[]? TryGetArrayOfDecimal(object? value) =>
		TryConvertArray(TryGetArray(value), v => TryGetDecimal(v) is decimal convertedValue ? (true, convertedValue) : (false, default));

	/// <summary>
	/// Tries to get an array of <typeparamref name="TEnum"/> values from a deserialized JSON
	/// object. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <typeparamref name="TEnum"/>, then returns <c>null</c>.
	/// Null values in the array are not permitted.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static TEnum[]? TryGetArrayOfEnum<TEnum>(
		IReadOnlyDictionary<string, object?> obj,
		string key)
			where TEnum : struct, Enum =>
				Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetArrayOfEnum<TEnum>(value) : null;

	/// <summary>
	/// Tries to get an array of <typeparamref name="TEnum"/> values from a deserialized JSON
	/// value. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <typeparamref name="TEnum"/>, then returns <c>null</c>.
	/// Null values in the array are not permitted.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static TEnum[]? TryGetArrayOfEnum<TEnum>(object? value)
		where TEnum : struct, Enum =>
			TryConvertArray(TryGetArray(value), v => TryGetEnum<TEnum>(v) is TEnum convertedValue ? (true, convertedValue) : (false, default));

	/// <summary>
	/// Tries to get an array of <see cref="int"/> values from a deserialized JSON
	/// object. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <see cref="int"/>, then returns <c>null</c>.
	/// Null values in the array are not permitted.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static int[]? TryGetArrayOfInt(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetArrayOfInt(value) : null;

	/// <summary>
	/// Tries to get an array of <see cref="int"/> values from a deserialized JSON
	/// value. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <see cref="int"/>, then returns <c>null</c>.
	/// Null values in the array are not permitted.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static int[]? TryGetArrayOfInt(object? value) =>
		TryConvertArray(TryGetArray(value), v => TryGetInt(v) is int convertedValue ? (true, convertedValue) : (false, default));

	/// <summary>
	/// Tries to get an array of <see cref="long"/> values from a deserialized JSON
	/// object. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <see cref="long"/>, then returns <c>null</c>.
	/// Null values in the array are not permitted.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static long[]? TryGetArrayOfLong(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetArrayOfLong(value) : null;

	/// <summary>
	/// Tries to get an array of <see cref="long"/> values from a deserialized JSON
	/// value. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <see cref="long"/>, then returns <c>null</c>.
	/// Null values in the array are not permitted.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static long[]? TryGetArrayOfLong(object? value) =>
		TryConvertArray(TryGetArray(value), v => TryGetLong(v) is long convertedValue ? (true, convertedValue) : (false, default));

	/// <summary>
	/// Tries to get an array of <see cref="string"/> values from a deserialized JSON
	/// object. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <see cref="string"/>, then returns <c>null</c>.
	/// Null values in the array are permitted.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static string?[]? TryGetArrayOfNullableString(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetArrayOfNullableString(value) : null;

	/// <summary>
	/// Tries to get an array of <see cref="string"/> values from a deserialized JSON
	/// value. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <see cref="string"/>, then returns <c>null</c>.
	/// Null values in the array are permitted.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static string?[]? TryGetArrayOfNullableString(object? value) =>
		TryConvertArray(TryGetArray(value), v => v is null ? (true, null) : (TryGetString(v) is string convertedValue ? (true, convertedValue) : (false, default)));

	/// <summary>
	/// Tries to get an array of <see cref="string"/> values from a deserialized JSON
	/// object. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <see cref="string"/>, then returns <c>null</c>.
	/// Null values in the array are not permitted.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static string[]? TryGetArrayOfString(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetArrayOfString(value) : null;

	/// <summary>
	/// Tries to get an array of <see cref="string"/> values from a deserialized JSON
	/// value. If the value isn't an array, or if any of the values in the array
	/// aren't compatible with <see cref="string"/>, then returns <c>null</c>.
	/// Null values in the array are not permitted.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static string[]? TryGetArrayOfString(object? value) =>
		TryConvertArray(TryGetArray(value), v => TryGetString(v) is string convertedValue ? (true, convertedValue) : (false, string.Empty));

	/// <summary>
	/// Tries to get a <see cref="bool"/> value from a deserialized JSON object.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static bool? TryGetBoolean(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetBoolean(value) : null;

	/// <summary>
	/// Tries to get a <see cref="bool"/> value from a deserialized JSON value.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static bool? TryGetBoolean(object? value) =>
		value is bool booleanValue ? booleanValue : null;

	/// <summary>
	/// Tries to get a <see cref="DateTimeOffset"/> value from a deserialized JSON object.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static DateTimeOffset? TryGetDateTimeOffset(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetDateTimeOffset(value) : null;

	/// <summary>
	/// Tries to get a <see cref="DateTimeOffset"/> value from a deserialized JSON value.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static DateTimeOffset? TryGetDateTimeOffset(object? value) =>
		value is string stringValue && DateTimeOffset.TryParse(stringValue, out var dto) ? dto : null;

	/// <summary>
	/// Tries to get a <see cref="decimal"/> value from a deserialized JSON object.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static decimal? TryGetDecimal(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetDecimal(value) : null;

	/// <summary>
	/// Tries to get a <see cref="decimal"/> value from a deserialized JSON value.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static decimal? TryGetDecimal(object? value) =>
		value is decimal decimalValue ? decimalValue : null;

	/// <summary>
	/// Tries to get an <see cref="Enum"/> value from a deserialized JSON object.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static TEnum? TryGetEnum<TEnum>(
		IReadOnlyDictionary<string, object?> obj,
		string key)
			where TEnum : struct, Enum =>
				Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetEnum<TEnum>(value) : null;

	/// <summary>
	/// Tries to get an <see cref="Enum"/> value from a deserialized JSON value.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static TEnum? TryGetEnum<TEnum>(object? value)
		where TEnum : struct, Enum =>
			value is string stringValue && Enum.TryParse<TEnum>(stringValue, ignoreCase: true, out var @enum) ? @enum : null;

	/// <summary>
	/// Tries to get an <see cref="int"/> value from a deserialized JSON object.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static int? TryGetInt(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetInt(value) : null;

	/// <summary>
	/// Tries to get an <see cref="int"/> value from a deserialized JSON value.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static int? TryGetInt(object? value) =>
		(value is decimal decimalValue && decimalValue % 1 == 0) ? (int)decimalValue : null;

	/// <summary>
	/// Tries to get a <see cref="long"/> value from a deserialized JSON object.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static long? TryGetLong(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetLong(value) : null;

	/// <summary>
	/// Tries to get an <see cref="long"/> value from a deserialized JSON value.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static long? TryGetLong(object? value) =>
		(value is decimal decimalValue && decimalValue % 1 == 0) ? (long)decimalValue : null;

	/// <summary>
	/// Tries to get an untyped object value from a deserialized JSON object.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static IReadOnlyDictionary<string, object?>? TryGetObject(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetObject(value) : null;

	/// <summary>
	/// Tries to get an untyped object value from a deserialized JSON value.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static IReadOnlyDictionary<string, object?>? TryGetObject(object? value) =>
		value as IReadOnlyDictionary<string, object?>;

	/// <summary>
	/// Tries to get a <see cref="string"/> value from a deserialized JSON object.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	/// <param name="defaultEmptyString">Flag to indicate if a default empty string should be returned instead of <c>null</c></param>
	/// <returns>Returns the value if present; <c>null</c>, otherwise.</returns>
	public static string? TryGetString(
		IReadOnlyDictionary<string, object?> obj,
		string key,
		bool defaultEmptyString = false) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetString(value) : (defaultEmptyString ? string.Empty : null);

	/// <summary>
	/// Tries to get an <see cref="long"/> value from a deserialized JSON value.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	/// <param name="defaultEmptyString">Flag to indicate if a default empty string should be returned instead of <c>null</c></param>
	/// <returns>Returns the value if present; <c>null</c>, otherwise.</returns>
	public static string? TryGetString(
		object? value,
		bool defaultEmptyString = false) =>
			value as string ?? (defaultEmptyString ? string.Empty : null);

	/// <summary>
	/// Tries to get a trait dictionary value from a deserialized JSON object.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	/// <param name="defaultEmptyDictionary">Flag to indicate if a default empty dictionary should be returned instead of <c>null</c></param>
	public static IReadOnlyDictionary<string, IReadOnlyCollection<string>>? TryGetTraits(
		IReadOnlyDictionary<string, object?> obj,
		string key,
		bool defaultEmptyDictionary = true) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetTraits(value) : (defaultEmptyDictionary ? emptyTraits : null);

	/// <summary>
	/// Tries to get a <see cref="Version"/> value from a deserialized JSON object.
	/// </summary>
	/// <param name="obj">The deserialized JSON object</param>
	/// <param name="key">The key for the value</param>
	public static Version? TryGetVersion(
		IReadOnlyDictionary<string, object?> obj,
		string key) =>
			Guard.ArgumentNotNull(obj).TryGetValue(key, out var value) ? TryGetVersion(value) : null;

	/// <summary>
	/// Tries to get a <see cref="Version"/> value from a deserialized JSON value.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	public static Version? TryGetVersion(object? value) =>
		value is string stringValue && Version.TryParse(stringValue, out var version) ? version : null;

	/// <summary>
	/// Tries to get a <see cref="string"/> value from a deserialized JSON value.
	/// </summary>
	/// <param name="value">The deserialized JSON value</param>
	/// <returns>Returns the value if present; <c>null</c>, otherwise.</returns>
	public static IReadOnlyDictionary<string, IReadOnlyCollection<string>>? TryGetTraits(object? value)
	{
		if (TryGetObject(value) is not IReadOnlyDictionary<string, object?> traits)
			return null;

		var result = new Dictionary<string, IReadOnlyCollection<string>>();

		foreach (var kvp in traits)
		{
			if (TryGetArrayOfString(kvp.Value) is not string[] valuesArray)
				return null;

			result[kvp.Key] = valuesArray;
		}

		return result;
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
