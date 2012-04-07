using System;

namespace Xunit
{
    /// <summary>
    /// The callback object which receives real-time status notifications from the
    /// test runner.
    /// </summary>
    public interface ITestMethodRunnerCallback
    {
        /// <summary>
        /// Called when the assembly has finished running.
        /// </summary>
        /// <param name="testAssembly">The test assembly.</param>
        /// <param name="total">The total number of tests run.</param>
        /// <param name="failed">The number of failed tests.</param>
        /// <param name="skipped">The number of skipped tests.</param>
        /// <param name="time">The time taken to run, in seconds.</param>
        void AssemblyFinished(TestAssembly testAssembly, int total, int failed, int skipped, double time);

        /// <summary>
        /// Called when the assembly has started running.
        /// </summary>
        /// <param name="testAssembly">The test assembly.</param>
        void AssemblyStart(TestAssembly testAssembly);

        /// <summary>
        /// Called when a class failure is encountered (i.e., when a fixture from
        /// IUseFixture throws an exception during construction or <see cref="IDisposable.Dispose"/>.
        /// </summary>
        /// <param name="testClass">The test class.</param>
        /// <param name="exceptionType">The full type name of the exception.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="stackTrace">The exception stack trace.</param>
        /// <returns></returns>
        bool ClassFailed(TestClass testClass, string exceptionType, string message, string stackTrace);

        /// <summary>
        /// Called when an exception is thrown (i.e., a catastrophic failure of the testing system).
        /// </summary>
        /// <param name="testAssembly">The test assembly.</param>
        /// <param name="exception">The exception that was thrown.</param>
        void ExceptionThrown(TestAssembly testAssembly, Exception exception);

        /// <summary>
        /// Called when a test has finished running, regardless of what the result was.
        /// </summary>
        /// <param name="testMethod">The test method.</param>
        /// <returns>Return true to continue running tests; return false to stop the test run.</returns>
        bool TestFinished(TestMethod testMethod);

        /// <summary>
        /// Called when a test has started running.
        /// </summary>
        /// <param name="testMethod">The test method.</param>
        /// <returns>Return true to continue running tests; return false to stop the test run.</returns>
        bool TestStart(TestMethod testMethod);
    }
}