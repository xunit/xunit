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

        /// <summary>
        /// The test cases that are associated with this message.
        /// </summary>
        IEnumerable<ITestCase> TestCases { get; }
    }
}