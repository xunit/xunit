using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception that is thrown when a call to Debug.Assert() fails.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class TraceAssertException : AssertException
    {
        readonly string assertDetailedMessage;
        readonly string assertMessage;

        /// <summary>
        /// Creates a new instance of the <see cref="TraceAssertException"/> class.
        /// </summary>
        /// <param name="assertMessage">The original assert message</param>
        public TraceAssertException(string assertMessage)
            : this(assertMessage, "") { }

        /// <summary>
        /// Creates a new instance of the <see cref="TraceAssertException"/> class.
        /// </summary>
        /// <param name="assertMessage">The original assert message</param>
        /// <param name="assertDetailedMessage">The original assert detailed message</param>
        public TraceAssertException(string assertMessage, string assertDetailedMessage)
        {
            this.assertMessage = assertMessage ?? "";
            this.assertDetailedMessage = assertDetailedMessage ?? "";
        }

        /// <inheritdoc/>
        protected TraceAssertException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            assertDetailedMessage = info.GetString("AssertDetailedMessage");
            assertMessage = info.GetString("AssertMessage");
        }

        /// <summary>
        /// Gets the original assert detailed message.
        /// </summary>
        public string AssertDetailedMessage
        {
            get { return assertDetailedMessage; }
        }

        /// <summary>
        /// Gets the original assert message.
        /// </summary>
        public string AssertMessage
        {
            get { return assertMessage; }
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                string result = "Debug.Assert() Failure";

                if (!String.IsNullOrEmpty(AssertMessage))
                {
                    result += " : " + AssertMessage;

                    if (!String.IsNullOrEmpty(AssertDetailedMessage))
                        result += Environment.NewLine + "Detailed Message:" + Environment.NewLine + AssertDetailedMessage;
                }

                return result;
            }
        }

        /// <inheritdoc/>
        [SecurityCritical]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Protected with the Guard class")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Guard.ArgumentNotNull("info", info);

            info.AddValue("AssertDetailedMessage", assertDetailedMessage);
            info.AddValue("AssertMessage", assertMessage);

            base.GetObjectData(info, context);
        }
    }
}