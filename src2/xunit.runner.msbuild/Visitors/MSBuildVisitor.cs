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
            CancelThunk = cancelThunk;
        }

        public readonly Func<bool> CancelThunk;
        public int Failed;
        public readonly TaskLoggingHelper Log;
        public int Skipped;
        public decimal Time;
        public int Total;
    }
}