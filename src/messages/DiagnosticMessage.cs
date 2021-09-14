using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="IDiagnosticMessage"/>.
    /// </summary>
    public class DiagnosticMessage : LongLivedMarshalByRefObject, IDiagnosticMessage
#if !XUNIT_FRAMEWORK
        , IMessageSinkMessageWithTypes
#endif
    {
        static readonly HashSet<string> interfaceTypes = new HashSet<string>(typeof(DiagnosticMessage).GetInterfaces().Select(x => x.FullName));
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticMessage"/> class.
        /// </summary>
        public DiagnosticMessage() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticMessage"/> class.
        /// </summary>
        /// <param name="message">The message to send</param>
        public DiagnosticMessage(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticMessage"/> class.
        /// </summary>
        /// <param name="format">The format of the message to send</param>
        /// <param name="args">The arguments used to format the message</param>
        public DiagnosticMessage(string format, params object[] args)
        {
            Message = string.Format(format, args);
        }

        /// <inheritdoc/>
        public HashSet<string> InterfaceTypes => interfaceTypes;

        /// <inheritdoc/>
        public string Message { get; set; }
    }
}
