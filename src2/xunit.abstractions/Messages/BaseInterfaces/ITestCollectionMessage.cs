using System;
using System.Linq;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Base message interface for all messages related to test collections.
    /// </summary>
    public interface ITestCollectionMessage : IMessageSinkMessage
    {
        /// <summary>
        /// Gets the test collection this test message is associated with.
        /// </summary>
        ITestCollection TestCollection { get; }
    }
}