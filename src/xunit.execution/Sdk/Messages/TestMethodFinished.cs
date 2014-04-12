using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestMethodFinished"/>.
    /// </summary>
    public class TestMethodFinished : TestCollectionMessage, ITestMethodFinished
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethodFinished"/> class.
        /// </summary>
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