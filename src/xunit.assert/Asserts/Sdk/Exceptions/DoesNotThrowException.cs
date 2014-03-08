using System;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when Assert.DoesNotThrow fails.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class DoesNotThrowException : AssertActualExpectedException
    {
        readonly string stackTrace;

        /// <summary>
        /// Creates a new instance of the <see cref="DoesNotThrowException"/> class.
        /// </summary>
        /// <param name="actual">Actual exception</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        public DoesNotThrowException(Exception actual)
            : base("(No exception)",
                   actual.GetType().FullName + (actual.Message == null ? "" : ": " + actual.Message),
                   "Assert.DoesNotThrow() Failure")
        {
            stackTrace = actual.StackTrace;
        }

        /// <summary>
        /// THIS CONSTRUCTOR IS FOR UNIT TESTING PURPOSES ONLY.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "This constructor is not meant to be public.")]
        protected DoesNotThrowException(string stackTrace)
            : base("Expected", "Actual", "UserMessage")
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
    }
}