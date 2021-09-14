using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="IBeforeTestStarting"/>.
    /// </summary>
    public class BeforeTestStarting : TestMessage, IBeforeTestStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeforeTestStarting"/> class.
        /// </summary>
        public BeforeTestStarting(ITest test, string attributeName)
            : base(test)
        {
            AttributeName = attributeName;
        }

        /// <inheritdoc/>
        public string AttributeName { get; private set; }
    }
}
