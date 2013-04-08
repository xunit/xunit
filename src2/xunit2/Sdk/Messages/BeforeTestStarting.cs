using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IBeforeTestStarting"/>.
    /// </summary>
    public class BeforeTestStarting : LongLivedMarshalByRefObject, IBeforeTestStarting
    {
        /// <inheritdoc/>
        public string AttributeName { get; set; }

        /// <inheritdoc/>
        public ITestCase TestCase { get; set; }

        /// <inheritdoc/>
        public string TestDisplayName { get; set; }
    }
}