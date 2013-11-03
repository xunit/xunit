using System;
using System.Security;

namespace Xunit
{
    /// <summary>
    /// This class inherits from <see cref="MarshalByRefObject"/> and reimplements
    /// <see cref="InitializeLifetimeService()"/> in a way that allows the object to live
    /// longer than the remoting default lifetime (5 minutes).
    /// </summary>
    public abstract class LongLivedMarshalByRefObject : MarshalByRefObject
    {
        /// <summary/>
        [SecurityCritical]
        public override sealed Object InitializeLifetimeService()
        {
            return null;
        }
    }
}