using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassStarting"/>.
    /// </summary>
    internal class TestClassStarting : TestClassMessage, ITestClassStarting
    {
        public TestClassStarting(ITestCollection testCollection, string className)
            : base(testCollection, className) { }
    }
}