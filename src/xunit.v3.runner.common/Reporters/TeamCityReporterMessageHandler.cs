using System;
using System.Text;
using Xunit.Abstractions;

namespace Xunit.Runner.Common
{
    /// <summary>
    /// An implementation of <see cref="IMessageSink" /> and <see cref="IMessageSinkWithTypes" /> that
    /// supports <see cref="TeamCityReporter" />.
    /// </summary>
    public class TeamCityReporterMessageHandler : FlowMappedTestMessageSink
    {
        readonly TeamCityDisplayNameFormatter displayNameFormatter;
        readonly IRunnerLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCityReporterMessageHandler" /> class.
        /// </summary>
        /// <param name="logger">The logger used to report messages</param>
        /// <param name="flowIdMapper">Optional code which maps a test collection name to a flow ID
        /// (the default behavior generates a new GUID for each test collection)</param>
        /// <param name="displayNameFormatter">Optional display name formatter</param>
        public TeamCityReporterMessageHandler(
            IRunnerLogger logger,
            Func<string, string>? flowIdMapper = null,
            TeamCityDisplayNameFormatter? displayNameFormatter = null)
                : base(flowIdMapper)
        {
            Guard.ArgumentNotNull(nameof(logger), logger);

            this.logger = logger;
            this.displayNameFormatter = displayNameFormatter ?? new TeamCityDisplayNameFormatter();

            Diagnostics.ErrorMessageEvent += HandleErrorMessage;

            Execution.TestAssemblyCleanupFailureEvent += HandleTestAssemblyCleanupFailure;
            Execution.TestCaseCleanupFailureEvent += HandleTestCaseCleanupFailure;
            Execution.TestClassCleanupFailureEvent += HandleTestCaseCleanupFailure;
            Execution.TestCollectionCleanupFailureEvent += HandleTestCollectionCleanupFailure;
            Execution.TestCollectionFinishedEvent += HandleTestCollectionFinished;
            Execution.TestCollectionStartingEvent += HandleTestCollectionStarting;
            Execution.TestCleanupFailureEvent += HandleTestCleanupFailure;
            Execution.TestFailedEvent += HandleTestFailed;
            Execution.TestMethodCleanupFailureEvent += HandleTestMethodCleanupFailure;
            Execution.TestPassedEvent += HandleTestPassed;
            Execution.TestSkippedEvent += HandleTestSkipped;
            Execution.TestStartingEvent += HandleTestStarting;
        }

        /// <summary>
        /// Handles instances of <see cref="IErrorMessage" />.
        /// </summary>
        protected virtual void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
        {
            Guard.ArgumentNotNull(nameof(args), args);

            var error = args.Message;
            LogError("FATAL ERROR", error);
        }

        /// <summary>
        /// Handles instances of <see cref="ITestAssemblyCleanupFailure" />.
        /// </summary>
        protected virtual void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
        {
            Guard.ArgumentNotNull(nameof(args), args);

            var cleanupFailure = args.Message;
            LogError($"Test Assembly Cleanup Failure ({cleanupFailure.TestAssembly.Assembly.AssemblyPath})", cleanupFailure);
        }

        /// <summary>
        /// Handles instances of <see cref="ITestCaseCleanupFailure" />.
        /// </summary>
        protected virtual void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
        {
            Guard.ArgumentNotNull(nameof(args), args);

            var cleanupFailure = args.Message;
            LogError($"Test Case Cleanup Failure ({cleanupFailure.TestCase.DisplayName})", cleanupFailure);
        }

        /// <summary>
        /// Handles instances of <see cref="ITestClassCleanupFailure" />.
        /// </summary>
        protected virtual void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args)
        {
            Guard.ArgumentNotNull(nameof(args), args);

            var cleanupFailure = args.Message;
            LogError($"Test Class Cleanup Failure ({cleanupFailure.TestClass.Class.Name})", cleanupFailure);
        }

        /// <summary>
        /// Handles instances of <see cref="ITestCollectionCleanupFailure" />.
        /// </summary>
        protected virtual void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
        {
            Guard.ArgumentNotNull(nameof(args), args);

            var cleanupFailure = args.Message;
            LogError($"Test Collection Cleanup Failure ({cleanupFailure.TestCollection.DisplayName})", cleanupFailure);
        }

        /// <summary>
        /// Handles instances of <see cref="ITestCollectionFinished" />.
        /// </summary>
        protected virtual void HandleTestCollectionFinished(MessageHandlerArgs<ITestCollectionFinished> args)
        {
            Guard.ArgumentNotNull(nameof(args), args);

            var testCollectionFinished = args.Message;
            logger.LogImportantMessage($"##teamcity[testSuiteFinished name='{Escape(displayNameFormatter.DisplayName(testCollectionFinished.TestCollection))}' flowId='{ToFlowId(testCollectionFinished.TestCollection.DisplayName)}']");
        }

