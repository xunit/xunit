using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Xunit.Sdk
{
    /// <summary>
    /// Utility methods for dealing with exceptions.
    /// </summary>
    public static class ExceptionUtility
    {
        const string RETHROW_MARKER = "$$RethrowMarker$$";

        /// <summary>
        /// Gets the message for the exception, including any inner exception messages.
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <returns>The formatted message</returns>
        public static string GetMessage(Exception ex)
        {
            return GetMessage(ex, 0);
        }

        static string GetMessage(Exception ex, int level)
        {
            string result = "";

            if (level > 0)
            {
                for (int idx = 0; idx < level; idx++)
                    result += "----";

                result += " ";
            }

            if (!(ex is AssertException))
                result += ex.GetType().FullName + " : ";

            result += ex.Message;

            if (ex.InnerException != null)
                result = result + Environment.NewLine + GetMessage(ex.InnerException, level + 1);

            return result;
        }

        /// <summary>
        /// Gets the stack trace for the exception, including any inner exceptions.
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <returns>The formatted stack trace</returns>
        public static string GetStackTrace(Exception ex)
        {
            if (ex == null)
                return "";

            string result = ex.StackTrace;

            if (result != null)
            {
                int idx = result.IndexOf(RETHROW_MARKER);
                if (idx >= 0)
                    result = result.Substring(0, idx);
            }

            if (ex.InnerException != null)
                result = result + Environment.NewLine +
                         "----- Inner Stack Trace -----" + Environment.NewLine +
                         GetStackTrace(ex.InnerException);

            return result;
        }

        /// <summary>
        /// Rethrows an exception object without losing the existing stack trace information
        /// </summary>
        /// <param name="ex">The exception to re-throw.</param>
        /// <remarks>
        /// For more information on this technique, see
        /// http://www.dotnetjunkies.com/WebLog/chris.taylor/archive/2004/03/03/8353.aspx
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        public static void RethrowWithNoStackTraceLoss(Exception ex)
        {
            FieldInfo remoteStackTraceString =
                typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic) ??
                typeof(Exception).GetField("remote_stack_trace", BindingFlags.Instance | BindingFlags.NonPublic);

            remoteStackTraceString.SetValue(ex, ex.StackTrace + RETHROW_MARKER);
            throw ex;
        }
    }
}
