using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestResultMessage"/>.
    /// </summary>
    public class TestResultMessage : TestMessage, ITestResultMessage
    {
        public TestResultMessage(ITestCase testCase, string testDisplayName, decimal executionTime)
            : base(testCase, testDisplayName)
        {
            ExecutionTime = executionTime;
        }

        /// <inheritdoc/>
        public decimal ExecutionTime { get; private set; }
    }
}