//using System;
//using System.Globalization;

//namespace Xunit.ConsoleClient
//{
//    public class StandardRunnerCallback : RunnerCallback
//    {
//        readonly bool silent;
//        int testCount = 0;
//        readonly int totalCount;

//        public StandardRunnerCallback(bool silent, int totalCount)
//        {
//            this.silent = silent;
//            this.totalCount = totalCount;
//        }

//        public override void AssemblyFinished(TestAssembly testAssembly, int total, int failed, int skipped, double time)
//        {
//            base.AssemblyFinished(testAssembly, total, failed, skipped, time);

//            if (!silent)
//                Console.Write("\r");

//            Console.WriteLine("{0} total, {1} failed, {2} skipped, took {3} seconds", total, failed, skipped, time.ToString("0.000", CultureInfo.CurrentCulture));
//        }

//        public override bool ClassFailed(TestClass testClass, string exceptionType, string message, string stackTrace)
//        {
//            if (!silent)
//                Console.Write("\r");

//            Console.ForegroundColor = ConsoleColor.Red;
//            Console.WriteLine("{0} [FIXTURE FAIL]", testClass.TypeName);
//            Console.ResetColor();

//            Console.WriteLine(Indent(message));

//            if (stackTrace != null)
//            {
//                Console.WriteLine(Indent("Stack Trace:"));
//                Console.WriteLine(Indent(StackFrameTransformer.TransformStack(stackTrace)));
//            }

//            Console.WriteLine();
//            return true;
//        }

//        protected override void TestFailed(TestMethod testMethod, TestFailedResult result)
//        {
//            if (!silent)
//                Console.Write("\r");

//            Console.ForegroundColor = ConsoleColor.Red;
//            Console.WriteLine("{0} [FAIL]", result.DisplayName);
//            Console.ResetColor();

//            Console.WriteLine(Indent(result.ExceptionMessage));

//            if (result.ExceptionStackTrace != null)
//            {
//                Console.ForegroundColor = ConsoleColor.DarkGray;
//                Console.WriteLine(Indent("Stack Trace:"));
//                Console.ResetColor();

//                Console.WriteLine(Indent(StackFrameTransformer.TransformStack(result.ExceptionStackTrace)));
//            }

//            Console.WriteLine();
//        }

//        protected override bool TestFinished(TestMethod testMethod, TestResult testResult)
//        {
//            if (!silent)
//            {
//                Console.ForegroundColor = ConsoleColor.DarkGray;
//                Console.Write("\rTests complete: {0} of {1}", ++testCount, totalCount);
//                Console.ResetColor();
//            }

//            return true;
//        }

//        protected override void TestSkipped(TestMethod testMethod, TestSkippedResult result)
//        {
//            if (!silent)
//                Console.Write("\r");

//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine("{0} [SKIP]", result.DisplayName);
//            Console.ResetColor();

//            Console.WriteLine(Indent(result.Reason));
//            Console.WriteLine();
//        }

//        // Helpers

//        string Indent(string message)
//        {
//            return Indent(message, 0);
//        }

//        string Indent(string message, int additionalSpaces)
//        {
//            string result = "";
//            string indent = "".PadRight(additionalSpaces + 3);

//            foreach (string line in message.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
//                result += indent + line + Environment.NewLine;

//            return result.TrimEnd();
//        }
//    }
//}