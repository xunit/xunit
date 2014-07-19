using System;
using System.Collections.Concurrent;
using System.Runtime.Remoting;
using System.Security;

namespace Xunit.Sdk
{
    /// <summary>
    /// Base class for all long-lived objects that may cross over an AppDomain.
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