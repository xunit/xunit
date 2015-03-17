using System.Collections.Generic;

namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that the discovery process has been completed for
    /// the requested assembly.
    /// </summary>
    public interface IDiscoveryCompleteMessage : IMessageSinkMessage { }
}