using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestMethodFinished"/>.
    /// </summary>
    public class TestMethodFinished : LongLivedMarshalByRefObject, ITestMethodFinished
    {
        /// <inheritdoc/>
        public string ClassName { get; set; }

        /// <inheritdoc/>
        public string MethodName { get; set; }
    }
}