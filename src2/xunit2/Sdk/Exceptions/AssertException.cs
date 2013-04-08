using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The base assert exception class
    /// </summary>
    [Serializable]
    public class AssertException : Exception
    {
        readonly string stackTrace;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertException"/> class.
        /// </summary>
        public AssertException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be displayed</param>
        public AssertException(string userMessage)
            : base(userMessage)
        {
            this.UserMessage = userMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be displayed</param>
        /// <param name="innerException">The inner exception</param>
        [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
        protected AssertException(string userMessage, Exception innerException)
            : base(userMessage, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be displayed</param>
        /// <param name="stackTrace">The stack trace to be displayed</param>
        protected AssertException(string userMessage, string stackTrace)
            : base(userMessage)
        {
            this.stackTrace = stackTrace;
        }

        /// <inheritdoc/>
        protected AssertException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            stackTrace = info.GetString("CustomStackTrace");
            UserMessage = info.GetString("UserMessage");
        }

        /// <summary>
        /// Gets a string representation of the frames on the call stack at the time the current exception was thrown.
        /// </summary>
        /// <returns>A string that describes the contents of the call stack, with the most recent method call appearing first.</returns>
        public override string StackTrace
        {
            get { return stackTrace ?? base.StackTrace; }
        }

        /// <summary>
        /// Gets the user message
        /// </summary>
        public string UserMessage { get; protected set; }

        /// <inheritdoc/>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Guard.ArgumentNotNull("info", info);

            info.AddValue("CustomStackTrace", stackTrace);
            info.AddValue("UserMessage", UserMessage);

            base.GetObjectData(info, context);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string className = GetType().ToString();
            string message = this.Message;
            string result;

            if (message == null || message.Length <= 0)
                result = className;
            else
                result = className + ": " + message;

            string stackTrace = StackTrace;
            if (stackTrace != null)
                result = result + Environment.NewLine + stackTrace;

            return result;
        }
    }
}