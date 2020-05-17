using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="IAfterTestStarting"/>.
    /// </summary>
    public class AfterTestStarting : TestMessage, IAfterTestStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AfterTestStarting"/> class.
        /// </summary>
        public AfterTestStarting(ITest test, string attributeName)
            : base(test)
        {
            AttributeName = attributeName;
        }

        /// <inheritdoc/>
        public string AttributeName { get; private set; }
    }
}
