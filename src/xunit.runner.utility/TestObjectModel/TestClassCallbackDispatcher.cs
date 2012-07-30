using System;

namespace Xunit
{
    /// <summary>
    /// Acts as an <see cref="IRunnerLogger"/> and adapts the callback messages
    /// into calls to an instance of <see cref="ITestMethodRunnerCallback"/>.
    /// </summary>
    public class TestClassCallbackDispatcher : IRunnerLogger
    {
        TestClass testClass;
        ITestMethodRunnerCallback callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassCallbackDispatcher"/> class.
        /// </summary>
        /// <param name="testClass">The test class.</param>
        /// <param name="callback">The run status information callback.</param>
        public TestClassCallbackDispatcher(TestClass testClass, ITestMethodRunnerCallback callback)
        {
            this.testClass = testClass;
            this.callback = callback;
        }

        /// <inheritdoc/>
        public void AssemblyFinished(string assemblyFileName, int total, int failed, int skipped, double time)
        {
            callback.AssemblyFinished(testClass.TestAssembly, total, failed, skipped, time);
        }

        /// <inheritdoc/>
        public void AssemblyStart(string assemblyFileName, string configFileName, string xunitVersion)
        {
            callback.AssemblyStart(testClass.TestAssembly);
        }

        /// <inheritdoc/>
        public bool ClassFailed(string className, string exceptionType, string message, string stackTrace)
        {
            // TODO: Should this information be part of TestClass?
            return callback.ClassFailed(testClass, exceptionType, message, stackTrace);
        }

        /// <inheritdoc/>
        public void ExceptionThrown(string assemblyFileName, Exception exception)
        {
            callback.ExceptionThrown(testClass.TestAssembly, exception);
        }

        /// <inheritdoc/>
        public void TestFailed(string name, string type, string method, double duration, string output, string exceptionType, string message, string stackTrace)
        {
            TestMethod testMethod = testClass.GetMethod(method);
            testMethod.RunResults.Add(new TestFailedResult(duration, name, output, exceptionType, message, stackTrace));
        }

        /// <inheritdoc/>
        public bool TestFinished(string name, string type, string method)
        {
            // TODO: Cache test method lookup?
            return callback.TestFinished(testClass.GetMethod(method));
        }

        /// <inheritdoc/>
        public void TestPassed(string name, string type, string method, double duration, string output)
        {
            TestMethod testMethod = testClass.GetMethod(method);
            testMethod.RunResults.Add(new TestPassedResult(duration, name, output));
        }

        /// <inheritdoc/>
        public void TestSkipped(string name, string type, string method, string reason)
        {
            TestMethod testMethod = testClass.GetMethod(method);
            testMethod.RunResults.Add(new TestSkippedResult(name, reason));
        }

        /// <inheritdoc/>
        public bool TestStart(string name, string type, string method)
        {
            return callback.TestStart(testClass.GetMethod(method));
        }
    }
}
