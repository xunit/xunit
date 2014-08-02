using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit.Abstractions;

namespace Xunit.Runner.MSBuild
{
    public class StandardOutputVisitor : MSBuildVisitor
    {
        private readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages;
        private readonly static Regex stackFrameRegex = GetStackFrameRegex();
        private readonly bool verbose;

        private string assemblyFileName;

        public StandardOutputVisitor(TaskLoggingHelper log,
                                     XElement assemblyElement,
                                     bool verbose,
                                     Func<bool> cancelThunk,
                                     ConcurrentDictionary<string, ExecutionSummary> completionMessages = null)
            : base(log, assemblyElement, cancelThunk)
        {
            this.completionMessages = completionMessages;
            this.verbose = verbose;
        }

        private static Tuple<string, int> GetStackFrameInfo(IFailureInformation failureInfo)
        {
            var stackTraces = ExceptionUtility.CombineStackTraces(failureInfo);
            if (stackTraces != null)
            {
                foreach (var frame in stackTraces.Split(new[] { Environment.NewLine }, 2, StringSplitOptions.RemoveEmptyEntries))
                {
                    var match = stackFrameRegex.Match(frame);
                    if (match.Success)
                        return Tuple.Create(match.Groups["file"].Value, Int32.Parse(match.Groups["line"].Value));
                }
            }

            return Tuple.Create((string)null, 0);
        }

        private static Regex GetStackFrameRegex()
        {
            // Stack trace lines look like this:
            // "   at BooleanAssertsTests.False.AssertFalse() in c:\Dev\xunit\xunit\test\test.xunit.assert\Asserts\BooleanAssertsTests.cs:line 22"

            var wordAt = "at";
            var wordsInLine = "in {0}:line {1}";
            var getResourceStringMethod = typeof(Environment).GetMethod("GetResourceString", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
            if (getResourceStringMethod != null)
            {
                wordAt = (string)getResourceStringMethod.Invoke(null, new object[] { "Word_At" });
                wordsInLine = (string)getResourceStringMethod.Invoke(null, new object[] { "StackTrace_InFileLineNumber" });
            }
            wordsInLine = wordsInLine.Replace("{0}", "(?<file>.*)").Replace("{1}", "(?<line>\\d+)");

            return new Regex(String.Format("{0} .* {1}", wordAt, wordsInLine));
        }

        protected override bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            assemblyFileName = Path.GetFileName(assemblyStarting.TestAssembly.Assembly.AssemblyPath);
            Log.LogMessage(MessageImportance.High, "  Started:  {0}", assemblyFileName);

            return base.Visit(assemblyStarting);
        }

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            // Base class does computation of results, so call it first.
            var result = base.Visit(assemblyFinished);

            Log.LogMessage(MessageImportance.High, "  Finished: {0}", assemblyFileName);

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
            var stackFrameInfo = GetStackFrameInfo(error);

            Log.LogError(null, null, null, stackFrameInfo.Item1, stackFrameInfo.Item2, 0, 0, 0, "{0}", Escape(ExceptionUtility.CombineMessages(error)));
            Log.LogError(null, null, null, stackFrameInfo.Item1, stackFrameInfo.Item2, 0, 0, 0, "{0}", ExceptionUtility.CombineStackTraces(error));

            return base.Visit(error);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            var stackFrameInfo = GetStackFrameInfo(testFailed);
            Log.LogError(null, null, null, stackFrameInfo.Item1, stackFrameInfo.Item2, 0, 0, 0, "{0}: {1}", Escape(testFailed.TestDisplayName), Escape(ExceptionUtility.CombineMessages(testFailed)));

            var combinedStackTrace = ExceptionUtility.CombineStackTraces(testFailed);
            if (!String.IsNullOrWhiteSpace(combinedStackTrace))
                Log.LogError(null, null, null, stackFrameInfo.Item1, stackFrameInfo.Item2, 0, 0, 0, "{0}", combinedStackTrace);

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