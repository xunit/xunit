//using System;
//using System.IO;

//namespace Xunit.ConsoleClient
//{
//    public class TeamCityRunnerCallback : RunnerCallback
//    {
//        public override void AssemblyFinished(TestAssembly testAssembly, int total, int failed, int skipped, double time)
//        {
//            base.AssemblyFinished(testAssembly, total, failed, skipped, time);

//            Console.WriteLine(
//                "##teamcity[testSuiteFinished name='{0}']",
//                Escape(Path.GetFileName(testAssembly.AssemblyFilename))
//            );
//        }

//        public override void AssemblyStart(TestAssembly testAssembly)
//        {
//            Console.WriteLine(
//                "##teamcity[testSuiteStarted name='{0}']",
//                Escape(Path.GetFileName(testAssembly.AssemblyFilename))
//            );
//        }

//        public override bool ClassFailed(TestClass testClass, string exceptionType, string message, string stackTrace)
//        {
//            Console.WriteLine(
//                "##teamcity[buildStatus status='FAILURE' text='Class failed: {0}: {1}|r|n{2}']",
//                Escape(testClass.TypeName),
//                Escape(message),
//                Escape(stackTrace)
//            );

//            return true;
//        }

//        protected override void TestFailed(TestMethod method, TestFailedResult result)
//        {
//            Console.WriteLine(
//                "##teamcity[testFailed name='{0}' details='{1}|r|n{2}']",
//                Escape(method.DisplayName),
//                Escape(result.ExceptionMessage),
//                Escape(result.ExceptionStackTrace)
//            );

//            WriteOutput(result.DisplayName, result.Output);
//        }

//        protected override bool TestFinished(TestMethod method, TestResult result)
//        {
//            WriteFinished(method.DisplayName, result.Duration);
//            return true;
//        }

//        protected override void TestPassed(TestMethod method, TestPassedResult result)
//        {
//            WriteOutput(method.DisplayName, result.Output);
//        }

//        protected override void TestSkipped(TestMethod method, TestSkippedResult result)
//        {
//            Console.WriteLine(
//                "##teamcity[testIgnored name='{0}' message='{1}']",
//                Escape(method.DisplayName),
//                Escape(result.Reason)
//            );
//        }

//        public override bool TestStart(TestMethod method)
//        {
//            Console.WriteLine(
//                "##teamcity[testStarted name='{0}']",
//                Escape(method.DisplayName)
//            );

//            return true;
//        }

//        // Helpers

//        static string Escape(string value)
//        {
//            if (value == null)
//                return String.Empty;

//            return value.Replace("|", "||")
//                        .Replace("'", "|'")
//                        .Replace("\r", "|r")
//                        .Replace("\n", "|n")
//                        .Replace("]", "|]");
//        }

//        static void WriteFinished(string name, double duration)
//        {
//            Console.WriteLine("##teamcity[testFinished name='{0}' duration='{1}']",
//                                          Escape(name), (int)(duration * 1000D));
//        }

//        static void WriteOutput(string name, string output)
//        {
//            if (output != null)
//                Console.WriteLine("##teamcity[testStdOut name='{0}' out='{1}']", Escape(name), Escape(output));
//        }
//    }
//}