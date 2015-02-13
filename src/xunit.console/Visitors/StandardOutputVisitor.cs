using System;
using System.Collections.Concurrent;
using System.IO;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace Xunit.ConsoleClient
{
    public class StandardOutputVisitor : XmlTestExecutionVisitor
    {
        string assemblyFileName;
        readonly object consoleLock;
        readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages;
        readonly string defaultDirectory;
        readonly bool quiet;

        public StandardOutputVisitor(object consoleLock,
                                     bool quiet,
                                     string defaultDirectory,
                                     XElement assemblyElement,
                                     Func<bool> cancelThunk,
                                     ConcurrentDictionary<string, ExecutionSummary> completionMessages = null)
            : base(assemblyElement, cancelThunk)
        {
            this.consoleLock = consoleLock;
            this.quiet = quiet;
            this.defaultDirectory = defaultDirectory;
            this.completionMessages = completionMessages;
        }

        protected override bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            assemblyFileName = Path.GetFileName(assemblyStarting.TestAssembly.Assembly.AssemblyPath);

            if (!quiet)
                lock (consoleLock)
                    Console.WriteLine("Starting:    {0}", Path.GetFileNameWithoutExtension(assemblyFileName));

            return base.Visit(assemblyStarting);
        }

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            // Base class does computation of results, so call it first.
            var result = base.Visit(assemblyFinished);

            if (!quiet)
                lock (consoleLock)
                    Console.WriteLine("Finished:    {0}", Path.GetFileNameWithoutExtension(assemblyFileName));

            if (completionMessages != null)
                completionMessages.TryAdd(Path.GetFileNameWithoutExtension(assemblyFileName), new ExecutionSummary
                {
                    Total = assemblyFinished.TestsRun,
                    Failed = assemblyFinished.TestsFailed,
                    Skipped = assemblyFinished.TestsSkipped,
                    Time = assemblyFinished.ExecutionTime,
                    Errors = Errors
                });

            return result;
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            lock (consoleLock)
            {
                // TODO: Thread-safe way to figure out the default foreground color
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("   {0} [FAIL]", Escape(testFailed.Test.DisplayName));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Error.WriteLine("      {0}", ExceptionUtility.CombineMessages(testFailed).Replace(Environment.NewLine, Environment.NewLine + "      "));

                WriteStackTrace(ExceptionUtility.CombineStackTraces(testFailed));
            }

            return base.Visit(testFailed);
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            lock (consoleLock)
            {
                // TODO: Thread-safe way to figure out the default foreground color
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("   {0} [SKIP]", Escape(testSkipped.Test.DisplayName));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("      {0}", Escape(testSkipped.Reason));
            }

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            return base.Visit(testStarting);
        }

        protected override bool Visit(IErrorMessage error)
        {
            WriteError("FATAL", error);

            return base.Visit(error);
        }

        protected override bool Visit(ITestAssemblyCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Assembly Cleanup Failure ({0})", cleanupFailure.TestAssembly.Assembly.AssemblyPath), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCaseCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Case Cleanup Failure ({0})", cleanupFailure.TestCase.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestClassCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Class Cleanup Failure ({0})", cleanupFailure.TestClass.Class.Name), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCollectionCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Collection Cleanup Failure ({0})", cleanupFailure.TestCollection.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Cleanup Failure ({0})", cleanupFailure.Test.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestMethodCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Method Cleanup Failure ({0})", cleanupFailure.TestMethod.Method.Name), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected void WriteError(string failureName, IFailureInformation failureInfo)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("   [{0}] {1}", failureName, Escape(failureInfo.ExceptionTypes[0]));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Error.WriteLine("      {0}", Escape(ExceptionUtility.CombineMessages(failureInfo)));

                WriteStackTrace(ExceptionUtility.CombineStackTraces(failureInfo));
            }
        }

        void WriteStackTrace(string stackTrace)
        {
            if (String.IsNullOrWhiteSpace(stackTrace))
                return;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Error.WriteLine("      Stack Trace:");

            Console.ForegroundColor = ConsoleColor.Gray;
            Array.ForEach(stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None),
                          stackFrame => Console.Error.WriteLine("         {0}", StackFrameTransformer.TransformFrame(stackFrame, defaultDirectory)));
        }
    }
}