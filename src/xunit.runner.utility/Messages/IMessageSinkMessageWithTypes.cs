using System.Collections.Generic;

namespace Xunit
{
    /// <summary>
    /// Identifies a message that can return its own type information.
    /// </summary>
    public interface IMessageSinkMessageWithTypes
    {
        /// <summary>
        /// Gets the interface type full names of the implemented interfaces.
        /// </summary>
        HashSet<string> InterfaceTypes { get; }
    }
}
