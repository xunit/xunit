using System;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    class OptimizedRemoteMessageSink : LongLivedMarshalByRefObject, IMessageSink
    {
#if !NET35
        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, string[]> messageInterfaces = new System.Collections.Concurrent.ConcurrentDictionary<Type, string[]>();
#endif
        readonly IMessageSinkWithTypes sinkWithTypes;

        public OptimizedRemoteMessageSink(IMessageSinkWithTypes sinkWithTypes)
        {
            this.sinkWithTypes = sinkWithTypes;
        }

        static string[] GetMessageTypes(IMessageSinkMessage message)
#if NET35
            => GetInterfaceNames(message.GetType());
#else
            => messageInterfaces.GetOrAdd(message.GetType(), t => GetInterfaceNames(t));
#endif

        static string[] GetInterfaceNames(Type type)
            => type.GetInterfaces().Select(t => t.FullName).ToArray();

        public bool OnMessage(IMessageSinkMessage message)
            => sinkWithTypes.OnMessageWithTypes(message, GetMessageTypes(message));
    }
}
