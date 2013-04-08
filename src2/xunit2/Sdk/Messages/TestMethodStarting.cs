using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestMethodStarting"/>.
    /// </summary>
    public class TestMethodStarting : LongLivedMarshalByRefObject, ITestMethodStarting
    {
        /// <inheritdoc/>
        public string ClassName { get; set; }

        /// <inheritdoc/>
        public string MethodName { get; set; }
    }
}