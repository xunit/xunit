using System.Text;

namespace Xunit.Internal;

internal sealed class JsonArraySerializer(StringBuilder buffer) : JsonSerializerBase(buffer, '[', ']')
{
	public void Serialize(int? value)
	{
		WriteSeparator();
		WriteValue(value);
	}

	public void Serialize(string? value)
	{
		WriteSeparator();
		WriteValue(value);
	}
}
