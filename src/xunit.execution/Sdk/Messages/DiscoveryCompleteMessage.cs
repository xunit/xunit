using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="IDiscoveryCompleteMessage"/>.
    /// </summary>
    public class DiscoveryCompleteMessage : LongLivedMarshalByRefObject, IDiscoveryCompleteMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryCompleteMessage"/> class.
        /// </summary>
        public DiscoveryCompleteMessage(IEnumerable<string> warnings)
        {
            Warnings = warnings;
        }

        /// <inheritdoc/>
        public IEnumerable<string> Warnings { get; private set; }
    }
}