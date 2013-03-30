using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit.Abstractions;

namespace Xunit.Runner.MSBuild
{
    public class StandardOutputVisitor : TestMessageVisitor<ITestAssemblyFinished>
    {
        Func<bool> cancelThunk;
        TaskLoggingHelper log;

        public int Total = 0;
        public int Failed = 0;
        public int Skipped = 0;
        public decimal Time = 0.0M;

        public StandardOutputVisitor(TaskLoggingHelper log, Func<bool> cancelThunk)
        {
            this.log = log;
            this.cancelThunk = cancelThunk;
        }

        protected override void Visit(ITestAssemblyFinished assemblyFinished)
        {
            log.LogMessage(MessageImportance.High,
                           "  Tests: {0}, Failures: {1}, Skipped: {2}, Time: {3} seconds",
                           assemblyFinished.TestsRun,
                           assemblyFinished.TestsFailed,
                           assemblyFinished.TestsSkipped,
                           assemblyFinished.ExecutionTime.ToString("0.000"));

            Total += assemblyFinished.TestsRun;
            Failed += assemblyFinished.TestsFailed;
            Skipped += assemblyFinished.TestsSkipped;
            Time += assemblyFinished.ExecutionTime;
        }

        protected override void Visit(IErrorMessage error)
        {
            log.LogError("{0}: {1}", error.ExceptionType, Escape(error.Message));
            log.LogError(error.StackTrace);
        }

        protected override void Visit(ITestFailed testFailed)
        {
            log.LogError("{0}: {1}", Escape(testFailed.TestDisplayName), Escape(testFailed.Message));
            log.LogError(testFailed.StackTrace);
        }

        protected override void Visit(ITestPassed testPassed)
        {
            log.LogMessage("    {0}", Escape(testPassed.TestDisplayName));
        }

        protected override void Visit(ITestSkipped testSkipped)
        {
            log.LogWarning("{0}: {1}", Escape(testSkipped.TestDisplayName), Escape(testSkipped.Reason));
        }

        static string Escape(string value)
        {
            if (value == null)
                return String.Empty;

            return value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }
    }
}