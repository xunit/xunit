using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassDisposeFinished"/>.
    /// </summary>
    public class TestClassDisposeFinished : TestMessage, ITestClassDisposeFinished
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassDisposeFinished"/> class.
        /// </summary>
        public TestClassDisposeFinished(ITest test)
            : base(test) { }
    }
}
