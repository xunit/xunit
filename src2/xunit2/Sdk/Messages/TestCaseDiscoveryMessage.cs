using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCaseDiscoveryMessage"/>.
    /// </summary>
    public class TestCaseDiscoveryMessage : LongLivedMarshalByRefObject, ITestCaseDiscoveryMessage
    {
        /// <inheritdoc/>
        public ITestCase TestCase { get; set; }
    }
}