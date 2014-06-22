using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyMessage"/>.
    /// </summary>
    public class TestAssemblyMessage : LongLivedMarshalByRefObject, ITestAssemblyMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyMessage"/> class.
        /// </summary>
        public TestAssemblyMessage(IEnumerable<ITestCase> testCases, ITestAssembly testAssembly)
        {
            TestAssembly = testAssembly;
            TestCases = testCases;
        }

        /// <inheritdoc/>
        public ITestAssembly TestAssembly { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ITestCase> TestCases { get; private set; }

    }
}
