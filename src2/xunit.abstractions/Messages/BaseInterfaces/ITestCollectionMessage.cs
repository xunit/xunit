using System;
using System.Linq;

namespace Xunit.Abstractions
{
    public interface ITestCollectionMessage : IMessageSinkMessage
    {
        /// <summary>
        /// Gets the test collection this test message is associated with.
        /// </summary>
        ITestCollection TestCollection { get; }
    }
}