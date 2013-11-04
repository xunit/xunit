using System;
using System.Collections.Concurrent;
using System.IO;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace Xunit.ConsoleClient
{
    public class StandardOutputVisitor : XmlTestExecutionVisitor
    {
        private readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages;
        private readonly bool verbose;

        private string assemblyFileName;

        public StandardOutputVisitor(XElement assemblyElement,
                                     bool verbose,
                                     Func<bool> cancelThunk,
                                     ConcurrentDictionary<string, ExecutionSummary> completionMessages = null)
            : base(assemblyElement, cancelThunk)
        {
            this.completionMessages = completionMessages;
            this.verbose = verbose;
        }

        protected override bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            assemblyFileName = Path.GetFileName(assemblyStarting.AssemblyFileName);
            Console.WriteLine("  Started: {0}", assemblyFileName);

            return base.Visit(assemblyStarting);
        }

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            // Base class does computation of results, so call it first.
            var result = base.Visit(assemblyFinished);

            Console.WriteLine("  Finished: {0}", assemblyFileName);

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
            Console.Error.WriteLine("{0}: {1}", error.ExceptionType, Escape(error.Message));
            Console.Error.WriteLine(error.StackTrace);

            return base.Visit(error);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            Console.Error.WriteLine("{0}: {1}", Escape(testFailed.TestDisplayName), Escape(testFailed.Message));

            if (!String.IsNullOrWhiteSpace(testFailed.StackTrace))
                Console.Error.WriteLine(testFailed.StackTrace);

            return base.Visit(testFailed);
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            if (verbose)
                Console.WriteLine("    PASS:  {0}", Escape(testPassed.TestDisplayName));
            else
                Console.WriteLine("    {0}", Escape(testPassed.TestDisplayName));

            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            Console.WriteLine("{0}: {1}", Escape(testSkipped.TestDisplayName), Escape(testSkipped.Reason));

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            if (verbose)
                Console.WriteLine("    START: {0}", Escape(testStarting.TestDisplayName));

            return base.Visit(testStarting);
        }
    }
}