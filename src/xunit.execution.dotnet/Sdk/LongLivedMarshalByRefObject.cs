namespace Xunit.Sdk
{
    /// <summary>
    /// Base class for all long-lived objects that may cross over an AppDomain.
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
