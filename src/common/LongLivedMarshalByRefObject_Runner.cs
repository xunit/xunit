using System;
using System.Security;

namespace Xunit
{
    /// <summary>
    /// Base class for all long-lived objects that may cross over an AppDomain.
    /// </summary>
    public abstract class LongLivedMarshalByRefObject : MarshalByRefObject
    {
        /// <inheritdoc/>
        [SecurityCritical]
        public override sealed Object InitializeLifetimeService()
        {
            return null;
        }
    }
}