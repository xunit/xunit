using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestMethodStarting : LongLivedMarshalByRefObject, ITestMethodStarting
    {
        /// <inheritdoc/>
        public string ClassName { get; set; }

        /// <inheritdoc/>
        public string MethodName { get; set; }
    }
}