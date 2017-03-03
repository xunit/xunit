namespace Xunit
{
    /// <summary>
    /// Base class for all long-lived objects that may cross over an AppDomain.
    /// </summary>
#if NET35 || NET452
    public abstract class LongLivedMarshalByRefObject : System.MarshalByRefObject
    {
        /// <inheritdoc/>
        [System.Security.SecurityCritical]
        public override sealed object InitializeLifetimeService()
        {
            return null;
        }
    }
#else
    public abstract class LongLivedMarshalByRefObject { }
#endif
}
