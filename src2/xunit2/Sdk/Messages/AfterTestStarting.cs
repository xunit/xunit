using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IAfterTestStarting"/>.
    /// </summary>
    public class AfterTestStarting : LongLivedMarshalByRefObject, IAfterTestStarting
    {
        /// <inheritdoc/>
        public string AttributeName { get; set; }

        /// <inheritdoc/>
        public ITestCase TestCase { get; set; }

        /// <inheritdoc/>
        public string TestDisplayName { get; set; }
    }
}