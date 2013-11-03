using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestFinished"/>.
    /// </summary>
    internal class TestFinished : TestMessage, ITestFinished
    {
        public TestFinished(ITestCase testCase, string testDisplayName, decimal executionTime)
            : base(testCase, testDisplayName)
        {
            ExecutionTime = executionTime;
        }

        /// <inheritdoc/>
        public decimal ExecutionTime { get; private set; }
    }
}