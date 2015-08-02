using System;
using System.Collections.Concurrent;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit.Abstractions;

namespace Xunit.Runner.MSBuild
{
    public class TeamCityVisitor : MSBuildVisitor
    {
        readonly TeamCityDisplayNameFormatter displayNameFormatter;
        readonly ConcurrentDictionary<string, string> flowMappings = new ConcurrentDictionary<string, string>();
        readonly Func<string, string> flowIdMapper;

        public TeamCityVisitor(TaskLoggingHelper log,
                               XElement assemblyElement,
                               Func<bool> cancelThunk,
                               Func<string, string> flowIdMapper = null,
                               TeamCityDisplayNameFormatter displayNameFormatter = null)
            : base(log, assemblyElement, cancelThunk)
        {
            this.flowIdMapper = flowIdMapper ?? (_ => Guid.NewGuid().ToString("N"));
            this.displayNameFormatter = displayNameFormatter ?? new TeamCityDisplayNameFormatter();
        }

        void LogFinish(ITestResultMessage testResult)
        {
            var formattedName = TeamCityEscape(displayNameFormatter.DisplayName(testResult.Test));

            if (!string.IsNullOrWhiteSpace(testResult.Output))
                Log.LogMessage(MessageImportance.High, "##teamcity[testStdOut name='{0}' out='{1}']", formattedName, TeamCityEscape(testResult.Output));

            Log.LogMessage(MessageImportance.High, "##teamcity[testFinished name='{0}' duration='{1}' flowId='{2}']",
                           formattedName,
                           (int)(testResult.ExecutionTime * 1000M),
                           ToFlowId(testResult.TestCollection.DisplayName));
        }

        protected override bool Visit(ITestCollectionFinished testCollectionFinished)
        {
            // Base class does computation of results, so call it first.
            var result = base.Visit(testCollectionFinished);

            Log.LogMessage(MessageImportance.High, "##teamcity[testSuiteFinished name='{0}' flowId='{1}']",
                           TeamCityEscape(displayNameFormatter.DisplayName(testCollectionFinished.TestCollection)),
                           ToFlowId(testCollectionFinished.TestCollection.DisplayName));

            return result;
        }

        protected override bool Visit(ITestCollectionStarting testCollectionStarting)
        {
            Log.LogMessage(MessageImportance.High, "##teamcity[testSuiteStarted name='{0}' flowId='{1}']",
                           TeamCityEscape(displayNameFormatter.DisplayName(testCollectionStarting.TestCollection)),
                           ToFlowId(testCollectionStarting.TestCollection.DisplayName));

            return base.Visit(testCollectionStarting);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            Log.LogMessage(MessageImportance.High, "##teamcity[testFailed name='{0}' details='{1}|r|n{2}' flowId='{3}']",
                           TeamCityEscape(displayNameFormatter.DisplayName(testFailed.Test)),
                           TeamCityEscape(ExceptionUtility.CombineMessages(testFailed)),
                           TeamCityEscape(ExceptionUtility.CombineStackTraces(testFailed)),
                           ToFlowId(testFailed.TestCollection.DisplayName));
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
                           TeamCityEscape(displayNameFormatter.DisplayName(testSkipped.Test)),
                           TeamCityEscape(testSkipped.Reason),
                           ToFlowId(testSkipped.TestCollection.DisplayName));
            LogFinish(testSkipped);

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            Log.LogMessage(MessageImportance.High, "##teamcity[testStarted name='{0}' flowId='{1}']",
                           TeamCityEscape(displayNameFormatter.DisplayName(testStarting.Test)),
                           ToFlowId(testStarting.TestCollection.DisplayName));

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

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            var result = base.Visit(assemblyFinished);

            Log.LogMessage(MessageImportance.High, "##teamcity[testSuiteFinished name='{0}' flowId='{1}']",
                           TeamCityEscape(assemblyFinished.TestAssembly.Assembly.Name),
                           ToFlowId(assemblyFinished.TestAssembly.Assembly.Name));

            return result;
        }

        protected override bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            Log.LogMessage(MessageImportance.High, "##teamcity[testSuiteStarted name='{0}' flowId='{1}']",
                           TeamCityEscape(assemblyStarting.TestAssembly.Assembly.Name),
                           ToFlowId(assemblyStarting.TestAssembly.Assembly.Name));

            return base.Visit(assemblyStarting);
        }

        void WriteError(string messageType, IFailureInformation failureInfo)
        {
            var message = $"[{messageType}] {failureInfo.ExceptionTypes[0]}: {ExceptionUtility.CombineMessages(failureInfo)}";
            var stack = ExceptionUtility.CombineStackTraces(failureInfo);

            Log.LogMessage(MessageImportance.High, "##teamcity[message text='{0}' errorDetails='{1}' status='ERROR']", TeamCityEscape(message), TeamCityEscape(stack));
        }

        static string TeamCityEscape(string value)
        {
            if (value == null)
                return string.Empty;

            return value.Replace("|", "||")
                        .Replace("'", "|'")
                        .Replace("\r", "|r")
                        .Replace("\n", "|n")
                        .Replace("]", "|]")
                        .Replace("[", "|[")
                        .Replace("\u0085", "|x")
                        .Replace("\u2028", "|l")
                        .Replace("\u2029", "|p");
        }

        string ToFlowId(string testCollectionName)
        {
            return flowMappings.GetOrAdd(testCollectionName, flowIdMapper);
        }
    }
}