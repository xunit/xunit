#nullable enable

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
            Guard.ArgumentNotNull(nameof(attributeName), attributeName);

            AttributeName = attributeName;
        }

        /// <inheritdoc/>
        public string AttributeName { get; }
    }
}
