using System;

namespace Xunit.Sdk
{
	/// <summary>
	/// Thrown if a test exceeds the specified timeout.
	/// </summary>
	public class TestTimeoutException : Exception, ITestTimeoutException
	{
		TestTimeoutException(string message)
			: base(message)
		{ }

		/// <summary>
		/// Creates a new instance of <see cref="TestTimeoutException"/> for a test which is
		/// not compatible with timeout.
		/// </summary>
		public static TestTimeoutException ForIncompatibleTest() =>
			new($"Tests marked with Timeout are only supported for async tests");

		/// <summary>
		/// Creates a new instance of <see cref="TestTimeoutException"/> for a test that has
		/// timed out.
		/// </summary>
		/// <param name="timeout">The timeout that was exceeded, in milliseconds</param>
		public static TestTimeoutException ForTimedOutTest(int timeout) =>
			new($"Test execution timed out after {timeout} milliseconds");
	}
}
