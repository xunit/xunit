using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit.Abstractions;

namespace Xunit.Runner.MSBuild
{
    public class TeamCityVisitor : MSBuildVisitor
    {
        string assemblyFileName;

        public TeamCityVisitor(TaskLoggingHelper log, Func<bool> cancelThunk, string assemblyFileName)
            : base(log, cancelThunk)
        {
            this.assemblyFileName = Path.GetFullPath(assemblyFileName);
        }

        private void LogFinish(ITestResultMessage testResult)
        {
            Log.LogMessage(MessageImportance.High, "##teamcity[testFinished name='{0}' duration='{1}']",
                           TeamCityEscape(testResult.TestDisplayName),
                           (int)(testResult.ExecutionTime * 1000M));
        }

        protected override bool Visit(IErrorMessage error)
        {
            Log.LogError("{0}: {1}", error.ExceptionType, Escape(error.Message));
            Log.LogError(error.StackTrace);

            return !CancelThunk();
        }

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            base.Visit(assemblyFinished);

            Log.LogMessage(MessageImportance.High, "##teamcity[testSuiteFinished name='{0}']", TeamCityEscape(assemblyFileName));

            return !CancelThunk();
        }

        protected override bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            Log.LogMessage(MessageImportance.High, "##teamcity[testSuiteStarted name='{0}']", TeamCityEscape(assemblyFileName));

            return !CancelThunk();
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            Log.LogMessage(MessageImportance.High, "##teamcity[testFailed name='{0}' details='{1}|r|n{2}']",
                           TeamCityEscape(testFailed.TestDisplayName),
                           TeamCityEscape(testFailed.Message),
                           TeamCityEscape(testFailed.StackTrace));
            LogFinish(testFailed);

            return !CancelThunk();
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            LogFinish(testPassed);

            return !CancelThunk();
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            Log.LogMessage(MessageImportance.High, "##teamcity[testIgnored name='{0}' message='{1}']",
                           TeamCityEscape(testSkipped.TestDisplayName),
                           TeamCityEscape(testSkipped.Reason));
            LogFinish(testSkipped);

            return !CancelThunk();
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            Log.LogMessage(MessageImportance.High, "##teamcity[testStarted name='{0}']", TeamCityEscape(testStarting.TestDisplayName));

            return !CancelThunk();
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

//        public bool ClassFailed(string className, string exceptionType, string message, string stackTrace)
//        {
//            log.LogMessage(MessageImportance.High, "##teamcity[buildStatus status='FAILURE' text='Class failed: {0}: {1}|r|n{2}']",
//                           Escape(className),
//                           Escape(message),
//                           Escape(stackTrace));
//            return !cancelled();
//        }