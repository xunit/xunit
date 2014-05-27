using System;
using System.Collections.Concurrent;
using System.Runtime.Remoting;
using System.Security;

namespace Xunit.Sdk
{
    /// <summary>
    /// This class inherits from <see cref="MarshalByRefObject"/> and re-implements
    /// InitializeLifetimeService in a way that allows the object to live
    /// longer than the remoting default lifetime (5 minutes).
    /// </summary>
    public abstract class LongLivedMarshalByRefObject :  MarshalByRefObject
    {
#if !NO_APPDOMAIN
        static ConcurrentBag<MarshalByRefObject> remoteObjects = new ConcurrentBag<MarshalByRefObject>();
#endif
        /// <summary>
        /// Creates a new instance of the <see cref="LongLivedMarshalByRefObject"/> type.
        /// </summary>
        protected LongLivedMarshalByRefObject()
        {
#if !NO_APPDOMAIN
            remoteObjects.Add(this);
#endif
        }

#if !NO_APPDOMAIN
        /// <inheritdoc/>
        [SecurityCritical]
        public override sealed Object InitializeLifetimeService()
        {
            return null;
        }
#endif

        /// <summary>
        /// Disconnects all remote objects.
        /// </summary>
        [SecuritySafeCritical]
        public static void DisconnectAll()
        {
#if !NO_APPDOMAIN
            foreach (var remoteObject in remoteObjects)
                RemotingServices.Disconnect(remoteObject);

            remoteObjects = new ConcurrentBag<MarshalByRefObject>();
#endif
        }
    }
}