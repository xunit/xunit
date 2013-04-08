using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestStarting"/>.
    /// </summary>
    public class TestStarting : LongLivedMarshalByRefObject, ITestStarting
    {
        /// <inheritdoc/>
        public ITestCase TestCase { get; set; }

        /// <inheritdoc/>
        public string TestDisplayName { get; set; }
    }
}