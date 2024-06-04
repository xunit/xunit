using System;
using System.Globalization;
using System.Text;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public abstract class JsonSerializerBase : IDisposable
{
	readonly char? close;
	bool writtenValue;

	/// <summary/>
	protected JsonSerializerBase(
		StringBuilder buffer,
		char? open = null,
		char? close = null)
	{
		Buffer = buffer;
		this.close = close;

		if (open is not null)
			Buffer.Append(open);
	}

	/// <summary/>
	public void Dispose()
	{
		GC.SuppressFinalize(this);

		if (close is not null)
			Buffer.Append(close);
	}

	/// <summary/>
	protected StringBuilder Buffer { get; }

	/// <summary/>
	protected void WriteSeparator()
	{
		if (writtenValue)
			Buffer.Append(',');

		writtenValue = true;
	}

	/// <summary/>
	protected void WriteValue(bool? value) =>
		Buffer.Append(value switch
		{
			true => "true",
			false => "false",
			_ => "null",
		});

	/// <summary/>
	protected void WriteValue(DateTimeOffset? value) =>
		WriteValue(value?.ToString("o", CultureInfo.InvariantCulture));

	/// <summary/>
	protected void WriteValue(decimal? value) =>
		Buffer.Append(value?.ToString(CultureInfo.InvariantCulture) ?? "null");

	/// <summary/>
	protected void WriteValue(Enum? value) =>
		WriteValue(value?.ToString());

	/// <summary/>
	protected void WriteValue(int? value) =>
		Buffer.Append(value?.ToString(CultureInfo.InvariantCulture) ?? "null");

	/// <summary/>
	protected void WriteValue(long? value) =>
		Buffer.Append(value?.ToString(CultureInfo.InvariantCulture) ?? "null");

	/// <summary/>
	protected void WriteValue(string? value)
	{
		if (value is null)
		{
			Buffer.Append("null");
			return;
		}

		Buffer.Append('"');

		foreach (var ch in value)
			switch (ch)
			{
				case '"': Buffer.Append("\\\""); break;
				case '\\': Buffer.Append("\\\\"); break;
				case '\b': Buffer.Append("\\b"); break;
				case '\f': Buffer.Append("\\f"); break;
				case '\n': Buffer.Append("\\n"); break;
				case '\r': Buffer.Append("\\r"); break;
				case '\t': Buffer.Append("\\t"); break;
				default: Buffer.Append(ch); break;
			}

		Buffer.Append('"');
	}
}
