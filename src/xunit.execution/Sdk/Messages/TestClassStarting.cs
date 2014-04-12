using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassStarting"/>.
    /// </summary>
    public class TestClassStarting : TestClassMessage, ITestClassStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassStarting"/> class.
        /// </summary>
        public TestClassStarting(ITestCollection testCollection, string className)
            : base(testCollection, className) { }
    }
}