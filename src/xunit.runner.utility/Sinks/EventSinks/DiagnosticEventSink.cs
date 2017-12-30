using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Class that maps diagnostic messages to events.
    /// </summary>
    public class DiagnosticEventSink : LongLivedMarshalByRefObject, IMessageSinkWithTypes
    {
        /// <summary>
        /// Occurs when a <see cref="IDiagnosticMessage"/> message is received.
        /// </summary>
        public event MessageHandler<IDiagnosticMessage> DiagnosticMessageEvent;

        /// <summary>
        /// Occurs when a <see cref="IErrorMessage"/> message is received.
        /// </summary>
        public event MessageHandler<IErrorMessage> ErrorMessageEvent;

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> typeNames)
            => message.Dispatch(typeNames, DiagnosticMessageEvent)
            && message.Dispatch(typeNames, ErrorMessageEvent);
    }
}
