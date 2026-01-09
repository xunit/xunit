using System.Globalization;
using System.Text;
using System.Xml;

namespace Xunit.Runner.Common;

internal static class XmlUtility
{
	public static readonly XmlWriterSettings HtmlWriterSettings = new()
	{
		Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
		OmitXmlDeclaration = true,
		Indent = true,
		IndentChars = "  ",
	};

	public static readonly XmlWriterSettings WriterSettings = new()
	{
		Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
		OmitXmlDeclaration = true,
	};

	public static string Escape(
		string? value,
		bool escapeNewlines = true)
	{
		if (value == null)
			return string.Empty;

		var escapedValue = new StringBuilder(value.Length + 20);
		for (var idx = 0; idx < value.Length; ++idx)
		{
			var ch = value[idx];
			if (ch < 32)
				escapedValue.Append(ch switch
				{
					'\0' => "\\0",
					'\a' => "\\a",
					'\b' => "\\b",
					'\f' => "\\f",
					'\n' => escapeNewlines ? "\\n" : "\n",
					'\r' => escapeNewlines ? "\\r" : "\r",
					'\t' => "\\t",
					'\v' => "\\v",
					_ => string.Format(CultureInfo.InvariantCulture, @"\x{0:x2}", +ch),
				});
			else if (ch == '"')
				escapedValue.Append("\\\"");
			else if (ch == '\\')
				escapedValue.Append("\\\\");
			else if (char.IsSurrogatePair(value, idx)) // Takes care of the case when idx + 1 == value.Length
			{
				escapedValue.Append(ch); // Append valid surrogate chars like normal
				escapedValue.Append(value[++idx]);
			}
			// Check for invalid chars and append them like \x----
			else if (char.IsSurrogate(ch) || ch == '\uFFFE' || ch == '\uFFFF')
				escapedValue.Append(string.Format(CultureInfo.InvariantCulture, @"\x{0:x4}", +ch));
			else
				escapedValue.Append(ch);
		}

		return escapedValue.ToString();
	}
}
