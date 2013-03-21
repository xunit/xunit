//using System;

//namespace Xunit.ConsoleClient
//{
//    public class RunnerCallback : ITestMethodRunnerCallback
//    {
//        public int TotalTests { get; set; }
//        public int TotalFailures { get; set; }
//        public int TotalSkips { get; set; }
//        public double TotalTime { get; set; }

//        public virtual void AssemblyFinished(TestAssembly testAssembly, int total, int failed, int skipped, double time)
//        {
//            TotalTests = total;
//            TotalFailures = failed;
//            TotalSkips = skipped;
//            TotalTime = time;
//        }

//        public virtual void AssemblyStart(TestAssembly testAssembly)
//        {
//        }

//        public virtual bool ClassFailed(TestClass testClass, string exceptionType, string message, string stackTrace)
//        {
//            return true;
//        }

//        public virtual void ExceptionThrown(TestAssembly testAssembly, Exception exception)
//        {
//            Console.WriteLine();
//            Console.WriteLine("CATASTROPHIC ERROR OCCURRED:");
//            Console.WriteLine(exception.ToString());
//            Console.WriteLine("WHILE RUNNING:");
//            Console.WriteLine(testAssembly.AssemblyFilename);
//            Console.WriteLine();
//        }

//        protected virtual void TestFailed(TestMethod method, TestFailedResult result)
//        {
//        }

//        public bool TestFinished(TestMethod method)
//        {
//            TestResult result = method.RunResults[method.RunResults.Count - 1];

//            TestPassedResult passedResult = result as TestPassedResult;
//            if (passedResult != null)
//                TestPassed(method, passedResult);
//            else
//            {
//                TestFailedResult failedResult = result as TestFailedResult;
//                if (failedResult != null)
//                    TestFailed(method, failedResult);
//                else
//                {
//                    TestSkippedResult skippedResult = result as TestSkippedResult;
//                    if (skippedResult != null)
//                        TestSkipped(method, skippedResult);
//                }
//            }

//            return TestFinished(method, result);
//        }

//        protected virtual bool TestFinished(TestMethod method, TestResult result)
//        {
//            return true;
//        }

//        protected virtual void TestPassed(TestMethod method, TestPassedResult result)
//        {
//        }

//        protected virtual void TestSkipped(TestMethod method, TestSkippedResult result)
//        {
//        }

//        public virtual bool TestStart(TestMethod method)
//        {
//            return true;
//        }
//    }
//}