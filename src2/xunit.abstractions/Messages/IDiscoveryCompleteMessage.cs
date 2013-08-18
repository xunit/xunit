using System.Collections.Generic;

namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that the discovery process has been completed for
    /// the requested assembly.
    /// </summary>
    public interface IDiscoveryCompleteMessage : ITestMessage
    {
        /// <summary>
        /// Gets the list of warnings that were raised during discovery.
        /// </summary>
        IEnumerable<string> Warnings { get; }
    }
}