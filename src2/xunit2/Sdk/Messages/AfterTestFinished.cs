using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IAfterTestFinished"/>.
    /// </summary>
    public class AfterTestFinished : LongLivedMarshalByRefObject, IAfterTestFinished
    {
        /// <inheritdoc/>
        public string AttributeName { get; set; }

        /// <inheritdoc/>
        public ITestCase TestCase { get; set; }

        /// <inheritdoc/>
        public string TestDisplayName { get; set; }
    }
}