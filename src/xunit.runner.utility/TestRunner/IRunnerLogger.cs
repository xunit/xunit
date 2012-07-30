using System;

namespace Xunit
{
    /// <summary>
    /// Represents a logger used by <see cref="TestRunner"/> and <see cref="XmlLoggerAdapter"/>.
    /// </summary>
    public interface IRunnerLogger
    {
        /// <summary>
        /// Called when the assembly has finished running.
        /// </summary>
        /// <param name="assemblyFileName">The assembly filename.</param>
        /// <param name="total">The total number of tests run.</param>
        /// <param name="failed">The number of failed tests.</param>
        /// <param name="skipped">The number of skipped tests.</param>
        /// <param name="time">The time taken to run, in seconds.</param>
        void AssemblyFinished(string assemblyFileName, int total, int failed, int skipped, double time);

        /// <summary>
        /// Called when the assembly has started running.
        /// </summary>
        /// <param name="assemblyFileName">The assembly filename.</param>
        /// <param name="configFileName">The configuration filename, if given; null, otherwise.</param>
        /// <param name="xunitVersion">The version of xunit.dll.</param>
        void AssemblyStart(string assemblyFileName, string configFileName, string xunitVersion);

        /// <summary>
        /// Called when a class failure is encountered (i.e., when a fixture from
        /// IUseFixture throws an exception during construction or <see cref="IDisposable.Dispose"/>.
        /// </summary>
        /// <param name="className">The full type name of the class.</param>
        /// <param name="exceptionType">The full type name of the exception.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="stackTrace">The exception stack trace.</param>
        /// <returns></returns>
        bool ClassFailed(string className, string exceptionType, string message, string stackTrace);

        /// <summary>
        /// Called when an exception is thrown (i.e., a catastrophic failure of the testing system).
        /// </summary>
        /// <param name="assemblyFileName">The assembly filename.</param>
        /// <param name="exception">The exception that was thrown.</param>
        void ExceptionThrown(string assemblyFileName, Exception exception);

        /// <summary>
        /// Called when a test fails.
        /// </summary>
        /// <param name="name">The description name of the test.</param>
        /// <param name="type">The full type name of the test class.</param>
        /// <param name="method">The name of the method.</param>
        /// <param name="duration">The time spent running the test, in seconds.</param>
        /// <param name="output">The output of the test during its run.</param>
        /// <param name="exceptionType">The full type name of the exception.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="stackTrace">The exception stack trace.</param>
        void TestFailed(string name, string type, string method, double duration, string output, string exceptionType, string message, string stackTrace);

        /// <summary>
        /// Called when a test has finished running, regardless of what the result was.
        /// </summary>
        /// <param name="name">The description name of the test.</param>
        /// <param name="type">The full type name of the test class.</param>
        /// <param name="method">The name of the method.</param>
        /// <returns>Return true to continue running tests; return false to stop the test run.</returns>
        bool TestFinished(string name, string type, string method);

        /// <summary>
        /// Called when a test has passed.
        /// </summary>
        /// <param name="name">The description name of the test.</param>
        /// <param name="type">The full type name of the test class.</param>
        /// <param name="method">The name of the method.</param>
        /// <param name="duration">The time spent running the test, in seconds.</param>
        /// <param name="output">The output of the test during its run.</param>
        void TestPassed(string name, string type, string method, double duration, string output);

        /// <summary>
        /// Called when a test was finished.
        /// </summary>
        /// <param name="name">The description name of the test.</param>
        /// <param name="type">The full type name of the test class.</param>
        /// <param name="method">The name of the method.</param>
        /// <param name="reason">The skip reason.</param>
        void TestSkipped(string name, string type, string method, string reason);

        /// <summary>
        /// Called when a test has started running.
        /// </summary>
        /// <param name="name">The description name of the test.</param>
        /// <param name="type">The full type name of the test class.</param>
        /// <param name="method">The name of the method.</param>
        /// <returns>Return true to continue running tests; return false to stop the test run.</returns>
        bool TestStart(string name, string type, string method);
    }
}