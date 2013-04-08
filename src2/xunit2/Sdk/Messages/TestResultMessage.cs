using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestResultMessage"/>.
    /// </summary>
    public class TestResultMessage : LongLivedMarshalByRefObject, ITestResultMessage
    {
        /// <inheritdoc/>
        public string TestDisplayName { get; set; }

        /// <inheritdoc/>
        public decimal ExecutionTime { get; set; }

        /// <inheritdoc/>
        public ITestCase TestCase { get; set; }
    }
}