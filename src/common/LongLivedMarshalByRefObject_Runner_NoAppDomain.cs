using System;

namespace Xunit
{
    /// <summary>
    /// This class inherits from <see cref="T:System.MarshalByRefObject"/> and reimplements
    /// InitializeLifetimeService in a way that allows the object to live
    /// longer than the remoting default lifetime (5 minutes).
    /// </summary>
    public abstract class LongLivedMarshalByRefObject : IDisposable
    {
        /// <inheritdoc/>
        public virtual void Dispose() { }
    }
}