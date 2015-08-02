using System;
using System.Collections.Concurrent;
using System.IO;
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
        readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages;
        readonly static Regex stackFrameRegex = GetStackFrameRegex();
        readonly bool verbose;

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

        static Tuple<string, int> GetStackFrameInfo(IFailureInformation failureInfo)
        {
            var stackTraces = ExceptionUtility.CombineStackTraces(failureInfo);
            if (stackTraces != null)
            {
                foreach (var frame in stackTraces.Split(new[] { Environment.NewLine }, 2, StringSplitOptions.RemoveEmptyEntries))
                {
                    var match = stackFrameRegex.Match(frame);
                    if (match.Success)
                        return Tuple.Create(match.Groups["file"].Value, int.Parse(match.Groups["line"].Value));
                }
            }

            return Tuple.Create((string)null, 0);
        }

        static Regex GetStackFrameRegex()
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

            return new Regex($"{wordAt} .* {wordsInLine}");
        }

        protected override bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            var assemblyDisplayName = Path.GetFileNameWithoutExtension(assemblyStarting.TestAssembly.Assembly.AssemblyPath);
            Log.LogMessage(MessageImportance.High, "  Starting:    {0}", assemblyDisplayName);

            return base.Visit(assemblyStarting);
        }

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            // Base class does computation of results, so call it first.
            var result = base.Visit(assemblyFinished);
            var assemblyDisplayName = Path.GetFileNameWithoutExtension(assemblyFinished.TestAssembly.Assembly.AssemblyPath);

            Log.LogMessage(MessageImportance.High, "  Finished:    {0}", assemblyDisplayName);

            if (completionMessages != null)
                completionMessages.TryAdd(assemblyDisplayName, new ExecutionSummary
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
            var stackFrameInfo = GetStackFrameInfo(testFailed);
            Log.LogError(null, null, null, stackFrameInfo.Item1, stackFrameInfo.Item2, 0, 0, 0, "{0}: {1}", Escape(testFailed.Test.DisplayName), Escape(ExceptionUtility.CombineMessages(testFailed)));

            var combinedStackTrace = ExceptionUtility.CombineStackTraces(testFailed);
            if (!string.IsNullOrWhiteSpace(combinedStackTrace))
                Log.LogError(null, null, null, stackFrameInfo.Item1, stackFrameInfo.Item2, 0, 0, 0, "{0}", combinedStackTrace);

            return base.Visit(testFailed);
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            if (verbose)
                Log.LogMessage("    PASS:  {0}", Escape(testPassed.Test.DisplayName));
            else
                Log.LogMessage("    {0}", Escape(testPassed.Test.DisplayName));

            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            Log.LogWarning("{0}: {1}", Escape(testSkipped.Test.DisplayName), Escape(testSkipped.Reason));

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            if (verbose)
                Log.LogMessage("    START: {0}", Escape(testStarting.Test.DisplayName));

            return base.Visit(testStarting);
        }

        protected override bool Visit(IErrorMessage error)
        {
            WriteError("FATAL", error);

            return base.Visit(error);
        }

        protected override bool Visit(ITestAssemblyCleanupFailure cleanupFailure)
        {
            WriteError($"Test Assembly Cleanup Failure ({cleanupFailure.TestAssembly.Assembly.AssemblyPath})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCaseCleanupFailure cleanupFailure)
        {
            WriteError($"Test Case Cleanup Failure ({cleanupFailure.TestCase.DisplayName})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestClassCleanupFailure cleanupFailure)
        {
            WriteError($"Test Class Cleanup Failure ({cleanupFailure.TestClass.Class.Name})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCollectionCleanupFailure cleanupFailure)
        {
            WriteError($"Test Collection Cleanup Failure ({cleanupFailure.TestCollection.DisplayName})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCleanupFailure cleanupFailure)
        {
            WriteError($"Test Cleanup Failure ({cleanupFailure.Test.DisplayName})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestMethodCleanupFailure cleanupFailure)
        {
            WriteError($"Test Method Cleanup Failure ({cleanupFailure.TestMethod.Method.Name})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        void WriteError(string failureName, IFailureInformation failureInfo)
        {
            var stackFrameInfo = GetStackFrameInfo(failureInfo);

            Log.LogError(null, null, null, stackFrameInfo.Item1, stackFrameInfo.Item2, 0, 0, 0, "[{0}] {1}", failureName, Escape(ExceptionUtility.CombineMessages(failureInfo)));
            Log.LogError(null, null, null, stackFrameInfo.Item1, stackFrameInfo.Item2, 0, 0, 0, "{0}", ExceptionUtility.CombineStackTraces(failureInfo));
        }
    }
}