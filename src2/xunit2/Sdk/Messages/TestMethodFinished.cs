using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestMethodFinished : LongLivedMarshalByRefObject, ITestMethodFinished
    {
        /// <inheritdoc/>
        public string ClassName { get; set; }

        /// <inheritdoc/>
        public string MethodName { get; set; }
    }
}