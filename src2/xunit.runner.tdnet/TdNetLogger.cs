//using System;
//using System.Reflection;
//using TestDriven.Framework;

//namespace Xunit.Runner.TdNet
//{
//    using TestResult = TestDriven.Framework.TestResult;

//    public class TdNetLogger : IRunnerLogger
//    {
//        private readonly Assembly assembly;
//        private readonly ITestListener listener;

//        public TdNetLogger(ITestListener listener, Assembly assembly)
//        {
//            this.listener = listener;
//            this.assembly = assembly;
//        }

//        public bool FoundTests { get; private set; }

//        public void AssemblyFinished(string assemblyFilename, int total, int failed, int skipped, double time) { }

//        public void AssemblyStart(string assemblyFilename, string configFilename, string xUnitVersion) { }

//        public bool ClassFailed(string className, string exceptionType, string message, string stackTrace)
//        {
//            Type type = assembly.GetType(className);

//            TestResult testResult =
//                new TestResult
//                    {
//                        FixtureType = type,
//                        Name = ("Fixture " + type.FullName),
//                        TotalTests = 1,
//                        State = TestState.Failed,
//                        Message = message,
//                        StackTrace = stackTrace
//                    };

//            if (listener != null)
//                listener.TestFinished(testResult);

//            return true;
//        }

//        public void ExceptionThrown(string assemblyFilename, Exception exception)
//        {
//            if (listener != null)
//                listener.WriteLine(exception.ToString(), Category.Error);
//        }

//        public void TestFailed(string name, string type, string method, double duration, string output, string exceptionType, string message, string stackTrace)
//        {
//            TestResult testResult = CreateTestResult(type, method, name, duration);

//            testResult.State = TestState.Failed;
//            testResult.Message = message;
//            testResult.StackTrace = stackTrace;

//            if (listener != null)
//                listener.TestFinished(testResult);

//            WriteOutput(name, output);
//        }

//        public bool TestFinished(string name, string type, string method)
//        {
//            FoundTests = true;
//            return true;
//        }

//        public void TestPassed(string name, string type, string method, double duration, string output)
//        {
//            TestResult testResult = CreateTestResult(type, method, name, duration);

//            testResult.State = TestState.Passed;

//            if (listener != null)
//                listener.TestFinished(testResult);

//            WriteOutput(name, output);
//        }

//        public void TestSkipped(string name, string type, string method, string reason)
//        {
//            TestResult testResult = CreateTestResult(type, method, name, 0);

//            testResult.State = TestState.Ignored;
//            testResult.Message = reason;

//            if (listener != null)
//                listener.TestFinished(testResult);
//        }

//        public bool TestStart(string name, string type, string method)
//        {
//            return true;
//        }

//        TestResult CreateTestResult(string type, string method, string name, double duration)
//        {
//            Type fixtureType = assembly.GetType(type);

//            return new TestResult
//                       {
//                           FixtureType = fixtureType,
//                           Method = fixtureType.GetMethod(method),
//                           Name = name,
//                           TimeSpan = new TimeSpan((long)(10000.0 * duration)),
//                           TotalTests = 1,
//                       };
//        }

//        private void WriteOutput(string name, string output)
//        {
//            if (output != null)
//            {
//                listener.WriteLine(String.Format("Output from {0}:", name), Category.Output);
//                foreach (string line in output.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
//                    listener.WriteLine(String.Format("  {0}", line), Category.Output);
//            }
//        }
//    }
//}