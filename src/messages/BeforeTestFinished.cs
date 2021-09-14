using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="IBeforeTestFinished"/>.
    /// </summary>
    public class BeforeTestFinished : TestMessage, IBeforeTestFinished
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeforeTestFinished"/> class.
        /// </summary>
        public BeforeTestFinished(ITest test, string attributeName)
            : base(test)
        {
            AttributeName = attributeName;
        }

        /// <inheritdoc/>
        public string AttributeName { get; private set; }
    }
}
