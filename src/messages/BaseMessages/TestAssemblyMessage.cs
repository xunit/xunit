using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyMessage"/> and <see cref="IExecutionMessage"/>.
    /// </summary>
    public class TestAssemblyMessage : LongLivedMarshalByRefObject, ITestAssemblyMessage, IExecutionMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyMessage"/> class.
        /// </summary>
        public TestAssemblyMessage(IEnumerable<ITestCase> testCases, ITestAssembly testAssembly)
        {
            TestAssembly = testAssembly;
            TestCases = testCases.ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyMessage"/> class.
        /// </summary>
        internal TestAssemblyMessage(ITestCase testCase, ITestAssembly testAssembly)
        {
            TestAssembly = testAssembly;
            TestCases = new ITestCase[] { testCase };
        }

        /// <inheritdoc/>
        public ITestAssembly TestAssembly { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ITestCase> TestCases { get; private set; }

    }
}
