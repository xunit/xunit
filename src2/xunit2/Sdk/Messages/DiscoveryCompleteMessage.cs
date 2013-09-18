using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IDiscoveryCompleteMessage"/>.
    /// </summary>
    internal class DiscoveryCompleteMessage : LongLivedMarshalByRefObject, IDiscoveryCompleteMessage
    {
        public DiscoveryCompleteMessage(IEnumerable<string> warnings)
        {
            Warnings = warnings;
        }

        /// <inheritdoc/>
        public IEnumerable<string> Warnings { get; private set; }
    }
}