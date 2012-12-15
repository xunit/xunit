using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security;

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
            get { return FilterStackTrace(stackTrace ?? base.StackTrace); }
        }

        /// <summary>
        /// Gets the user message
        /// </summary>
        public string UserMessage { get; protected set; }

        /// <summary>
        /// Determines whether to exclude a line from the stack frame. By default, this method
        /// removes all stack frames from methods beginning with Xunit.Assert or Xunit.Sdk.
        /// </summary>
        /// <param name="stackFrame">The stack frame to be filtered.</param>
        /// <returns>Return true to exclude the line from the stack frame; false, otherwise.</returns>
        protected virtual bool ExcludeStackFrame(string stackFrame)
        {
            Guard.ArgumentNotNull("stackFrame", stackFrame);

            return stackFrame.StartsWith("at Xunit.Assert.", StringComparison.Ordinal)
                || stackFrame.StartsWith("at Xunit.Sdk.", StringComparison.Ordinal);
        }

        /// <summary>
        /// Filters the stack trace to remove all lines that occur within the testing framework.
        /// </summary>
        /// <param name="stack">The original stack trace</param>
        /// <returns>The filtered stack trace</returns>
        protected string FilterStackTrace(string stack)
        {
            if (stack == null)
                return null;

            List<string> results = new List<string>();

            foreach (string line in SplitLines(stack))
            {
                string trimmedLine = line.TrimStart();
                if (!ExcludeStackFrame(trimmedLine))
                    results.Add(line);
            }

            return string.Join(Environment.NewLine, results.ToArray());
        }

        /// <inheritdoc/>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Guard.ArgumentNotNull("info", info);

            info.AddValue("CustomStackTrace", stackTrace);
            info.AddValue("UserMessage", UserMessage);

            base.GetObjectData(info, context);
        }

        // Our own custom String.Split because Silverlight/CoreCLR doesn't support the version we were using
        static IEnumerable<string> SplitLines(string input)
        {
            while (true)
            {
                int idx = input.IndexOf(Environment.NewLine);

                if (idx < 0)
                {
                    yield return input;
                    break;
                }

                yield return input.Substring(0, idx);
                input = input.Substring(idx + Environment.NewLine.Length);
            }
        }

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