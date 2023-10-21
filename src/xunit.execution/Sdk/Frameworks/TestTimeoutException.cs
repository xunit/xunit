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
        /// Initializes a new instance of <see cref="TestTimeoutException"/>.
        /// </summary>
        /// <param name="timeout">The timeout that was exceeded, in milliseconds</param>
        public TestTimeoutException(int timeout)
            : base(string.Format(CultureInfo.CurrentCulture, "Test execution timed out after {0} milliseconds", timeout)) { }
    }
}
