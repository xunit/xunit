using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// A special-purpose streaming serializer for arrays to JSON. Only supports a limited number of
/// types (boolean, DateTimeOffset, decimal, Enum, int, long, string, and trait dictionaries).
/// </summary>
/// <remarks>
/// These types are made public for third parties only for the purpose of serializing and
/// deserializing messages that are sent across the process boundary (that is, types which
/// implement <see cref="IMessageSinkMessage"/>). Any other usage is not supported.
/// </remarks>
/// <param name="buffer">The buffer to write JSON to</param>
/// <param name="disposeNotifier">An optional callback to be notified when disposed</param>
public sealed class JsonArraySerializer(
	StringBuilder buffer,
	Action? disposeNotifier = null) :
		JsonSerializerBase(buffer, disposeNotifier, '[', ']')
{
	bool openChild;

	void GuardNoOpenChild()
	{
		if (openChild)
			throw new InvalidOperationException("There is an open child serializer that must be completed before serializing new values to this object");
	}

	/// <summary>
	/// Serialize a <see cref="bool"/> value into the array.
	/// </summary>
	public void Serialize(bool? value)
	{
		GuardNoOpenChild();

		WriteSeparator();
		WriteValue(value);
	}

	/// <summary>
	/// Serialize a <see cref="DateTimeOffset"/> value into the array.
	/// </summary>
	/// <param name="value"></param>
	public void Serialize(DateTimeOffset? value)
	{
		GuardNoOpenChild();

		WriteSeparator();
		WriteValue(value);
	}

	/// <summary>
	/// Serialize a <see cref="decimal"/> value into the array.
	/// </summary>
	public void Serialize(decimal? value)
	{
		GuardNoOpenChild();

		WriteSeparator();
		WriteValue(value);
	}

	/// <summary>
	/// Serialize an <see cref="Enum"/> value into the array.
	/// </summary>
	public void Serialize(Enum? value)
	{
		GuardNoOpenChild();

		WriteSeparator();
		WriteValue(value);
	}

	/// <summary>
	/// Serialize an <see cref="int"/> value into the array.
	/// </summary>
	public void Serialize(int? value)
	{
		GuardNoOpenChild();

		WriteSeparator();
		WriteValue(value);
	}

	/// <summary>
	/// Serialize a <see cref="long"/> value into the array.
	/// </summary>
	public void Serialize(long? value)
	{
		GuardNoOpenChild();

		WriteSeparator();
		WriteValue(value);
	}

	/// <summary>
	/// Serialize a trait dictionary value into the array.
	/// </summary>
	public void Serialize(IReadOnlyDictionary<string, IReadOnlyCollection<string>> dictionary)
	{
		GuardNoOpenChild();
		Guard.ArgumentNotNull(dictionary);

		WriteSeparator();

		using var dictionarySerializer = SerializeObject();

		foreach (var kvp in dictionary)
			using (var arraySerializer = dictionarySerializer.SerializeArray(kvp.Key))
				foreach (var value in kvp.Value)
					arraySerializer.Serialize(value);
	}

	/// <summary>
	/// Serialize a <see cref="string"/> value into the array.
	/// </summary>
	public void Serialize(string? value)
	{
		GuardNoOpenChild();

		WriteSeparator();
		WriteValue(value);
	}

	/// <summary>
	/// Start serializing an array into the array.
	/// </summary>
	/// <remarks>
	/// IMPORTANT: This serializer must be used completely and disposed before any other value
	/// is serialized into the array, or the serialization will be corrupted.
	/// </remarks>
	public JsonArraySerializer SerializeArray()
	{
		openChild = true;

		WriteSeparator();
		return new JsonArraySerializer(Buffer, () => openChild = false);
	}

	/// <summary>
	/// Start serializing an object into the array.
	/// </summary>
	/// <remarks>
	/// IMPORTANT: This serializer must be used completely and disposed before any other value
	/// is serialized into the array, or the serialization will be corrupted.
	/// </remarks>
	public JsonObjectSerializer SerializeObject()
	{
		WriteSeparator();
		return new JsonObjectSerializer(Buffer, () => openChild = false);
	}
}
