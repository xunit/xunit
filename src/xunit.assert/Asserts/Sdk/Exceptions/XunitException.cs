using System;

namespace Xunit.Sdk
{
    /// <summary>
    /// The base assert exception class
    /// </summary>
    public class XunitException : Exception
    {
        readonly string stackTrace;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitException"/> class.
        /// </summary>
        public XunitException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be displayed</param>
        public XunitException(string userMessage)
            : base(userMessage)
        {
            UserMessage = userMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be displayed</param>
        /// <param name="innerException">The inner exception</param>
        protected XunitException(string userMessage, Exception innerException)
            : base(userMessage, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be displayed</param>
        /// <param name="stackTrace">The stack trace to be displayed</param>
        protected XunitException(string userMessage, string stackTrace)
            : base(userMessage)
        {
            this.stackTrace = stackTrace;
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
        public override string ToString()
        {
            string className = GetType().ToString();
            string message = Message;
            string result;

            if (message == null || message.Length <= 0)
                result = className;
            else
                result = string.Format("{0}: {1}", className, message);

            string stackTrace = StackTrace;
            if (stackTrace != null)
                result = result + Environment.NewLine + stackTrace;

            return result;
        }
    }
}