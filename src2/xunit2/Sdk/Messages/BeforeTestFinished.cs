using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IBeforeTestFinished"/>.
    /// </summary>
    public class BeforeTestFinished : LongLivedMarshalByRefObject, IBeforeTestFinished
    {
        /// <inheritdoc/>
        public string AttributeName { get; set; }

        /// <inheritdoc/>
        public ITestCase TestCase { get; set; }

        /// <inheritdoc/>
        public string TestDisplayName { get; set; }
    }
}