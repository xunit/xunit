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
    public class DiscoveryCompleteMessage : LongLivedMarshalByRefObject, IDiscoveryCompleteMessage { }
}