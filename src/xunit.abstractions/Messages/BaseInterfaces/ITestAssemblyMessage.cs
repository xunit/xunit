using System.Collections.Generic;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Base message interface for all messages related to test assemblies.
    /// </summary>
    public interface ITestAssemblyMessage : IMessageSinkMessage
    {
        /// <summary>
        /// The test assembly that is associated with this message.
        /// </summary>
        ITestAssembly TestAssembly { get; }
    }
}