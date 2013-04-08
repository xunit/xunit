using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCaseStarting"/>.
    /// </summary>
    public class TestCaseStarting : LongLivedMarshalByRefObject, ITestCaseStarting
    {
        /// <inheritdoc/>
        public ITestCase TestCase { get; set; }
    }
}