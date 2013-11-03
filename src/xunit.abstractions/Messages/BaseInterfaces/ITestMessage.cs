using System;
using System.Linq;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Base message interface for all messages related to tests.
    /// </summary>
    public interface ITestMessage : ITestCaseMessage
    {
        /// <summary>
        /// The display name of the test.
        /// </summary>
        string TestDisplayName { get; }
    }
}
