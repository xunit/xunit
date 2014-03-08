using System;
using System.Collections.Concurrent;
using System.Runtime.Remoting;
using System.Security;

namespace Xunit.Sdk
{
    /// <summary>
    /// This class inherits from <see cref="MarshalByRefObject"/> and re-implements
    /// <see cref="InitializeLifetimeService()"/> in a way that allows the object to live
    /// longer than the remoting default lifetime (5 minutes).
    /// </summary>
    public abstract class LongLivedMarshalByRefObject : MarshalByRefObject
    {
        static ConcurrentBag<MarshalByRefObject> remoteObjects = new ConcurrentBag<MarshalByRefObject>();

        /// <summary>
        /// Creates a new instance of the <see cref="LongLivedMarshalByRefObject"/> type.
        /// </summary>
        protected LongLivedMarshalByRefObject()
        {
            remoteObjects.Add(this);
        }

        /// <inheritdoc/>
        [SecurityCritical]
        public override sealed Object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Disconnects all remote objects.
        /// </summary>
        [SecuritySafeCritical]
        public static void DisconnectAll()
        {
            foreach (var remoteObject in remoteObjects)
                RemotingServices.Disconnect(remoteObject);

            remoteObjects = new ConcurrentBag<MarshalByRefObject>();
        }
    }
}