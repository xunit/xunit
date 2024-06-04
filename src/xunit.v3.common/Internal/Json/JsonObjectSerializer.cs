using System;
using System.Collections.Generic;
using System.Text;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public sealed class JsonObjectSerializer(StringBuilder buffer) : JsonSerializerBase(buffer, '{', '}')
{
	/// <summary/>
	public void Serialize(
		string key,
		bool? value,
		bool includeNullValues = false)
	{
		if (includeNullValues || value.HasValue)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	/// <summary/>
	public void Serialize(
		string key,
		DateTimeOffset? value,
		bool includeNullValues = false)
	{
		if (includeNullValues || value.HasValue)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	/// <summary/>
	public void Serialize(
		string key,
		decimal? value,
		bool includeNullValues = false)
	{
		if (includeNullValues || value.HasValue)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	/// <summary/>
	public void Serialize(
		string key,
		Enum? value,
		bool includeNullValues = false)
	{
		if (includeNullValues || value is not null)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	/// <summary/>
	public void Serialize(
		string key,
		int? value,
		bool includeNullValues = false)
	{
		if (includeNullValues || value.HasValue)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	/// <summary/>
	public void Serialize(
		string key,
		long? value,
		bool includeNullValues = false)
	{
		if (includeNullValues || value.HasValue)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	/// <summary/>
	public void Serialize(
		string key,
		IReadOnlyDictionary<string, IReadOnlyList<string>> dictionary)
	{
		Guard.ArgumentNotNull(dictionary);

		using var dictionarySerializer = SerializeObject(key);

		foreach (var kvp in dictionary)
			using (var arraySerializer = dictionarySerializer.SerializeArray(kvp.Key))
				foreach (var value in kvp.Value)
					arraySerializer.Serialize(value);
	}

	/// <summary/>
	public void Serialize(
		string key,
		string? value,
		bool includeNullValues = false)
	{
		if (includeNullValues || value is not null)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	/// <summary/>
	public JsonArraySerializer SerializeArray(string key)
	{
		WriteKey(key);
		return new JsonArraySerializer(Buffer);
	}

	/// <summary/>
	public JsonObjectSerializer SerializeObject(string key)
	{
		WriteKey(key);
		return new JsonObjectSerializer(Buffer);
	}

	void WriteKey(string key)
	{
		WriteSeparator();
		WriteValue(key);
		Buffer.Append(':');
	}
}
