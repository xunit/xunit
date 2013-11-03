using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestMethodFinished"/>.
    /// </summary>
    internal class TestMethodFinished : TestCollectionMessage, ITestMethodFinished
    {
        public TestMethodFinished(ITestCollection testCollection, string className, string methodName)
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