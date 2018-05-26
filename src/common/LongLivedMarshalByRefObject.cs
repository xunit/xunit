#if XUNIT_FRAMEWORK
namespace Xunit
#else
namespace Xunit.Sdk
#endif
{
#if NETFRAMEWORK
    using System;
    using System.Security;

#if XUNIT_FRAMEWORK
    using System.Collections.Concurrent;
    using System.Runtime.Remoting;
#endif

    /// <summary>
    /// Base class for all long-lived objects that may cross over an AppDomain.
    /// </summary>
    public abstract class LongLivedMarshalByRefObject : MarshalByRefObject
    {
#if XUNIT_FRAMEWORK
        static ConcurrentBag<MarshalByRefObject> remoteObjects = new ConcurrentBag<MarshalByRefObject>();

        /// <summary>
        /// Creates a new instance of the <see cref="LongLivedMarshalByRefObject"/> type.
        /// </summary>
        protected LongLivedMarshalByRefObject()
        {
            remoteObjects.Add(this);
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
#else
        /// <summary>
        /// Disconnects all remote objects.
        /// </summary>
        public static void DisconnectAll() { }
#endif

        /// <inheritdoc/>
        [SecurityCritical]
        public override sealed object InitializeLifetimeService()
        {
            return null;
        }
    }
#else
    /// <summary>
    /// Base class for all long-lived objects that may cross over an AppDomain.
    /// </summary>
    public abstract class LongLivedMarshalByRefObject
    {
        /// <summary>
        /// Disconnects all remote objects.
        /// </summary>
        public static void DisconnectAll() { }
    }
#endif
}
