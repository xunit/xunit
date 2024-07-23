using System.Collections.Generic;
using System.Linq;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Extension methods for the JSON serialization types.
/// </summary>
public static class JsonSerializerExtensions
{
	/// <summary>
	/// Serializes an array of integers into the object.
	/// </summary>
	/// <param name="serializer"></param>
	/// <param name="key">The key to serialize the array to</param>
	/// <param name="values">The values in the array</param>
	/// <param name="includeNullArray">Whether to serialize the array if it's null</param>
	public static void SerializeIntArray(
		this JsonObjectSerializer serializer,
		string key,
		IEnumerable<int>? values,
		bool includeNullArray = false)
	{
		Guard.ArgumentNotNull(serializer);
		Guard.ArgumentNotNull(key);

		if (values is null)
		{
			if (includeNullArray)
				serializer.SerializeNull(key);
			return;
		}

		using var indexArraySerializer = serializer.SerializeArray(key);
		foreach (var value in values)
			indexArraySerializer.Serialize(value);
	}

	/// <summary>
	/// Serializes an array of strings into the object.
	/// </summary>
	/// <param name="serializer"></param>
	/// <param name="key">The key to serialize the array to</param>
	/// <param name="values">The values in the array</param>
	/// <param name="includeNullArray">Whether to serialize the array if it's null</param>
	public static void SerializeStringArray(
		this JsonObjectSerializer serializer,
		string key,
		IEnumerable<string?>? values,
		bool includeNullArray = false)
	{
		Guard.ArgumentNotNull(serializer);
		Guard.ArgumentNotNull(key);

		if (values is null)
		{
			if (includeNullArray)
				serializer.SerializeNull(key);
			return;
		}

		using var indexArraySerializer = serializer.SerializeArray(key);
		foreach (var value in values)
			indexArraySerializer.Serialize(value);
	}

	/// <summary>
	/// Serialize a trait dictionary value into the object.
	/// </summary>
	/// <param name="serializer"></param>
	/// <param name="key">The name of the value</param>
	/// <param name="dictionary">The trait dictionary</param>
	/// <param name="includeEmptyTraits">A flag to indicate whether to render empty traits</param>
	public static void SerializeTraits(
		this JsonObjectSerializer serializer,
		string key,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> dictionary,
		bool includeEmptyTraits = false)
	{
		Guard.ArgumentNotNull(serializer);
		Guard.ArgumentNotNull(key);
		Guard.ArgumentNotNull(dictionary);

		if (!includeEmptyTraits && dictionary.Count == 0)
			return;

		using var dictionarySerializer = serializer.SerializeObject(key);
		foreach (var kvp in dictionary.OrderBy(kvp => kvp.Key))
			dictionarySerializer.SerializeStringArray(kvp.Key, kvp.Value.OrderBy(v => v));
	}
}
