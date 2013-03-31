using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit.Abstractions;

namespace Xunit.Runner.MSBuild
{
    public class StandardOutputVisitor : MSBuildVisitor
    {
        public StandardOutputVisitor(TaskLoggingHelper log, Func<bool> cancelThunk)
            : base(log, cancelThunk) { }

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            base.Visit(assemblyFinished);

            Log.LogMessage(MessageImportance.High,
                           "  Tests: {0}, Failures: {1}, Skipped: {2}, Time: {3} seconds",
                           assemblyFinished.TestsRun,
                           assemblyFinished.TestsFailed,
                           assemblyFinished.TestsSkipped,
                           assemblyFinished.ExecutionTime.ToString("0.000"));

            return !CancelThunk();
        }

        protected override bool Visit(IErrorMessage error)
        {
            Log.LogError("{0}: {1}", error.ExceptionType, Escape(error.Message));
            Log.LogError(error.StackTrace);

            return !CancelThunk();
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            Log.LogError("{0}: {1}", Escape(testFailed.TestDisplayName), Escape(testFailed.Message));
            Log.LogError(testFailed.StackTrace);

            return !CancelThunk();
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            Log.LogMessage("    {0}", Escape(testPassed.TestDisplayName));

            return !CancelThunk();
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            Log.LogWarning("{0}: {1}", Escape(testSkipped.TestDisplayName), Escape(testSkipped.Reason));

            return !CancelThunk();
        }
    }
}