using System;
using System.Runtime.Remoting;
using System.Security;

namespace Xunit
{
    /// <summary>
    /// This class inherits from <see cref="MarshalByRefObject"/> and reimplements
    /// InitializeLifetimeService in a way that allows the object to live
    /// longer than the remoting default lifetime (5 minutes).
    /// </summary>
    public abstract class LongLivedMarshalByRefObject : MarshalByRefObject, IDisposable
    {
        /// <inheritdoc/>
        [SecurityCritical]
        public override sealed Object InitializeLifetimeService()
        {
            return null;
        }

        /// <inheritdoc/>
        [SecuritySafeCritical]
        public virtual void Dispose()
        {
            RemotingServices.Disconnect(this);
        }
    }
}