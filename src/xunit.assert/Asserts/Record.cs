using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Xunit
{
    public partial class Assert
    {
        /// <summary>
        /// Records any exception which is thrown by the given code.
        /// </summary>
        /// <param name="testCode">The code which may thrown an exception.</param>
        /// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception is resurfaced to the user.")]
        protected static Exception RecordException(Action testCode)
        {
            Assert.GuardArgumentNotNull("testCode", testCode);

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
        /// <param name="testCode">The code which may thrown an exception.</param>
        /// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception is resurfaced to the user.")]
        protected static Exception RecordException(Func<object> testCode)
        {
            Assert.GuardArgumentNotNull("testCode", testCode);
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
        [SuppressMessage("Code Notifications", "RECS0083:Shows NotImplementedException throws in the quick task bar", Justification = "This is a purposeful use of NotImplementedException")]
        protected static Exception RecordException(Func<Task> testCode) { throw new NotImplementedException(); }

        /// <summary>
        /// Records any exception which is thrown by the given task.
        /// </summary>
        /// <param name="testCode">The task which may thrown an exception.</param>
        /// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception is resurfaced to the user.")]
        protected static async Task<Exception> RecordExceptionAsync(Func<Task> testCode)
        {
            Assert.GuardArgumentNotNull("testCode", testCode);

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
    }
}