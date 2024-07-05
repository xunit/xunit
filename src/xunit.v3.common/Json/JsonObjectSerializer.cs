using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// A special-purpose streaming serializer for objects to JSON. Only supports a limited number of type
/// (boolean, DateTimeOffset, decimal, Enum, int, long, string, and trait dictionaries).
/// </summary>
/// <remarks>
/// These types are made public for third parties only for the purpose of serializing and
/// deserializing messages that are sent across the process boundary (that is, types which
/// derived directly or indirectly from <see cref="_MessageSinkMessage"/>). Any other usage
/// is not supported.
/// </remarks>
/// <param name="buffer">The buffer to write JSON to</param>
/// <param name="disposeNotifier">An optional callback to be notified when disposed</param>
public sealed class JsonObjectSerializer(
	StringBuilder buffer,
	Action? disposeNotifier = null) :
		JsonSerializerBase(buffer, disposeNotifier, '{', '}')
{
	bool openChild;

	void GuardNoOpenChild()
	{
		if (openChild)
			throw new InvalidOperationException("There is an open child serializer that must be completed before serializing new values to this object");
	}

	/// <summary>
	/// Serialize a <see cref="bool"/> value into the object.
	/// </summary>
	/// <param name="key">The name of the value</param>
	/// <param name="value">The value</param>
	/// <param name="includeNullValues">Set to <c>true</c> to serialize a <c>null</c> value, or <c>false</c> to skip it</param>
	public void Serialize(
		string key,
		bool? value,
		bool includeNullValues = false)
	{
		GuardNoOpenChild();

		if (includeNullValues || value.HasValue)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	/// <summary>
	/// Serialize a <see cref="DateTimeOffset"/> value into the object.
	/// </summary>
	/// <param name="key">The name of the value</param>
	/// <param name="value">The value</param>
	/// <param name="includeNullValues">Set to <c>true</c> to serialize a <c>null</c> value, or <c>false</c> to skip it</param>
	public void Serialize(
		string key,
		DateTimeOffset? value,
		bool includeNullValues = false)
	{
		GuardNoOpenChild();

		if (includeNullValues || value.HasValue)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	/// <summary>
	/// Serialize a <see cref="decimal"/> value into the object.
	/// </summary>
	/// <param name="key">The name of the value</param>
	/// <param name="value">The value</param>
	/// <param name="includeNullValues">Set to <c>true</c> to serialize a <c>null</c> value, or <c>false</c> to skip it</param>
	public void Serialize(
		string key,
		decimal? value,
		bool includeNullValues = false)
	{
		GuardNoOpenChild();

		if (includeNullValues || value.HasValue)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	/// <summary>
	/// Serialize an <see cref="Enum"/> value into the object.
	/// </summary>
	/// <param name="key">The name of the value</param>
	/// <param name="value">The value</param>
	/// <param name="includeNullValues">Set to <c>true</c> to serialize a <c>null</c> value, or <c>false</c> to skip it</param>
	public void Serialize(
		string key,
		Enum? value,
		bool includeNullValues = false)
	{
		GuardNoOpenChild();

		if (includeNullValues || value is not null)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	/// <summary>
	/// Serialize an <see cref="int"/> value into the object.
	/// </summary>
	/// <param name="key">The name of the value</param>
	/// <param name="value">The value</param>
	/// <param name="includeNullValues">Set to <c>true</c> to serialize a <c>null</c> value, or <c>false</c> to skip it</param>
	public void Serialize(
		string key,
		int? value,
		bool includeNullValues = false)
	{
		GuardNoOpenChild();

		if (includeNullValues || value.HasValue)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	/// <summary>
	/// Serialize a <see cref="long"/> value into the object.
	/// </summary>
	/// <param name="key">The name of the value</param>
	/// <param name="value">The value</param>
	/// <param name="includeNullValues">Set to <c>true</c> to serialize a <c>null</c> value, or <c>false</c> to skip it</param>
	public void Serialize(
		string key,
		long? value,
		bool includeNullValues = false)
	{
		GuardNoOpenChild();

		if (includeNullValues || value.HasValue)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	/// <summary>
	/// Serialize a trait dictionary value into the object.
	/// </summary>
	/// <param name="key">The name of the value</param>
	/// <param name="dictionary">The trait dictionary</param>
	public void Serialize(
		string key,
		IReadOnlyDictionary<string, IReadOnlyList<string>> dictionary)
	{
		GuardNoOpenChild();
		Guard.ArgumentNotNull(dictionary);

		using var dictionarySerializer = SerializeObject(key);

		foreach (var kvp in dictionary)
			using (var arraySerializer = dictionarySerializer.SerializeArray(kvp.Key))
				foreach (var value in kvp.Value)
					arraySerializer.Serialize(value);
	}

	/// <summary>
	/// Serialize a <see cref="string"/> value into the object.
	/// </summary>
	/// <param name="key">The name of the value</param>
	/// <param name="value">The value</param>
	/// <param name="includeNullValues">Set to <c>true</c> to serialize a <c>null</c> value, or <c>false</c> to skip it</param>
	public void Serialize(
		string key,
		string? value,
		bool includeNullValues = false)
	{
		GuardNoOpenChild();

		if (includeNullValues || value is not null)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	/// <summary>
	/// Start serializing an array into the object.
	/// </summary>
	/// <param name="key">The name of the array</param>
	/// <remarks>
	/// IMPORTANT: This serializer must be used completely and disposed before any other value
	/// is serialized into the object, or the serialization would be corrupted.
	/// </remarks>
	public JsonArraySerializer SerializeArray(string key)
	{
		openChild = true;

		WriteKey(key);
		return new JsonArraySerializer(Buffer, () => openChild = false);
	}

	/// <summary>
	/// Start serializing an object into the object.
	/// </summary>
	/// <param name="key">The name of the object</param>
	/// <remarks>
	/// IMPORTANT: This serializer must be used completely and disposed before any other value
	/// is serialized into the object, or the serialization would be corrupted.
	/// </remarks>
	public JsonObjectSerializer SerializeObject(string key)
	{
		openChild = true;

		WriteKey(key);
		return new JsonObjectSerializer(Buffer, () => openChild = false);
	}

	void WriteKey(string key)
	{
		WriteSeparator();
		WriteValue(key);
		Buffer.Append(':');
	}
}
