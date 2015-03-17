using System.Collections.Generic;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Base message interface for all messages related to test execution. It includes the list
    /// of test cases that are associated with this execution step.
    /// </summary>
    public interface IExecutionMessage : IMessageSinkMessage
    {
        /// <summary>
        /// The test cases that are associated with this message.
        /// </summary>
        IEnumerable<ITestCase> TestCases { get; }
    }
}