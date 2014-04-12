using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestMethodStarting"/>.
    /// </summary>
    public class TestMethodStarting : TestCollectionMessage, ITestMethodStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethodStarting"/> class.
        /// </summary>
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