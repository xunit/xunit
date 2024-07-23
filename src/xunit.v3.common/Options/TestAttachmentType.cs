namespace Xunit.Sdk;

/// <summary>
/// Gets the type of the test attachment
/// </summary>
public enum TestAttachmentType
{
	/// <summary>
	/// Indicates a test attachment that is a string.
	/// </summary>
	String = 1,

	/// <summary>
	/// Indicates a test attachment that is an array of bytes.
	/// </summary>
	ByteArray = 2,
}
