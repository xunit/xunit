using System;
using System.Diagnostics.CodeAnalysis;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Allows the user to record actions for a test.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "This is not marked as static because we want people to be able to derive from it")]
    public class Record
    {
        /// <summary>
        /// Records any exception which is thrown by the given code.
        /// </summary>
        /// <param name="code">The code which may thrown an exception.</param>
        /// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception is resurfaced to the user.")]
        public static Exception Exception(Action code)
        {
            Guard.ArgumentNotNull("code", code);

            try
            {
                code();
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
        /// <param name="code">The code which may thrown an exception.</param>
        /// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception is resurfaced to the user.")]
        public static Exception Exception(Func<object> code)
        {
            Guard.ArgumentNotNull("code", code);

            try
            {
                code();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}