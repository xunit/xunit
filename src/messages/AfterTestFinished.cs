using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="IAfterTestFinished"/>.
    /// </summary>
    public class AfterTestFinished : TestMessage, IAfterTestFinished
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AfterTestFinished"/> class.
        /// </summary>
        public AfterTestFinished(ITest test, string attributeName)
            : base(test)
        {
            AttributeName = attributeName;
        }

        /// <inheritdoc/>
        public string AttributeName { get; private set; }
    }
}
