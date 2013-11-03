using System;
using Xunit.Sdk;

namespace Xunit
{
    public partial class Assert
    {
        /// <summary>
        /// Verifies that a block of code does not throw any exceptions.
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        public static void DoesNotThrow(Action testCode)
        {
            DoesNotThrow(Record.Exception(testCode));
        }

        /// <summary>
        /// Verifies that a block of code does not throw any exceptions.
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        public static void DoesNotThrow(Func<object> testCode)
        {
            DoesNotThrow(Record.Exception(testCode));
        }

        private static void DoesNotThrow(Exception exception)
        {
            if (exception != null)
                throw new DoesNotThrowException(exception);
        }

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
            return (T)Throws(typeof(T), Record.Exception(testCode));
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
            return (T)Throws(typeof(T), Record.Exception(testCode));
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
            return Throws(exceptionType, Record.Exception(testCode));
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
            return Throws(exceptionType, Record.Exception(testCode));
        }

        private static Exception Throws(Type exceptionType, Exception exception)
        {
            Guard.ArgumentNotNull("exceptionType", exceptionType);

            if (exception == null)
                throw new ThrowsException(exceptionType);

            if (!exceptionType.Equals(exception.GetType()))
                throw new ThrowsException(exceptionType, exception);

            return exception;
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type), where the exception
        /// derives from <see cref="ArgumentException"/> and has the given parameter name.
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The parameter name that is expected to be in the exception</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static T Throws<T>(Action testCode, string paramName)
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
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The parameter name that is expected to be in the exception</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static T Throws<T>(Func<object> testCode, string paramName)
            where T : ArgumentException
        {
            var ex = Assert.Throws<T>(testCode);
            Assert.Equal(paramName, ex.ParamName);
            return ex;
        }
    }
}