using System.Text;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public sealed class JsonArraySerializer(StringBuilder buffer) : JsonSerializerBase(buffer, '[', ']')
{
	/// <summary/>
	public void Serialize(int? value)
	{
		WriteSeparator();
		WriteValue(value);
	}

	/// <summary/>
	public void Serialize(string? value)
	{
		WriteSeparator();
		WriteValue(value);
	}

	/// <summary/>
	public JsonArraySerializer SerializeArray()
	{
		WriteSeparator();
		return new JsonArraySerializer(Buffer);
	}

	/// <summary/>
	public JsonObjectSerializer SerializeObject()
	{
		WriteSeparator();
		return new JsonObjectSerializer(Buffer);
	}
}
