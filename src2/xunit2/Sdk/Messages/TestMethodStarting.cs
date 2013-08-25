using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestMethodStarting"/>.
    /// </summary>
    public class TestMethodStarting : TestCollectionMessage, ITestMethodStarting
    {
        public TestMethodStarting(ITestCollection testCollection, string className, string methodName)
            : base(testCollection)
        {
            ClassName = className;
            MethodName = methodName;
        }

        /// <inheritdoc/>
        public string ClassName { get; private set; }

        /// <inheritdoc/>
        public string MethodName { get; private set; }
    }
}