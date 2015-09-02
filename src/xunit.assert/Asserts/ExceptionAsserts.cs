using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit
{
    public partial class Assert
    {
        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// </summary>
        /// <typeparam name="T">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static T Throws<T>(Action testCode)
            where T : Exception
        {
            return (T)Throws(typeof(T), RecordException(testCode));
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// Generally used to test property accessors.
        /// </summary>
        /// <typeparam name="T">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static T Throws<T>(Func<object> testCode)
            where T : Exception
        {
            return (T)Throws(typeof(T), RecordException(testCode));
        }

        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.", true)]
        [SuppressMessage("Code Notifications", "RECS0083:Shows NotImplementedException throws in the quick task bar", Justification = "This is a purposeful use of NotImplementedException")]
        public static T Throws<T>(Func<Task> testCode) where T : Exception { throw new NotImplementedException(); }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// </summary>
        /// <typeparam name="T">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the task to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static async Task<T> ThrowsAsync<T>(Func<Task> testCode)
            where T : Exception
        {
            return (T)Throws(typeof(T), await RecordExceptionAsync(testCode));
        }

        /// <summary>
        /// Verifies that the exact exception or a derived exception type is thrown.
        /// </summary>
        /// <typeparam name="T">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static T ThrowsAny<T>(Action testCode)
            where T : Exception
        {
            return (T)ThrowsAny(typeof(T), RecordException(testCode));
        }

        /// <summary>
        /// Verifies that the exact exception or a derived exception type is thrown.
        /// Generally used to test property accessors.
        /// </summary>
        /// <typeparam name="T">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static T ThrowsAny<T>(Func<object> testCode)
            where T : Exception
        {
            return (T)ThrowsAny(typeof(T), RecordException(testCode));
        }

        /// <summary>
        /// Verifies that the exact exception or a derived exception type is thrown.
        /// </summary>
        /// <typeparam name="T">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the task to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static async Task<T> ThrowsAnyAsync<T>(Func<Task> testCode)
            where T : Exception
        {
            return (T)ThrowsAny(typeof(T), await RecordExceptionAsync(testCode));
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// </summary>
        /// <param name="exceptionType">The type of the exception expected to be thrown</param>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static Exception Throws(Type exceptionType, Action testCode)
        {
            return Throws(exceptionType, RecordException(testCode));
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// Generally used to test property accessors.
        /// </summary>
        /// <param name="exceptionType">The type of the exception expected to be thrown</param>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static Exception Throws(Type exceptionType, Func<object> testCode)
        {
            return Throws(exceptionType, RecordException(testCode));
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// </summary>
        /// <param name="exceptionType">The type of the exception expected to be thrown</param>
        /// <param name="testCode">A delegate to the task to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static async Task<Exception> ThrowsAsync(Type exceptionType, Func<Task> testCode)
        {
            return Throws(exceptionType, await RecordExceptionAsync(testCode));
        }

        static Exception Throws(Type exceptionType, Exception exception)
        {
            Assert.GuardArgumentNotNull("exceptionType", exceptionType);

            if (exception == null)
                throw new ThrowsException(exceptionType);

            if (!exceptionType.Equals(exception.GetType()))
                throw new ThrowsException(exceptionType, exception);

            return exception;
        }

        static Exception ThrowsAny(Type exceptionType, Exception exception)
        {
            Assert.GuardArgumentNotNull("exceptionType", exceptionType);

            if (exception == null)
                throw new ThrowsException(exceptionType);

            if (!exceptionType.GetTypeInfo().IsAssignableFrom(exception.GetType().GetTypeInfo()))
                throw new ThrowsException(exceptionType, exception);

            return exception;
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type), where the exception
        /// derives from <see cref="ArgumentException"/> and has the given parameter name.
        /// </summary>
        /// <param name="paramName">The parameter name that is expected to be in the exception</param>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static T Throws<T>(string paramName, Action testCode)
            where T : ArgumentException
        {
            var ex = Assert.Throws<T>(testCode);
            Assert.Equal(paramName, ex.ParamName);
            return ex;
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type), where the exception
        /// derives from <see cref="ArgumentException"/> and has the given parameter name.
        /// </summary>
        /// <param name="paramName">The parameter name that is expected to be in the exception</param>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static T Throws<T>(string paramName, Func<object> testCode)
            where T : ArgumentException
        {
            var ex = Assert.Throws<T>(testCode);
            Assert.Equal(paramName, ex.ParamName);
            return ex;
        }

        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.", true)]
        [SuppressMessage("Code Notifications", "RECS0083:Shows NotImplementedException throws in the quick task bar", Justification = "This is a purposeful use of NotImplementedException")]
        public static T Throws<T>(string paramName, Func<Task> testCode) where T : ArgumentException { throw new NotImplementedException(); }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type), where the exception
        /// derives from <see cref="ArgumentException"/> and has the given parameter name.
        /// </summary>
        /// <param name="paramName">The parameter name that is expected to be in the exception</param>
        /// <param name="testCode">A delegate to the task to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static async Task<T> ThrowsAsync<T>(string paramName, Func<Task> testCode)
            where T : ArgumentException
        {
            var ex = await Assert.ThrowsAsync<T>(testCode);
            Assert.Equal(paramName, ex.ParamName);
            return ex;
        }
    }
}
