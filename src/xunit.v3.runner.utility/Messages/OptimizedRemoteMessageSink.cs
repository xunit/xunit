using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    class OptimizedRemoteMessageSink : LongLivedMarshalByRefObject, IMessageSink
    {
        readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        readonly Dictionary<Type, HashSet<string>> interfaceCache = new Dictionary<Type, HashSet<string>>();
        readonly IMessageSinkWithTypes runnerSink;

        public OptimizedRemoteMessageSink(IMessageSinkWithTypes runnerSink)
        {
            this.runnerSink = runnerSink;
        }

        HashSet<string> GetMessageTypes(IMessageSinkMessage message)
        {
            var messageType = message.GetType();
            HashSet<string> result;

            cacheLock.TryEnterReadLock(-1);

            try
            {
                interfaceCache.TryGetValue(messageType, out result);
            }
            finally
            {
                cacheLock.ExitReadLock();
            }

            if (result == null)
            {
                cacheLock.TryEnterWriteLock(-1);

                try
                {
                    result = new HashSet<string>(messageType.GetInterfaces().Select(x => x.FullName));
                    interfaceCache[messageType] = result;
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
            }

            return result;
        }

        public bool OnMessage(IMessageSinkMessage message)
            => runnerSink.OnMessageWithTypes(message, GetMessageTypes(message));
    }
}
