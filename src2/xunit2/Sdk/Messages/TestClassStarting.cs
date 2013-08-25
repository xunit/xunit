using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassStarting"/>.
    /// </summary>
    public class TestClassStarting : TestCollectionMessage, ITestClassStarting
    {
        public TestClassStarting(ITestCollection testCollection, string className)
            : base(testCollection)
        {
            ClassName = className;
        }

        /// <inheritdoc/>
        public string ClassName { get; private set; }
    }
}