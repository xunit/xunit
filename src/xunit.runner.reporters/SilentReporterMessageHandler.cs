using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Runner.Reporters
{
    public sealed class SilentReporterMessageHandler : LongLivedMarshalByRefObject, IMessageSinkWithTypes, IMessageSink
    {
        public void Dispose()
        { }

        public bool OnMessage(IMessageSinkMessage message) =>
            true;

        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes) =>
            true;
    }
}
