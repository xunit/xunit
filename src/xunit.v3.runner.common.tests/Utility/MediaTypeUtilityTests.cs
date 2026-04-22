using Xunit;

public class MediaTypeUtilityTests
{
	[Fact]
	public void ReplacesCrossPlatformInvalidChars()
	{
		// Chars invalid on Windows must be scrubbed regardless of the runtime OS, since
		// attachments written on one OS are frequently consumed on another (e.g. uploaded
		// as GitHub Actions artifacts, which rejects ':', '"', '<', '>', '|', '*', '?').
		var result = MediaTypeUtility.GetSanitizedFileNameWithExtension("name:with<>|\"*?/\\bad", "text/plain");

		Assert.Equal("name_with________bad.txt", result);
	}

	[Fact]
	public void ReplacesControlChars()
	{
		var result = MediaTypeUtility.GetSanitizedFileNameWithExtension("a\0b\tc\nd", "text/plain");

		Assert.Equal("a_b_c_d.txt", result);
	}

	[Fact]
	public void AppendsExtensionWhenMissing()
	{
		var result = MediaTypeUtility.GetSanitizedFileNameWithExtension("attachment", "application/json");

		Assert.Equal("attachment.json", result);
	}

	[Fact]
	public void DoesNotAppendExtensionWhenAlreadyPresent()
	{
		var result = MediaTypeUtility.GetSanitizedFileNameWithExtension("attachment.json", "application/json");

		Assert.Equal("attachment.json", result);
	}

	[Fact]
	public void UnknownMediaTypeUsesBinExtension()
	{
		var result = MediaTypeUtility.GetSanitizedFileNameWithExtension("attachment", "application/x-not-a-real-type");

		Assert.Equal("attachment.bin", result);
	}
}
