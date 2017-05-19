using System;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="IDiscoveryCompleteMessage"/>.
    /// </summary>
    [Serializable]
    public class DiscoveryCompleteMessage : LongLivedMarshalByRefObject, IDiscoveryCompleteMessage { }
}