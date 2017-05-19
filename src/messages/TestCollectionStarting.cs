using System;
using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestCollectionStarting"/>.
    /// </summary>
    [Serializable]
    public class TestCollectionStarting : TestCollectionMessage, ITestCollectionStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCollectionStarting"/> class.
        /// </summary>
        public TestCollectionStarting(IEnumerable<ITestCase> testCases, ITestCollection testCollection)
            : base(testCases, testCollection) { }
    }
}