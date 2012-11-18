using System;
using System.Security;

namespace Xunit.Sdk
{
    /// <summary>
    /// This is an internal class, and is not intended to be called from end-user code.
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
