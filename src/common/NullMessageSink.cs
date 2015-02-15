using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// An implementation of <see cref="IMessageSink"/> that ignores all messages.
    /// </summary>
    public class NullMessageSink : LongLivedMarshalByRefObject, IMessageSink
    {
        /// <inheritdoc/>
        public bool OnMessage(IMessageSinkMessage message)
        {
            return true;
        }
    }
}
