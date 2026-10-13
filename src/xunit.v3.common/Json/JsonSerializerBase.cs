using System.Text;

namespace Xunit.Sdk;

/// <summary>
/// Base class used for streaming JSON serialization.
/// </summary>
/// <remarks>
/// These types are made public for third parties only for the purpose of serializing and
/// deserializing messages that are sent across the process boundary (that is, types which
/// implement <see cref="IMessageSinkMessage"/>). Any other usage is not supported.
/// </remarks>
public abstract class JsonSerializerBase : IDisposable
{
	readonly char? close;
	readonly Action? disposeNotifier;
	bool writtenValue;

	/// <summary>
	/// Initializes a new instance of the <see cref="JsonSerializerBase"/> class.
	/// </summary>
	/// <param name="buffer">The buffer to write JSON to</param>
	/// <param name="disposeNotifier">A callback to be notified when disposed</param>
	/// <param name="open">The character to write when starting (i.e., '[' for arrays)</param>
	/// <param name="close">The character to write when finishing (i.e., ']' for arrays)</param>
	protected JsonSerializerBase(
		StringBuilder buffer,
		Action? disposeNotifier = null,
		char? open = null,
		char? close = null)
	{
		Buffer = buffer;
		this.disposeNotifier = disposeNotifier;
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

		disposeNotifier?.Invoke();
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

		// This code is mostly the same as XmlUtility.Escape, except:
		// * The list of escaped control characters is shorter
		// * We always escape newlines
		// * We always use \uXXXX as the Unicode escaping
		for (var idx = 0; idx < value.Length; ++idx)
		{
			var ch = value[idx];
			if (ch < 32)
				Buffer.Append(ch switch
				{
					'\b' => "\\b",
					'\f' => "\\f",
					'\n' => "\\n",
					'\r' => "\\r",
					'\t' => "\\t",
					_ => string.Format(CultureInfo.InvariantCulture, @"\u{0:x4}", +ch),
				});
			else if (ch == '"')
				Buffer.Append("\\\"");
			else if (ch == '\\')
				Buffer.Append("\\\\");
			else if (char.IsSurrogatePair(value, idx))
			{
				Buffer.Append(ch);
				Buffer.Append(value[++idx]);
			}
			else if (char.IsSurrogate(ch) || ch == '\uFFFE' || ch == '\uFFFF')
				Buffer.Append(string.Format(CultureInfo.InvariantCulture, @"\u{0:x4}", +ch));
			else
				Buffer.Append(ch);
		}

		Buffer.Append('"');
	}

	/// <summary/>
	protected void WriteValue(Version? value) =>
		WriteValue(value?.ToString());
}
