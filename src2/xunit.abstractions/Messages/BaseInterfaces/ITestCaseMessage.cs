using System;
using System.Linq;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Base message interface for all messages related to test cases.
    /// </summary>
    public interface ITestCaseMessage : ITestCollectionMessage
    {
        /// <summary>
        /// The test case that is associated with this message.
        /// </summary>
        ITestCase TestCase { get; }
    }
}