﻿using System;
#if !WINDOWS_PHONE_APP
using System.Runtime.Remoting;
#endif
using System.Security;

namespace Xunit
{
    /// <summary>
    /// This class inherits from <see cref="MarshalByRefObject"/> and reimplements
    /// InitializeLifetimeService in a way that allows the object to live
    /// longer than the remoting default lifetime (5 minutes).
    /// </summary>
    public abstract class LongLivedMarshalByRefObject :
#if !WINDOWS_PHONE_APP
        MarshalByRefObject,
#endif
 IDisposable
    {
#if !NO_APPDOMAIN
        /// <inheritdoc/>
        [SecurityCritical]
        public override sealed Object InitializeLifetimeService()
        {
            return null;
        }
#endif

        /// <inheritdoc/>
        [SecuritySafeCritical]
        public virtual void Dispose()
        {
#if !NO_APPDOMAIN
            RemotingServices.Disconnect(this);
#endif
        }
    }
}