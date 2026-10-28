using Xunit.Sdk;

namespace Xunit.MicrosoftTestingPlatform;

/// <summary>
/// An exception type that is used to <see cref="TestPlatformExecutionMessageSink"/> to report failed
/// test exception information back to Microsoft.Testing.Platform.
/// </summary>
/// <param name="metadata"></param>
/// <param name="messageHeader">The optional header to insert before the combined messages</param>
/// <remarks>
/// This class is an implementation detail for Microsoft.Testing.Platform that is public for testing purposes.
/// Use this class at your own risk, as breaking changes may occur as needed.
/// </remarks>
[Serializable]
public sealed class XunitException(
	IErrorMetadata metadata,
	string? messageHeader = null) :
		Exception(CombineMessages(metadata, messageHeader))
{
	/// <inheritdoc/>
	public override string? StackTrace { get; } = ExceptionUtility.CombineStackTraces(metadata);

	static string CombineMessages(
		IErrorMetadata metadata,
		string? messageHeader)
	{
		var result = ExceptionUtility.CombineMessages(metadata);

		if (!string.IsNullOrWhiteSpace(messageHeader))
		{
			if (result.Length == 0)
				result = messageHeader;
			else
				result = messageHeader + ": " + result;
		}

		return result;
	}
}
