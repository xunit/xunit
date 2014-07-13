using System;

namespace Xunit.Sdk
{
    /// <summary>
    /// This class inherits from <see cref="T:System.MarshalByRefObject"/> and re-implements
    /// InitializeLifetimeService in a way that allows the object to live
    /// longer than the remoting default lifetime (5 minutes).
    /// </summary>
    public abstract class LongLivedMarshalByRefObject
    {
        /// <summary>
        /// Creates a new instance of the <see cref="LongLivedMarshalByRefObject"/> type.
        /// </summary>
        protected LongLivedMarshalByRefObject() { }

        /// <summary>
        /// Disconnects all remote objects.
        /// </summary>
        public static void DisconnectAll() { }
    }
}