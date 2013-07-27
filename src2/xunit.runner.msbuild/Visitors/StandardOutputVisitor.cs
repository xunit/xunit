using System;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit.Abstractions;

namespace Xunit.Runner.MSBuild
{
    public class StandardOutputVisitor : MSBuildVisitor
    {
        private readonly bool verbose;

        public StandardOutputVisitor(TaskLoggingHelper log, XElement assemblyElement, bool verbose, Func<bool> cancelThunk)
            : base(log, assemblyElement, cancelThunk)
        {
            this.verbose = verbose;
        }

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            // Base class does computation of results, so call it first.
            var result = base.Visit(assemblyFinished);

            Log.LogMessage(MessageImportance.High,
                           "  Tests: {0}, Failures: {1}, Skipped: {2}, Time: {3} seconds",
                           assemblyFinished.TestsRun,
                           assemblyFinished.TestsFailed,
                           assemblyFinished.TestsSkipped,
                           assemblyFinished.ExecutionTime.ToString("0.000"));

            return result;
        }

        protected override bool Visit(IErrorMessage error)
        {
            Log.LogError("{0}: {1}", error.ExceptionType, Escape(error.Message));
            Log.LogError(error.StackTrace);

            return base.Visit(error);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            Log.LogError("{0}: {1}", Escape(testFailed.TestDisplayName), Escape(testFailed.Message));

            if (!String.IsNullOrWhiteSpace(testFailed.StackTrace))
                Log.LogError(testFailed.StackTrace);

            return base.Visit(testFailed);
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            if (verbose)
                Log.LogMessage("    PASS:  {0}", Escape(testPassed.TestDisplayName));
            else
                Log.LogMessage("    {0}", Escape(testPassed.TestDisplayName));

            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            Log.LogWarning("{0}: {1}", Escape(testSkipped.TestDisplayName), Escape(testSkipped.Reason));

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            if (verbose)
                Log.LogMessage("    START: {0}", Escape(testStarting.TestDisplayName));

            return base.Visit(testStarting);
        }
    }
}