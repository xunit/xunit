using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IAfterTestStarting"/>.
    /// </summary>
    internal class AfterTestStarting : TestMessage, IAfterTestStarting
    {
        public AfterTestStarting(ITestCase testCase, string testDisplayName, string attributeName)
            : base(testCase, testDisplayName)
        {
            AttributeName = attributeName;
        }

        /// <inheritdoc/>
        public string AttributeName { get; private set; }
    }
}