using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Represents an attachment to a test result.
/// </summary>
public class TestAttachment
{
	const string BinaryHeader = "b:";
	const string StringHeader = "s:";
	static readonly Regex MediaTypeRegex = new("^\\w+\\/[-.\\w]+(?:\\+[-.\\w]+)?$", RegexOptions.Compiled);

	readonly byte[]? byteArrayValue;
	readonly string? mediaType;
	readonly string? stringValue;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public TestAttachment()
	{ }

	TestAttachment(
		byte[] byteArrayValue,
		string mediaType)
	{
		this.byteArrayValue = byteArrayValue;
		this.mediaType = mediaType;
	}

	TestAttachment(string stringValue) =>
		this.stringValue = stringValue;

	/// <summary>
	/// Gets the type of the test attachment.
	/// </summary>
	public TestAttachmentType AttachmentType =>
		byteArrayValue is not null ? TestAttachmentType.ByteArray : TestAttachmentType.String;

	/// <summary>
	/// Gets the attachment content as a byte array, along with the media type. Can only be called
	/// when <see cref="AttachmentType"/> returns <see cref="TestAttachmentType.ByteArray"/>.
	/// </summary>
	public (byte[] ByteArray, string MediaType) AsByteArray() =>
		(
			byteArrayValue ?? throw new InvalidOperationException("The test attachment value is not a byte array"),
			mediaType ?? throw new InvalidOperationException("The test attachment value is not a byte array")
		);

	/// <summary>
	/// Gets the attachment content as a string. Can only be called when <see cref="AttachmentType"/>
	/// returns <see cref="TestAttachmentType.String"/>.
	/// </summary>
	public string AsString() =>
		stringValue ?? throw new InvalidOperationException("The test attachment value is not a string");

	/// <summary>
	/// Creates an instance of <see cref="TestAttachment"/> that wraps a byte array value.
	/// </summary>
	/// <param name="value">The byte array value</param>
	/// <param name="mediaType">The media type</param>
	/// <remarks>
	/// The <paramref name="mediaType"/> value must be in the MIME "type/subtype" form, and does not support
	/// parameter values. The subtype is allowed to have a single "+" to denote specialization of the
	/// subtype (i.e., "application/xhtml+xml"). For more information on media types, see
	/// <see href="https://datatracker.ietf.org/doc/html/rfc2045#section-5.1"/>.
	/// </remarks>
	public static TestAttachment Create(
		byte[] value,
		string mediaType)
	{
		Guard.ArgumentNotNull(value);
		Guard.ArgumentNotNull(mediaType);
		Guard.ArgumentValid("Media type is an invalid format", MediaTypeRegex.Match(mediaType).Success, nameof(mediaType));

		return new(value, mediaType);
	}

	/// <summary>
	/// Creates an instance of <see cref="TestAttachment"/> that wraps a string value.
	/// </summary>
	public static TestAttachment Create(string value) =>
		new(value);

	/// <summary>
	/// Parses a value that was obtained by calling <see cref="ToString"/> back into a <see cref="TestAttachment"/>.
	/// </summary>
	public static TestAttachment Parse(string value)
	{
		Guard.ArgumentNotNull(value);

		if (value.StartsWith(StringHeader, StringComparison.Ordinal))
			return new(value.Substring(StringHeader.Length));

		if (value.StartsWith(BinaryHeader, StringComparison.Ordinal))
		{

			value = value.Substring(BinaryHeader.Length);
			var semiColonIdx = value.IndexOf(';');
			if (semiColonIdx > -1)
			{
				var mediaType = value.Substring(0, semiColonIdx);
				var bytes = Convert.FromBase64String(value.Substring(semiColonIdx + 1));
				return new(bytes, mediaType);
			}
		}

		throw new ArgumentException("Improperly encoded attachment value");
	}

	/// <summary>
	/// Gets a string value for the test attachment. For string value attachments, will return the string value;
	/// for byte array values, it will return the base-64 encoded value of the bytes.
	/// </summary>
	public override string ToString() =>
		stringValue is not null
			? StringHeader + stringValue
			: byteArrayValue is not null
				? string.Format(CultureInfo.InvariantCulture, "{0}{1};{2}", BinaryHeader, mediaType, Convert.ToBase64String(byteArrayValue))
				: "<unset>";
}
