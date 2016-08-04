using Xunit.Abstractions;

namespace Xunit
{
    class MessageSinkWithTypesAdapter : LongLivedMarshalByRefObject, IMessageSinkWithTypes
    {
        readonly IMessageSink inner;

        public MessageSinkWithTypesAdapter(IMessageSink inner)
        {
            this.inner = inner;
        }

        public bool OnMessageWithTypes(IMessageSinkMessage message, string[] messageTypes)
        {
            return inner.OnMessage(message);
        }

        bool IMessageSink.OnMessage(IMessageSinkMessage message)
        {
            return OnMessageWithTypes(message, null);
        }
    }
}
