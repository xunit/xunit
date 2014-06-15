using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestCollectionMessage"/>.
    /// </summary>
    public class TestCollectionMessage : LongLivedMarshalByRefObject, ITestCollectionMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCollectionMessage"/> class.
        /// </summary>
        public TestCollectionMessage(ITestCollection testCollection)
        {
            TestCollection = testCollection;
        }

        /// <inheritdoc/>
        public ITestCollection TestCollection { get; private set; }
    }
}
