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

        public StandardOutputVisitor(object consoleLock,
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
            assemblyFileName = Path.GetFileName(assemblyStarting.AssemblyFileName);

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
                    Time = assemblyFinished.ExecutionTime
                });

            return result;
        }

        protected override bool Visit(IErrorMessage error)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("   {0} [FATAL]", Escape(error.ExceptionTypes[0]));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Error.WriteLine("      {0}", Escape(ExceptionUtility.CombineMessages(error)));

                WriteStackTrace(ExceptionUtility.CombineStackTraces(error));
            }

            return base.Visit(error);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            lock (consoleLock)
            {
                // TODO: Thread-safe way to figure out the default foreground color
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("   {0} [FAIL]", Escape(testFailed.TestDisplayName));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Error.WriteLine("      {0}", Escape(ExceptionUtility.CombineMessages(testFailed)));

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
                Console.Error.WriteLine("   {0} [SKIP]", Escape(testSkipped.TestDisplayName));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Error.WriteLine("      {0}", Escape(testSkipped.Reason));
            }

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            return base.Visit(testStarting);
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