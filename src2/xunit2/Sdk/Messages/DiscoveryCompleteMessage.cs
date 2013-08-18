using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IDiscoveryCompleteMessage"/>.
    /// </summary>
    public class DiscoveryCompleteMessage : LongLivedMarshalByRefObject, IDiscoveryCompleteMessage
    {
        /// <inheritdoc/>
        public IEnumerable<string> Warnings { get; set; }
    }
}