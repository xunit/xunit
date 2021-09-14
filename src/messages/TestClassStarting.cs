using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
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
        public TestClassStarting(IEnumerable<ITestCase> testCases, ITestClass testClass)
            : base(testCases, testClass) { }
    }
}
