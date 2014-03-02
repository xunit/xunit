using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Xunit.ConsoleClient
{
    public class StandardRunnerCallback : RunnerCallback
    {
	    readonly bool silent;
        int testCount = 0;
        readonly int totalCount;
		private Stopwatch testTimer = new Stopwatch();
	    private StreamWriter timing, log;

	    public StandardRunnerCallback(string timingReport, bool silent, int totalCount)
        {
	        timing = File.CreateText(timingReport);
		    timing.AutoFlush = true;

		    log = File.CreateText("test.log");
		    log.AutoFlush = true;

	        this.silent = silent;
            this.totalCount = totalCount;
        }

        public override void AssemblyFinished(TestAssembly testAssembly, int total, int failed, int skipped, double time)
        {
			timing.Dispose();
			
			base.AssemblyFinished(testAssembly, total, failed, skipped, time);

            if (!silent)
                Console.Write("\r");

            Console.WriteLine("{0} total, {1} failed, {2} skipped, took {3} seconds", total, failed, skipped, time.ToString("0.000", CultureInfo.InvariantCulture));
			log.WriteLine("{0} total, {1} failed, {2} skipped, took {3} seconds", total, failed, skipped, time.ToString("0.000", CultureInfo.InvariantCulture));
        }

        public override bool ClassFailed(TestClass testClass, string exceptionType, string message, string stackTrace)
        {
            if (!silent)
                Console.Write("\r");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0} [FIXTURE FAIL]", testClass.TypeName);
			log.WriteLine("{0} [FIXTURE FAIL]", testClass.TypeName);
            Console.ResetColor();

            Console.WriteLine(Indent(message));

            if (stackTrace != null)
            {
                Console.WriteLine(Indent("Stack Trace:"));
				log.WriteLine(Indent("Stack Trace:"));
                Console.WriteLine(Indent(StackFrameTransformer.TransformStack(stackTrace)));
				log.WriteLine(Indent(StackFrameTransformer.TransformStack(stackTrace)));
            }

            Console.WriteLine();
			log.WriteLine();
            return true;
        }

	    public override bool TestStart(TestMethod method)
	    {
			testTimer.Reset();
			testTimer.Start();
		    try
		    {
			    File.WriteAllText("last-test.txt", method.DisplayName);
		    }
		    catch (Exception)
		    {
		    }			
		    try
		    {
			    Console.Title = method.DisplayName.Length > 1024 ? method.DisplayName.Substring(0, 1024) : method.DisplayName;
		    }
		    catch (Exception)
		    {
		    }

		    return base.TestStart(method);
	    }

	    protected override void TestFailed(TestMethod testMethod, TestFailedResult result)
        {
            if (!silent)
                Console.Write("\r");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0} [FAIL]", result.DisplayName);
			log.WriteLine("{0} [FAIL]", result.DisplayName);
            Console.ResetColor();

            Console.WriteLine(Indent(result.ExceptionMessage));

            if (result.ExceptionStackTrace != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(Indent("Stack Trace:"));
				log.WriteLine(Indent("Stack Trace:"));
                Console.ResetColor();

                Console.WriteLine(Indent(StackFrameTransformer.TransformStack(result.ExceptionStackTrace)));
				log.WriteLine(Indent(StackFrameTransformer.TransformStack(result.ExceptionStackTrace)));
            }

            Console.WriteLine();
			log.WriteLine();
        }

        protected override bool TestFinished(TestMethod testMethod, TestResult testResult)
        {
	        testTimer.Stop();
			timing.WriteLine("{0}\t{1}", testMethod.DisplayName,testTimer.ElapsedMilliseconds);
			
            if (!silent)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("\rTests complete: {0} of {1}", ++testCount, totalCount);
				log.Write("\rTests complete: {0} of {1}", ++testCount, totalCount);
                Console.ResetColor();
            }

            return true;
        }

        protected override void TestSkipped(TestMethod testMethod, TestSkippedResult result)
        {
            if (!silent)
                Console.Write("\r");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0} [SKIP]", result.DisplayName);
			log.WriteLine("{0} [SKIP]", result.DisplayName);
            Console.ResetColor();

            Console.WriteLine(Indent(result.Reason));
			log.WriteLine(Indent(result.Reason));
            Console.WriteLine();
			log.WriteLine();
        }

        // Helpers

        string Indent(string message)
        {
            return Indent(message, 0);
        }

        string Indent(string message, int additionalSpaces)
        {
            string result = "";
            string indent = "".PadRight(additionalSpaces + 3);

            foreach (string line in message.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                result += indent + line + Environment.NewLine;

            return result.TrimEnd();
        }
    }
}