using System;
using System.Linq;

namespace Xunit.Abstractions
{
    public interface ITestCaseMessage : ITestCollectionMessage
    {
        /// <summary>
        /// The test case that is associated with this message.
        /// </summary>
        ITestCase TestCase { get; }
    }
}