using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestCollectionMessage"/>.
    /// </summary>
    public class TestCollectionMessage : TestAssemblyMessage, ITestCollectionMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCollectionMessage"/> class.
        /// </summary>
        public TestCollectionMessage(IEnumerable<ITestCase> testCases, ITestCollection testCollection)
            : base(testCases, testCollection.TestAssembly)
        {
            TestCollection = testCollection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCollectionMessage"/> class.
        /// </summary>
        internal TestCollectionMessage(ITestCase testCase, ITestCollection testCollection)
            : base(testCase, testCollection.TestAssembly)
        {
            TestCollection = testCollection;
        }

        /// <inheritdoc/>
        public ITestCollection TestCollection { get; private set; }
    }
}
