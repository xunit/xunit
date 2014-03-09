using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Xunit
{
    /// <summary>
    /// Allows the user to record actions for a test.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "This is not marked as static because we want people to be able to derive from it")]
    public partial class Record
    {
        /// <summary>
        /// Records any exception which is thrown by the given code.
        /// </summary>
        /// <param name="testCode">The code which may thrown an exception.</param>
        /// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception is resurfaced to the user.")]
        public static Exception Exception(Action testCode)
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
        public static Exception Exception(Func<object> testCode)
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
        public static Exception Exception(Func<Task> testCode) { throw new NotImplementedException(); }

        /// <summary>
        /// Records any exception which is thrown by the given task.
        /// </summary>
        /// <param name="testCode">The task which may thrown an exception.</param>
        /// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception is resurfaced to the user.")]
        public static async Task<Exception> ExceptionAsync(Func<Task> testCode)
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