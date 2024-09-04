using System;
using System.Globalization;
using Xunit.v3;

// This lives in Xunit.Sdk instead of Xunit.v3 because our message filter will only simplify exception
// names in the "Xunit.Sdk" namespace. See ExceptionUtility.GetMessage for more information.
namespace Xunit.Sdk;

/// <summary>
/// Thrown if a test exceeds the specified timeout.
/// </summary>
[Serializable]
public class TestTimeoutException : Exception, ITestTimeoutException
{
	TestTimeoutException(string message)
		: base(message)
	{ }

	/// <summary>
	/// Creates a new instance of <see cref="TestTimeoutException"/> for a test that has
	/// timed out.
	/// </summary>
	/// <param name="timeout">The timeout that was exceeded, in milliseconds</param>
	public static TestTimeoutException ForTimedOutTest(int timeout) =>
		new(string.Format(CultureInfo.CurrentCulture, "Test execution timed out after {0} milliseconds", timeout));
}
