using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassMessage"/>.
    /// </summary>
    public class TestClassMessage : TestCollectionMessage, ITestClassMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassMessage"/> class.
        /// </summary>
        public TestClassMessage(IEnumerable<ITestCase> testCases, ITestClass testClass)
            : base(testCases, testClass.TestCollection)
        {
            TestClass = testClass;
        }

        /// <inheritdoc/>
        public ITestClass TestClass { get; set; }
    }
}