        /// <summary>
        /// Handles instances of <see cref="ITestCollectionStarting" />.
        /// </summary>
        protected virtual void HandleTestCollectionStarting(MessageHandlerArgs<ITestCollectionStarting> args)
        {
            Guard.ArgumentNotNull(nameof(args), args);

            var testCollectionStarting = args.Message;
            logger.LogImportantMessage($"##teamcity[testSuiteStarted name='{Escape(displayNameFormatter.DisplayName(testCollectionStarting.TestCollection))}' flowId='{ToFlowId(testCollectionStarting.TestCollection.DisplayName)}']");
        }

        /// <summary>
        /// Handles instances of <see cref="ITestCleanupFailure" />.
        /// </summary>
        protected virtual void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
        {
            Guard.ArgumentNotNull(nameof(args), args);

            var cleanupFailure = args.Message;
            LogError($"Test Cleanup Failure ({cleanupFailure.Test.DisplayName})", cleanupFailure);
        }

        /// <summary>
        /// Handles instances of <see cref="ITestFailed" />.
        /// </summary>
        protected virtual void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            Guard.ArgumentNotNull(nameof(args), args);

            var testFailed = args.Message;
            logger.LogImportantMessage($"##teamcity[testFailed name='{Escape(displayNameFormatter.DisplayName(testFailed.Test))}' details='{Escape(ExceptionUtility.CombineMessages(testFailed))}|r|n{Escape(ExceptionUtility.CombineStackTraces(testFailed))}' flowId='{ToFlowId(testFailed.TestCollection.DisplayName)}']");
            LogFinish(testFailed);
        }

        /// <summary>
        /// Handles instances of <see cref="ITestMethodCleanupFailure" />.
        /// </summary>
        protected virtual void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
        {
            Guard.ArgumentNotNull(nameof(args), args);

            var cleanupFailure = args.Message;
            LogError($"Test Method Cleanup Failure ({cleanupFailure.TestMethod.Method.Name})", cleanupFailure);
        }

        /// <summary>
        /// Handles instances of <see cref="ITestPassed" />.
        /// </summary>
        protected virtual void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
        {
            Guard.ArgumentNotNull(nameof(args), args);

            var testPassed = args.Message;
            LogFinish(testPassed);
        }

        /// <summary>
        /// Handles instances of <see cref="ITestSkipped" />.
        /// </summary>
        protected virtual void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            Guard.ArgumentNotNull(nameof(args), args);

            var testSkipped = args.Message;
            logger.LogImportantMessage($"##teamcity[testIgnored name='{Escape(displayNameFormatter.DisplayName(testSkipped.Test))}' message='{Escape(testSkipped.Reason)}' flowId='{ToFlowId(testSkipped.TestCollection.DisplayName)}']");
            LogFinish(testSkipped);
        }

        /// <summary>
        /// Handles instances of <see cref="ITestStarting" />.
        /// </summary>
        protected virtual void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
        {
            Guard.ArgumentNotNull(nameof(args), args);

            var testStarting = args.Message;
            logger.LogImportantMessage($"##teamcity[testStarted name='{Escape(displayNameFormatter.DisplayName(testStarting.Test))}' flowId='{ToFlowId(testStarting.TestCollection.DisplayName)}']");
        }

        // Helpers

        void LogError(string messageType, IFailureInformation failureInfo)
        {
            var message = $"[{messageType}] {failureInfo.ExceptionTypes[0]}: {ExceptionUtility.CombineMessages(failureInfo)}";
            var stack = ExceptionUtility.CombineStackTraces(failureInfo);

            logger.LogImportantMessage($"##teamcity[message text='{Escape(message)}' errorDetails='{Escape(stack)}' status='ERROR']");
        }

        void LogFinish(ITestResultMessage testResult)
        {
            var formattedName = Escape(displayNameFormatter.DisplayName(testResult.Test));

            if (!string.IsNullOrWhiteSpace(testResult.Output))
                logger.LogImportantMessage($"##teamcity[testStdOut name='{formattedName}' out='{Escape(testResult.Output)}']");

            logger.LogImportantMessage($"##teamcity[testFinished name='{formattedName}' duration='{(int)(testResult.ExecutionTime * 1000M)}' flowId='{ToFlowId(testResult.TestCollection.DisplayName)}']");
        }

        static bool IsAscii(char ch) => ch <= '\x007f';

        static string? Escape(string? value)
        {
            if (value == null)
                return null;

            var sb = new StringBuilder(value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];

                switch (ch)
                {
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
                        if (IsAscii(ch))
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
    }
}
