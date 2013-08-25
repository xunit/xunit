using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IBeforeTestFinished"/>.
    /// </summary>
    public class BeforeTestFinished : TestMessage, IBeforeTestFinished
    {
        public BeforeTestFinished(ITestCase testCase, string testDisplayName, string attributeName)
            : base(testCase, testDisplayName)
        {
            AttributeName = attributeName;
        }

        /// <inheritdoc/>
        public string AttributeName { get; private set; }
    }
}