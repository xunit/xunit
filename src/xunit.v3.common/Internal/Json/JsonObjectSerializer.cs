using System;
using System.Collections.Generic;
using System.Text;

namespace Xunit.Internal;

internal sealed class JsonObjectSerializer(StringBuilder buffer) : JsonSerializerBase(buffer, '{', '}')
{
	public void Serialize(
		string key,
		bool? value)
	{
		if (value.HasValue)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	public void Serialize(
		string key,
		DateTimeOffset? value)
	{
		if (value.HasValue)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	public void Serialize(
		string key,
		decimal? value)
	{
		if (value.HasValue)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	public void Serialize(
		string key,
		Enum? value)
	{
		if (value is not null)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	public void Serialize(
		string key,
		int? value)
	{
		if (value.HasValue)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	public void Serialize(
		string key,
		IReadOnlyDictionary<string, IReadOnlyList<string>> dictionary)
	{
		using var dictionarySerializer = SerializeObject(key);

		foreach (var kvp in dictionary)
			using (var arraySerializer = dictionarySerializer.SerializeArray(kvp.Key))
				foreach (var value in kvp.Value)
					arraySerializer.Serialize(value);
	}

	public void Serialize(
		string key,
		string? value)
	{
		if (value is not null)
		{
			WriteKey(key);
			WriteValue(value);
		}
	}

	public JsonArraySerializer SerializeArray(string key)
	{
		WriteKey(key);
		return new JsonArraySerializer(Buffer);
	}

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
