using System;
using System.Globalization;
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

            TeamCityLogError(error, "FATAL ERROR");
        }

        protected override void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
        {
            base.HandleTestAssemblyCleanupFailure(args);

            var cleanupFailure = args.Message;

            TeamCityLogError(ToAssemblyFlowId(cleanupFailure), cleanupFailure, "Test Assembly Cleanup Failure ({0})", ToAssemblyName(cleanupFailure));
        }

        protected virtual void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
        {
            var assemblyFinished = args.Message;

            TeamCityLogSuiteFinished(ToAssemblyFlowId(assemblyFinished), ToAssemblyName(assemblyFinished));
        }

        protected virtual void HandleTestAssemblyStarting(MessageHandlerArgs<ITestAssemblyStarting> args)
        {
            var assemblyStarting = args.Message;

            TeamCityLogSuiteStarted(ToAssemblyFlowId(assemblyStarting), ToAssemblyName(assemblyStarting), rootFlowId);
        }

        protected override void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
        {
            base.HandleTestCaseCleanupFailure(args);

            var cleanupFailure = args.Message;

            TeamCityLogError(ToCollectionFlowId(cleanupFailure), cleanupFailure, "Test Case Cleanup Failure ({0})", ToTestCaseName(cleanupFailure));
        }

        protected override void HandleTestClassCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args)
        {
            base.HandleTestClassCleanupFailure(args);

            var cleanupFailure = args.Message;

            TeamCityLogError(ToCollectionFlowId(cleanupFailure), cleanupFailure, "Test Class Cleanup Failure ({0})", ToTestClassName(cleanupFailure));
        }

        protected override void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
        {
            base.HandleTestCollectionCleanupFailure(args);

            var cleanupFailure = args.Message;

            TeamCityLogError(ToCollectionFlowId(cleanupFailure), cleanupFailure, "Test Collection Cleanup Failure ({0})", ToTestCollectionName(cleanupFailure));
        }

        protected virtual void HandleTestCollectionFinished(MessageHandlerArgs<ITestCollectionFinished> args)
        {
            var testCollectionFinished = args.Message;

            TeamCityLogSuiteFinished(ToCollectionFlowId(testCollectionFinished), ToTestCollectionName(testCollectionFinished));
        }

        protected virtual void HandleTestCollectionStarting(MessageHandlerArgs<ITestCollectionStarting> args)
        {
            var testCollectionStarting = args.Message;

            TeamCityLogSuiteStarted(ToCollectionFlowId(testCollectionStarting), ToTestCollectionName(testCollectionStarting), ToAssemblyFlowId(testCollectionStarting));
        }

        protected override void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
        {
            base.HandleTestCleanupFailure(args);

            var cleanupFailure = args.Message;

            TeamCityLogError(ToCollectionFlowId(cleanupFailure), cleanupFailure, "Test Cleanup Failure ({0})", ToTestName(cleanupFailure));
        }

        protected override void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            base.HandleTestFailed(args);

            var testFailed = args.Message;

            TeamCityLogMessage(
                ToCollectionFlowId(testFailed),
                "testFailed",
                "name='{0}' details='{1}|r|n{2}'",
                TeamCityEscape(ToTestName(testFailed)),
                TeamCityEscape(ExceptionUtility.CombineMessages(testFailed)),
                TeamCityEscape(ExceptionUtility.CombineStackTraces(testFailed))
            );
        }

        protected virtual void HandleTestFinished(MessageHandlerArgs<ITestFinished> args)
        {
            var testFinished = args.Message;

            var formattedName = TeamCityEscape(ToTestName(testFinished));
            var flowId = ToCollectionFlowId(testFinished);

            if (!string.IsNullOrWhiteSpace(testFinished.Output))
                TeamCityLogMessage(flowId, "testStdOut", "name='{0}' out='{1}' tc:tags='tc:parseServiceMessagesInside']", formattedName, TeamCityEscape(testFinished.Output));

            TeamCityLogMessage(flowId, "testFinished", "name='{0}' duration='{1}'", formattedName, (int)(testFinished.ExecutionTime * 1000M));
        }

        protected override void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
        {
            base.HandleTestMethodCleanupFailure(args);

            var cleanupFailure = args.Message;

            TeamCityLogError(ToCollectionFlowId(cleanupFailure), cleanupFailure, "Test Method Cleanup Failure ({0})", ToTestMethodName(cleanupFailure));
        }

        protected override void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            base.HandleTestSkipped(args);

            var testSkipped = args.Message;

            TeamCityLogMessage(ToCollectionFlowId(testSkipped), "testIgnored", "name='{0}' message='{1}'", TeamCityEscape(ToTestName(testSkipped)), TeamCityEscape(testSkipped.Reason));
        }

        protected virtual void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
        {
            var testStarting = args.Message;

            TeamCityLogMessage(ToCollectionFlowId(testStarting), "testStarted", "name='{0}'", TeamCityEscape(ToTestName(testStarting)));
        }

        // Helpers

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
                            sb.Append(((int)ch).ToString("x4", CultureInfo.CurrentCulture));
                        }
                        break;
                }
            }

            return sb.ToString();
        }

        void TeamCityLogError(IFailureInformation failure, string messageType) =>
            TeamCityLogError(string.Empty, failure, "{0}", messageType);

        void TeamCityLogError(string flowId, IFailureInformation failure, string messageTypeFormat, params object[] args)
        {
            var message = string.Format(
                CultureInfo.InvariantCulture,
                "[{0}] {1}: {2}",
                string.Format(CultureInfo.InvariantCulture, messageTypeFormat, args),
                failure.ExceptionTypes[0],
                ExceptionUtility.CombineMessages(failure)
            );
            var stackTrace = ExceptionUtility.CombineStackTraces(failure);

            TeamCityLogMessage(flowId, "message", "status='ERROR' text='{0}' errorDetails='{1}'", TeamCityEscape(message), TeamCityEscape(stackTrace));
        }

        void TeamCityLogMessage(string flowId, string messageType, string extraMetadataFormat = "", params object[] args) =>
            Logger.LogRaw(
                "##teamcity[{0} timestamp='{1}+0000'{2}{3}]",
                messageType,
                TeamCityEscape(UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff", CultureInfo.InvariantCulture)),
                flowId.Length != 0 ? string.Format(CultureInfo.InvariantCulture, " flowId='{0}'", TeamCityEscape(flowId)) : "",
                extraMetadataFormat.Length != 0 ? " " + string.Format(CultureInfo.InvariantCulture, extraMetadataFormat, args) : ""
            );

        void TeamCityLogSuiteFinished(
            string flowId,
            string name)
        {
            TeamCityLogMessage(flowId, "testSuiteFinished", "name='{0}'", TeamCityEscape(name));
            TeamCityLogMessage(flowId: flowId, messageType: "flowFinished");
        }

        void TeamCityLogSuiteStarted(
            string flowId,
            string escapedName,
            string parentFlowId = null)
        {
            TeamCityLogMessage(flowId, "flowStarted", parentFlowId == null ? "" : "parent='{0}'", TeamCityEscape(parentFlowId));
            TeamCityLogMessage(flowId, "testSuiteStarted", "name='{0}'", TeamCityEscape(escapedName));
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
            string.Format(CultureInfo.InvariantCulture, "{0} ({1})", message.TestCollection.DisplayName, ToCollectionFlowId(message));

        string ToTestMethodName(ITestMethodMessage message) =>
            string.Format(CultureInfo.InvariantCulture, "{0}.{1}", message.TestMethod.Method.Type.Name, message.TestMethod.Method.Name);

        string ToTestName(ITestMessage message) =>
            message.Test.DisplayName;
    }
}
