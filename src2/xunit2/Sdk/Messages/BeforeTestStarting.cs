using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IBeforeTestStarting"/>.
    /// </summary>
    internal class BeforeTestStarting : TestMessage, IBeforeTestStarting
    {
        public BeforeTestStarting(ITestCase testCase, string testDisplayName, string attributeName)
            : base(testCase, testDisplayName)
        {
            AttributeName = attributeName;
        }

        /// <inheritdoc/>
        public string AttributeName { get; private set; }
    }
}