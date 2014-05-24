using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestCollectionStarting"/>.
    /// </summary>
    public class TestCollectionStarting : TestCollectionMessage, ITestCollectionStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCollectionStarting"/> class.
        /// </summary>
        public TestCollectionStarting(ITestCollection testCollection)
            : base(testCollection) { }
    }
}