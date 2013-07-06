using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit.Abstractions;

namespace Xunit.Runner.MSBuild
{
    public class TeamCityVisitor : MSBuildVisitor
    {
        string assemblyFileName;

        public TeamCityVisitor(TaskLoggingHelper log, XElement assembliesElement, Func<bool> cancelThunk, string assemblyFileName)
            : base(log, assembliesElement, cancelThunk)
        {
            this.assemblyFileName = Path.GetFullPath(assemblyFileName);

            FlowId = Guid.NewGuid().ToString("n");
        }

        public string FlowId { get; set; }

        private void LogFinish(ITestResultMessage testResult)
        {
            Log.LogMessage(MessageImportance.High, "##teamcity[testFinished name='{0}' duration='{1}' flowId='{2}']",
                           TeamCityEscape(testResult.TestDisplayName),
                           (int)(testResult.ExecutionTime * 1000M),
                           FlowId);
        }

        protected override bool Visit(IErrorMessage error)
        {
            Log.LogError("{0}: {1}", error.ExceptionType, Escape(error.Message));
            Log.LogError(error.StackTrace);

            return base.Visit(error);
        }

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            // Base class does computation of results, so call it first.
            var result = base.Visit(assemblyFinished);

            Log.LogMessage(MessageImportance.High, "##teamcity[testSuiteFinished name='{0}' flowId='{1}']",
                           TeamCityEscape(assemblyFileName),
                           FlowId);

            return result;
        }

        protected override bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            Log.LogMessage(MessageImportance.High, "##teamcity[testSuiteStarted name='{0}' flowId='{1}']",
                           TeamCityEscape(assemblyFileName),
                           FlowId);

            return base.Visit(assemblyStarting);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            Log.LogMessage(MessageImportance.High, "##teamcity[testFailed name='{0}' details='{1}|r|n{2}' flowId='{3}']",
                           TeamCityEscape(testFailed.TestDisplayName),
                           TeamCityEscape(testFailed.Message),
                           TeamCityEscape(testFailed.StackTrace),
                           FlowId);
            LogFinish(testFailed);

            return base.Visit(testFailed);
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            LogFinish(testPassed);

            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            Log.LogMessage(MessageImportance.High, "##teamcity[testIgnored name='{0}' message='{1}' flowId='{2}']",
                           TeamCityEscape(testSkipped.TestDisplayName),
                           TeamCityEscape(testSkipped.Reason),
                           FlowId);
            LogFinish(testSkipped);

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            Log.LogMessage(MessageImportance.High, "##teamcity[testStarted name='{0}' flowId='{1}']",
                           TeamCityEscape(testStarting.TestDisplayName),
                           FlowId);

            return base.Visit(testStarting);
        }

        static string TeamCityEscape(string value)
        {
            if (value == null)
                return String.Empty;

            return value.Replace("|", "||")
                        .Replace("'", "|'")
                        .Replace("\r", "|r")
                        .Replace("\n", "|n")
                        .Replace("]", "|]");
        }
    }
}