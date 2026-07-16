using Microsoft.Testing.Platform.TestHost;

namespace Xunit;

/// <summary>
/// Extension methods for <see cref="ITestContext"/>.
/// </summary>
public static class ITestContextExtensions
{
	const string mtpSessionKey = "microsoft-testing-platform-session-uid";

	/// <summary>
	/// Gets the current Microsoft Testing Platform session UID.
	/// </summary>
	/// <returns>The current MTP session UID, when running inside an MTP session; <see langword="null"/>, otherwise.</returns>
	public static SessionUid? MicrosoftTestingPlatformSession(this ITestContext testContext)
	{
		Guard.ArgumentNotNull(testContext).KeyValueStorage.TryGetValue(mtpSessionKey, out var value);

		return
			value is string sessionUid
				? new SessionUid(sessionUid)
				: null;
	}

	internal static void SetMicrosoftTestingPlatformSession(
		this ITestContext testContext,
		SessionUid sessionUid) =>
			Guard.ArgumentNotNull(testContext).KeyValueStorage[mtpSessionKey] = sessionUid.Value;
}
