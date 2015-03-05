using System;

namespace Xunit
{
    /// <summary>
    /// Base class for all long-lived objects that may cross over an AppDomain.
    /// </summary>
    public abstract class LongLivedMarshalByRefObject 
#if !WINDOWS_PHONE_APP && !WINDOWS_PHONE && !ASPNETCORE50
        : MarshalByRefObject 
#endif
    { }
}
