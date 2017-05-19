using System;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassDisposeStarting"/>.
    /// </summary>
    [Serializable]
    public class TestClassDisposeStarting : TestMessage, ITestClassDisposeStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassDisposeStarting"/> class.
        /// </summary>
        public TestClassDisposeStarting(ITest test)
            : base(test) { }
    }
}