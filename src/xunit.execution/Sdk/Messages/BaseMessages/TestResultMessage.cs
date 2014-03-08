using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestResultMessage"/>.
    /// </summary>
    internal class TestResultMessage : TestMessage, ITestResultMessage
    {
        public TestResultMessage(ITestCase testCase, string testDisplayName, decimal executionTime, string output)
            : base(testCase, testDisplayName)
        {
            ExecutionTime = executionTime;
            Output = output ?? String.Empty;
        }

        /// <inheritdoc/>
        public decimal ExecutionTime { get; private set; }

        /// <inheritdoc/>
        public string Output { get; private set; }
    }
}