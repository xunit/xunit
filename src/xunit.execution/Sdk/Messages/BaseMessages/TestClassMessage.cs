using System;
using System.Linq;
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
        public TestClassMessage(ITestCollection testCollection, string className)
            : base(testCollection)
        {
            ClassName = className;
        }

        /// <inheritdoc/>
        public string ClassName { get; private set; }
    }
}
