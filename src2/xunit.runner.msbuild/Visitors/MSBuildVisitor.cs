using System;
using Microsoft.Build.Utilities;
using Xunit.Abstractions;

namespace Xunit.Runner.MSBuild
{
    public class MSBuildVisitor : TestMessageVisitor<ITestAssemblyFinished>
    {
        public MSBuildVisitor(TaskLoggingHelper log, Func<bool> cancelThunk)
        {
            Log = log;
            CancelThunk = cancelThunk ?? (() => false);
        }

        public readonly Func<bool> CancelThunk;
        public int Failed;
        public readonly TaskLoggingHelper Log;
        public int Skipped;
        public decimal Time;
        public int Total;

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            Total += assemblyFinished.TestsRun;
            Failed += assemblyFinished.TestsFailed;
            Skipped += assemblyFinished.TestsSkipped;
            Time += assemblyFinished.ExecutionTime;

            return base.Visit(assemblyFinished);
        }

        protected static string Escape(string value)
        {
            if (value == null)
                return String.Empty;

            return value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }
    }
}