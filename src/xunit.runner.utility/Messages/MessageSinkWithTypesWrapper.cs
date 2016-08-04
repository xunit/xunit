using System;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    class MessageSinkWithTypesWrapper : LongLivedMarshalByRefObject, IMessageSink
    {
#if !NET35
        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, string[]> messageInterfaces = new System.Collections.Concurrent.ConcurrentDictionary<Type, string[]>();
#endif
        readonly IMessageSinkWithTypes sink;

        public MessageSinkWithTypesWrapper(IMessageSinkWithTypes sink)
        {
            this.sink = sink;
        }

        static string[] GetMessageTypes(IMessageSinkMessage message)
        {
#if NET35
            return GetInterfaceNames(message.GetType());
#else
            return messageInterfaces.GetOrAdd(message.GetType(), t => GetInterfaceNames(t));
#endif
        }

        static string[] GetInterfaceNames(Type type)
        {
            return type.GetInterfaces().Select(t => t.FullName).ToArray();
        }

        public bool OnMessage(IMessageSinkMessage message)
        {
            var interfaces = GetMessageTypes(message);
            return sink.OnMessageWithTypes(message, interfaces);
        }
    }
}
