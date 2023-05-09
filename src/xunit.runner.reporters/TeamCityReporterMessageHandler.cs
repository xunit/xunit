using System;
using System.Text;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class TeamCityReporterMessageHandler : DefaultRunnerReporterWithTypesMessageHandler
    {
        readonly string rootFlowId;

        public TeamCityReporterMessageHandler(IRunnerLogger logger, string rootFlowId)
            : base(logger)
        {
            this.rootFlowId = rootFlowId;

            Execution.TestAssemblyFinishedEvent += HandleTestAssemblyFinished;
            Execution.TestAssemblyStartingEvent += HandleTestAssemblyStarting;
            Execution.TestCollectionFinishedEvent += HandleTestCollectionFinished;
            Execution.TestCollectionStartingEvent += HandleTestCollectionStarting;
            Execution.TestFinishedEvent += HandleTestFinished;
            Execution.TestStartingEvent += HandleTestStarting;
        }

        /// <summary>
        /// Gets the current date &amp; time in UTC.
        /// </summary>
        protected virtual DateTimeOffset UtcNow =>
            DateTimeOffset.UtcNow;

        protected override void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
        {
            base.HandleErrorMessage(args);

            var error = args.Message;

            LogError("FATAL ERROR", error);
        }

        protected override void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
        {
            base.HandleTestAssemblyCleanupFailure(args);

            var cleanupFailure = args.Message;

            LogError($"Test Assembly Cleanup Failure ({ToAssemblyName(cleanupFailure)})", cleanupFailure, ToAssemblyFlowId(cleanupFailure));
        }

        protected virtual void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
        {
            var assemblyFinished = args.Message;

            LogSuiteFinished(ToAssemblyName(assemblyFinished), ToAssemblyFlowId(assemblyFinished));
        }

        protected virtual void HandleTestAssemblyStarting(MessageHandlerArgs<ITestAssemblyStarting> args)
        {
            var assemblyStarting = args.Message;

            LogSuiteStarted(ToAssemblyName(assemblyStarting), ToAssemblyFlowId(assemblyStarting), rootFlowId);
        }

        protected override void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
        {
            base.HandleTestCaseCleanupFailure(args);

            var cleanupFailure = args.Message;

            LogError($"Test Case Cleanup Failure ({ToTestCaseName(cleanupFailure)})", cleanupFailure, ToCollectionFlowId(cleanupFailure));
        }

        protected override void HandleTestClassCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args)
        {
            base.HandleTestClassCleanupFailure(args);

            var cleanupFailure = args.Message;

            LogError($"Test Class Cleanup Failure ({ToTestClassName(cleanupFailure)})", cleanupFailure, ToCollectionFlowId(cleanupFailure));
        }

        protected override void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
        {
            base.HandleTestCollectionCleanupFailure(args);

            var cleanupFailure = args.Message;

            LogError($"Test Collection Cleanup Failure ({ToTestCollectionName(cleanupFailure)})", cleanupFailure, ToCollectionFlowId(cleanupFailure));
        }

        protected virtual void HandleTestCollectionFinished(MessageHandlerArgs<ITestCollectionFinished> args)
        {
            var testCollectionFinished = args.Message;

            LogSuiteFinished(ToTestCollectionName(testCollectionFinished), ToCollectionFlowId(testCollectionFinished));
        }

        protected virtual void HandleTestCollectionStarting(MessageHandlerArgs<ITestCollectionStarting> args)
        {
            var testCollectionStarting = args.Message;

            LogSuiteStarted(ToTestCollectionName(testCollectionStarting), ToCollectionFlowId(testCollectionStarting), ToAssemblyFlowId(testCollectionStarting));
        }

        protected override void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
        {
            base.HandleTestCleanupFailure(args);

            var cleanupFailure = args.Message;

            LogError($"Test Cleanup Failure ({ToTestName(cleanupFailure)})", cleanupFailure, ToCollectionFlowId(cleanupFailure));
        }

        protected override void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            base.HandleTestFailed(args);

            var testFailed = args.Message;
            var details = $"{TeamCityEscape(ExceptionUtility.CombineMessages(testFailed))}|r|n{TeamCityEscape(ExceptionUtility.CombineStackTraces(testFailed))}";

            LogMessage("testFailed", $"name='{TeamCityEscape(ToTestName(testFailed))}' details='{details}'", ToCollectionFlowId(testFailed));
        }

        protected virtual void HandleTestFinished(MessageHandlerArgs<ITestFinished> args)
        {
            var testFinished = args.Message;

            var formattedName = TeamCityEscape(ToTestName(testFinished));
            var flowId = ToCollectionFlowId(testFinished);

            if (!string.IsNullOrWhiteSpace(testFinished.Output))
                LogMessage("testStdOut", $"name='{formattedName}' out='{TeamCityEscape(testFinished.Output)}' tc:tags='tc:parseServiceMessagesInside']", flowId);

            LogMessage("testFinished", $"name='{formattedName}' duration='{(int)(testFinished.ExecutionTime * 1000M)}'", flowId);
        }

        protected override void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
        {
            base.HandleTestMethodCleanupFailure(args);

            var cleanupFailure = args.Message;

            LogError($"Test Method Cleanup Failure ({ToTestMethodName(cleanupFailure)})", cleanupFailure, ToCollectionFlowId(cleanupFailure));
        }

        protected override void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            base.HandleTestSkipped(args);

            var testSkipped = args.Message;

            LogMessage("testIgnored", $"name='{TeamCityEscape(ToTestName(testSkipped))}' message='{TeamCityEscape(testSkipped.Reason)}'", ToCollectionFlowId(testSkipped));
        }

        protected virtual void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
        {
            var testStarting = args.Message;

            LogMessage("testStarted", $"name='{TeamCityEscape(ToTestName(testStarting))}'", ToCollectionFlowId(testStarting));
        }

        // Helpers

        void LogError(
            string messageType,
            IFailureInformation failure,
            string flowId = null)
        {
            var message = $"[{messageType}] {failure.ExceptionTypes[0]}: {ExceptionUtility.CombineMessages(failure)}";
            var stackTrace = ExceptionUtility.CombineStackTraces(failure);

            LogMessage("message", $"status='ERROR' text='{TeamCityEscape(message)}' errorDetails='{TeamCityEscape(stackTrace)}'", flowId);
        }

        void LogMessage(
            string messageType,
            string arguments = null,
            string flowId = null) =>
                Logger.LogRaw($"##teamcity[{messageType} timestamp='{TeamCityEscape(UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff"))}+0000'{(flowId != null ? $" flowId='{TeamCityEscape(flowId)}'" : "")}{(arguments != null ? " " + arguments : "")}]");

        void LogSuiteFinished(
            string name,
            string flowId)
        {
            LogMessage("testSuiteFinished", $"name='{TeamCityEscape(name)}'", flowId);
            LogMessage("flowFinished", flowId: flowId);
        }

        void LogSuiteStarted(
            string escapedName,
            string flowId,
            string parentFlowId = null)
        {
            LogMessage("flowStarted", parentFlowId != null ? $"parent='{TeamCityEscape(parentFlowId)}'" : null, flowId);
            LogMessage("testSuiteStarted", $"name='{TeamCityEscape(escapedName)}'", flowId);
        }

        static string TeamCityEscape(string value)
        {
            if (value == null)
                return null;

            var sb = new StringBuilder(value.Length);

            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];

                switch (ch)
                {
                    case '\\':
                        sb.Append("|0x005C");
                        break;
                    case '|':
                        sb.Append("||");
                        break;
                    case '\'':
                        sb.Append("|'");
                        break;
                    case '\n':
                        sb.Append("|n");
                        break;
                    case '\r':
                        sb.Append("|r");
                        break;
                    case '[':
                        sb.Append("|[");
                        break;
                    case ']':
                        sb.Append("|]");
                        break;
                    default:
                        if (ch < '\x007f')
                            sb.Append(ch);
                        else
                        {
                            sb.Append("|0x");
                            sb.Append(((int)ch).ToString("x4"));
                        }
                        break;
                }
            }

            return sb.ToString();
        }

        string ToAssemblyFlowId(ITestAssemblyMessage message) =>
            ToAssemblyName(message);

        string ToAssemblyName(ITestAssemblyMessage message) =>
            message.TestAssembly.Assembly.AssemblyPath ?? message.TestAssembly.Assembly.SimpleAssemblyName();

        string ToTestCaseName(ITestCaseMessage message) =>
            message.TestCase.DisplayName;

        string ToTestClassName(ITestClassMessage message) =>
            message.TestClass.Class.Name;

        string ToCollectionFlowId(ITestCollectionMessage message) =>
            message.TestCollection.UniqueID.ToString("N");

        string ToTestCollectionName(ITestCollectionMessage message) =>
            $"{message.TestCollection.DisplayName} ({ToCollectionFlowId(message)})";

        string ToTestMethodName(ITestMethodMessage message) =>
            $"{message.TestMethod.Method.Type.Name}.{message.TestMethod.Method.Name}";

        string ToTestName(ITestMessage message) =>
            message.Test.DisplayName;
    }
}
