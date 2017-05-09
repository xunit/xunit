using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Xunit
{
    /// <summary>
    /// Allows the user to record actions for a test.
    /// </summary>
    [SuppressMessage("Language Usage Opportunities", "RECS0014:If all fields, properties and methods members are static, the class can be made static.", Justification = "This is a potential extensibility point")]
    public class Record
    {
        /// <summary>
        /// Records any exception which is thrown by the given code.
        /// </summary>
        /// <param name="testCode">The code which may throw an exception.</param>
        /// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
        public static Exception Exception(Action testCode)
        {
            GuardArgumentNotNull("testCode", testCode);

            try
            {
                testCode();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        /// <summary>
        /// Records any exception which is thrown by the given code that has
        /// a return value. Generally used for testing property accessors.
        /// </summary>
        /// <param name="testCode">The code which may throw an exception.</param>
        /// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
        public static Exception Exception(Func<object> testCode)
        {
            GuardArgumentNotNull("testCode", testCode);
            Task task;

            try
            {
                task = testCode() as Task;
            }
            catch (Exception ex)
            {
                return ex;
            }

            if (task != null)
                throw new InvalidOperationException("You must call Assert.ThrowsAsync, Assert.DoesNotThrowAsync, or Record.ExceptionAsync when testing async code.");

            return null;
        }

        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("You must call Record.ExceptionAsync (and await the result) when testing async code.", true)]
        public static Exception Exception(Func<Task> testCode) { throw new NotImplementedException(); }

        /// <summary>
        /// Records any exception which is thrown by the given task.
        /// </summary>
        /// <param name="testCode">The task which may throw an exception.</param>
        /// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
        public static async Task<Exception> ExceptionAsync(Func<Task> testCode)
        {
            GuardArgumentNotNull("testCode", testCode);

            try
            {
                await testCode();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        /// <summary/>
        internal static void GuardArgumentNotNull(string argName, object argValue)
        {
            if (argValue == null)
                throw new ArgumentNullException(argName);
        }
    }
}
