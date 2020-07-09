using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassConstructionFinished"/>.
    /// </summary>
    public class TestClassConstructionFinished : TestMessage, ITestClassConstructionFinished
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassConstructionFinished"/> class.
        /// </summary>
        public TestClassConstructionFinished(ITest test)
            : base(test) { }
    }
}
