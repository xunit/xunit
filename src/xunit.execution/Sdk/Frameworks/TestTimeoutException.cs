using System;
using System.Globalization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Thrown if a test exceeds the specified timeout.
    /// </summary>
    public class TestTimeoutException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestTimeoutException"/> class, returning a
        /// message indicating that the test method isn't compatible with timeout functionality.
        /// </summary>
        public TestTimeoutException()
            : base("Tests marked with Timeout are only supported for async tests")
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestTimeoutException"/> class, returning a
        /// message indicating that the test method timed out.
        /// </summary>
        /// <param name="timeout">The timeout that was exceeded, in milliseconds</param>
        public TestTimeoutException(int timeout)
            : base(string.Format(CultureInfo.CurrentCulture, "Test execution timed out after {0} milliseconds", timeout)) { }
    }
}
