using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestMethodMessage"/>.
    /// </summary>
    public class TestMethodMessage : TestClassMessage, ITestMethodMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethodMessage"/> class.
        /// </summary>
        public TestMethodMessage(ITestCollection testCollection, string className, string methodName)
            : base(testCollection, className)
        {
            MethodName = methodName;
        }

        /// <inheritdoc/>
        public string MethodName { get; private set; }
    }
}
