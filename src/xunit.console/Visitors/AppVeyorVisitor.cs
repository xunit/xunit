using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace Xunit.ConsoleClient
{
    public class AppVeyorVisitor : XmlTestExecutionVisitor
    {
        const string FrameworkName = "xUnit";

        string assemblyFileName;
        readonly object consoleLock;
        readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages;
        readonly string defaultDirectory;
        readonly ConcurrentDictionary<string, int> testMethods = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public AppVeyorVisitor(object consoleLock,
                               string defaultDirectory,
                               XElement assemblyElement,
                               Func<bool> cancelThunk,
                               ConcurrentDictionary<string, ExecutionSummary> completionMessages = null)
            : base(assemblyElement, cancelThunk)
        {
            this.consoleLock = consoleLock;
            this.defaultDirectory = defaultDirectory;
            this.completionMessages = completionMessages;
        }

        protected override bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            assemblyFileName = Path.GetFileName(assemblyStarting.TestAssembly.Assembly.AssemblyPath);

            lock (consoleLock)
                Console.WriteLine("Starting: {0}", assemblyFileName);

            return base.Visit(assemblyStarting);
        }

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            // Base class does computation of results, so call it first.
            var result = base.Visit(assemblyFinished);

            lock (consoleLock)
                Console.WriteLine("Finished: {0}", assemblyFileName);

            if (completionMessages != null)
                completionMessages.TryAdd(assemblyFileName, new ExecutionSummary
                {
                    Total = assemblyFinished.TestsRun,
                    Failed = assemblyFinished.TestsFailed,
                    Skipped = assemblyFinished.TestsSkipped,
                    Time = assemblyFinished.ExecutionTime,
                    Errors = Errors
                });

            return result;
        }

        protected override bool Visit(IErrorMessage error)
        {
            lock (consoleLock)
            {
                Console.Error.WriteLine("   {0} [FATAL]", Escape(error.ExceptionTypes[0]));
                Console.Error.WriteLine("      {0}", Escape(ExceptionUtility.CombineMessages(error)));

                WriteStackTrace(ExceptionUtility.CombineStackTraces(error));
            }

            return base.Visit(error);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            var testName = testStarting.Test.DisplayName;
            if (testMethods.ContainsKey(testName))
                testName = String.Format("{0} {1}", testName, testMethods[testName]);

            AppVeyorLogger.AddTest(testName, FrameworkName, assemblyFileName, "Running", null, null, null, null, null);

            return base.Visit(testStarting);
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            AppVeyorLogger.UpdateTest(GetFinishedTestName(testPassed.Test.DisplayName), FrameworkName, assemblyFileName, "Passed",
                                      Convert.ToInt64(testPassed.ExecutionTime * 1000), null, null, testPassed.Output, null);

            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            AppVeyorLogger.UpdateTest(GetFinishedTestName(testSkipped.Test.DisplayName), FrameworkName, assemblyFileName, "Skipped",
                                      Convert.ToInt64(testSkipped.ExecutionTime * 1000), null, null, null, null);

            lock (consoleLock)
            {
                Console.Error.WriteLine("   {0} [SKIP]", Escape(testSkipped.Test.DisplayName));
                Console.Error.WriteLine("      {0}", Escape(testSkipped.Reason));
            }

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            AppVeyorLogger.UpdateTest(GetFinishedTestName(testFailed.Test.DisplayName), FrameworkName, assemblyFileName, "Failed",
                                      Convert.ToInt64(testFailed.ExecutionTime * 1000), ExceptionUtility.CombineMessages(testFailed),
                                      ExceptionUtility.CombineStackTraces(testFailed), testFailed.Output, null);

            lock (consoleLock)
            {
                Console.Error.WriteLine("   {0} [FAIL]", Escape(testFailed.Test.DisplayName));
                Console.Error.WriteLine("      {0}", Escape(ExceptionUtility.CombineMessages(testFailed)));

                WriteStackTrace(ExceptionUtility.CombineStackTraces(testFailed));
            }

            return base.Visit(testFailed);
        }

        protected override bool Visit(IAfterTestFinished afterTestFinished)
        {
            Console.Write(".");

            return base.Visit(afterTestFinished);
        }

        private string GetFinishedTestName(string methodName)
        {
            var testName = methodName;
            var number = 0;

            if (testMethods.ContainsKey(methodName))
            {
                number = testMethods[methodName];
                testName = String.Format("{0} {1}", methodName, number);
            }

            testMethods[methodName] = number + 1;

            return testName;
        }

        void WriteStackTrace(string stackTrace)
        {
            if (String.IsNullOrWhiteSpace(stackTrace))
                return;

            Console.Error.WriteLine("      Stack Trace:");
            Array.ForEach(stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None),
                          stackFrame => Console.Error.WriteLine("         {0}", StackFrameTransformer.TransformFrame(stackFrame, defaultDirectory)));
        }
    }
}