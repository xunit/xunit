using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Default implementation of <see cref="IMessageSinkWithTypes"/> used to report
    /// messages for test runners.
    /// </summary>
    public class DefaultRunnerReporterWithTypesMessageHandler : TestMessageSink
    {
        /// <summary>
        /// Gets the environment variable that's used to hide passing tests with output
        /// when diagnostics messages are enabled.
        /// </summary>
        public const string EnvVar_HidePassingOutput = "XUNIT_HIDE_PASSING_OUTPUT_DIAGNOSTICS";

        readonly string defaultDirectory = null;
        readonly ITestFrameworkExecutionOptions defaultExecutionOptions = TestFrameworkOptions.ForExecution();
        readonly Dictionary<string, ITestFrameworkExecutionOptions> executionOptionsByAssembly = new Dictionary<string, ITestFrameworkExecutionOptions>(StringComparer.OrdinalIgnoreCase);
        readonly bool logPassingTestsWithOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRunnerReporterWithTypesMessageHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger used to report messages</param>
        public DefaultRunnerReporterWithTypesMessageHandler(IRunnerLogger logger)
        {
#if NETFRAMEWORK
            defaultDirectory = Directory.GetCurrentDirectory();
#endif

            Logger = logger;

            logPassingTestsWithOutput = string.IsNullOrEmpty(EnvironmentHelper.GetEnvironmentVariable(EnvVar_HidePassingOutput));

            Diagnostics.ErrorMessageEvent += HandleErrorMessage;

            Execution.TestAssemblyCleanupFailureEvent += HandleTestAssemblyCleanupFailure;
            Execution.TestClassCleanupFailureEvent += HandleTestClassCleanupFailure;
            Execution.TestCaseCleanupFailureEvent += HandleTestCaseCleanupFailure;
            Execution.TestCollectionCleanupFailureEvent += HandleTestCollectionCleanupFailure;
            Execution.TestCleanupFailureEvent += HandleTestCleanupFailure;
            Execution.TestFailedEvent += HandleTestFailed;
            Execution.TestMethodCleanupFailureEvent += HandleTestMethodCleanupFailure;
            Execution.TestOutputEvent += HandleTestOutput;
            Execution.TestPassedEvent += HandleTestPassed;
            Execution.TestSkippedEvent += HandleTestSkipped;

            Runner.TestAssemblyDiscoveryFinishedEvent += HandleTestAssemblyDiscoveryFinished;
            Runner.TestAssemblyDiscoveryStartingEvent += HandleTestAssemblyDiscoveryStarting;
            Runner.TestAssemblyExecutionFinishedEvent += HandleTestAssemblyExecutionFinished;
            Runner.TestAssemblyExecutionStartingEvent += HandleTestAssemblyExecutionStarting;
            Runner.TestExecutionSummaryEvent += HandleTestExecutionSummary;
        }

        /// <summary>
        /// Get the logger used to report messages.
        /// </summary>
        protected IRunnerLogger Logger { get; private set; }

        void AddExecutionOptions(string assemblyFilename, ITestFrameworkExecutionOptions executionOptions)
        {
            using (ReaderWriterLockWrapper.WriteLock())
                executionOptionsByAssembly[Path.GetFileNameWithoutExtension(assemblyFilename)] = executionOptions;
        }

        /// <summary>
        /// Escapes text for display purposes.
        /// </summary>
        /// <param name="text">The text to be escaped</param>
        /// <returns>The escaped text</returns>
        protected virtual string Escape(string text)
        {
            if (text == null)
                return string.Empty;

            return text.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\0", "\\0");
        }

        /// <summary>
        /// Gets the display name of a test assembly from a test assembly message.
        /// </summary>
        /// <param name="assemblyMessage">The test assembly message</param>
        /// <returns>The assembly display name</returns>
        protected virtual string GetAssemblyDisplayName(ITestAssemblyMessage assemblyMessage)
            => Path.GetFileNameWithoutExtension(assemblyMessage.TestAssembly.Assembly.AssemblyPath);

        /// <summary>
        /// Gets the display name of a test assembly from a test assembly message.
        /// </summary>
        /// <param name="assembly">The test assembly</param>
        /// <returns>The assembly display name</returns>
        protected virtual string GetAssemblyDisplayName(XunitProjectAssembly assembly)
            => Path.GetFileNameWithoutExtension(assembly.AssemblyFilename);

        /// <summary>
        /// Get the test framework options for the given assembly. If it cannot find them, then it
        /// returns a default set of options.
        /// </summary>
        /// <param name="assemblyFilename">The test assembly filename</param>
        /// <returns></returns>
        protected ITestFrameworkExecutionOptions GetExecutionOptions(string assemblyFilename)
        {
            ITestFrameworkExecutionOptions result;

            using (ReaderWriterLockWrapper.ReadLock())
                if (!executionOptionsByAssembly.TryGetValue(Path.GetFileNameWithoutExtension(assemblyFilename), out result))
                    result = defaultExecutionOptions;

            return result;
        }

        /// <summary>
        /// Logs an error message to the logger.
        /// </summary>
        /// <param name="failureType">The type of the failure</param>
        /// <param name="failureInfo">The failure information</param>
        protected void LogError(string failureType, IFailureInformation failureInfo)
        {
            var frameInfo = StackFrameInfo.FromFailure(failureInfo);

            lock (Logger.LockObject)
            {
                Logger.LogError(frameInfo, "    [{0}] {1}", failureType, Escape(failureInfo.ExceptionTypes.FirstOrDefault() ?? "(Unknown Exception Type)"));

                foreach (var messageLine in ExceptionUtility.CombineMessages(failureInfo).Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    Logger.LogImportantMessage(frameInfo, "      {0}", messageLine);

                LogStackTrace(frameInfo, ExceptionUtility.CombineStackTraces(failureInfo));
            }
        }

        /// <summary>
        /// Logs a stack trace to the logger.
        /// </summary>
        protected virtual void LogStackTrace(StackFrameInfo frameInfo, string stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
                return;

            Logger.LogMessage(frameInfo, "      Stack Trace:");

            foreach (var stackFrame in stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                Logger.LogImportantMessage(frameInfo, "        {0}", StackFrameTransformer.TransformFrame(stackFrame, defaultDirectory));
        }

        /// <summary>
        /// Lots test output to the logger.
        /// </summary>
        protected virtual void LogOutput(StackFrameInfo frameInfo, string output)
        {
            if (string.IsNullOrEmpty(output))
                return;

            // ITestOutputHelper terminates everything with NewLine, but we really don't need that
            // extra blank line in our output.
            if (output.EndsWith(Environment.NewLine, StringComparison.Ordinal))
                output = output.Substring(0, output.Length - Environment.NewLine.Length);

            Logger.LogMessage(frameInfo, "      Output:");

            foreach (var line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                Logger.LogImportantMessage(frameInfo, "        {0}", line);
        }

        void RemoveExecutionOptions(string assemblyFilename)
        {
            using (ReaderWriterLockWrapper.WriteLock())
                executionOptionsByAssembly.Remove(Path.GetFileNameWithoutExtension(assemblyFilename));
        }

        /// <summary>
        /// Called when <see cref="IErrorMessage"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
            => LogError("FATAL ERROR", args.Message);

        /// <summary>
        /// Called when <see cref="ITestAssemblyDiscoveryFinished"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestAssemblyDiscoveryFinished(MessageHandlerArgs<ITestAssemblyDiscoveryFinished> args)
        {
            var discoveryFinished = args.Message;
            var assemblyDisplayName = GetAssemblyDisplayName(discoveryFinished.Assembly);

            if (discoveryFinished.DiscoveryOptions.GetDiagnosticMessagesOrDefault())
            {
                var count =
                    discoveryFinished.TestCasesToRun == discoveryFinished.TestCasesDiscovered
                        ? discoveryFinished.TestCasesDiscovered.ToString(CultureInfo.CurrentCulture)
                        : string.Format(CultureInfo.CurrentCulture, "{0} of {1}", discoveryFinished.TestCasesToRun, discoveryFinished.TestCasesDiscovered);

                Logger.LogImportantMessage("  Discovered:  {0} (found {1} test case{2})", assemblyDisplayName, count, discoveryFinished.TestCasesToRun == 1 ? "" : "s");
            }
            else
                Logger.LogImportantMessage("  Discovered:  {0}", assemblyDisplayName);
        }

        /// <summary>
        /// Called when <see cref="ITestAssemblyDiscoveryStarting"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestAssemblyDiscoveryStarting(MessageHandlerArgs<ITestAssemblyDiscoveryStarting> args)
        {
            var discoveryStarting = args.Message;
            var assemblyDisplayName = GetAssemblyDisplayName(discoveryStarting.Assembly);

            if (discoveryStarting.DiscoveryOptions.GetDiagnosticMessagesOrDefault())
            {
#if NETFRAMEWORK
                Logger.LogImportantMessage(
                    "  Discovering: {0} (app domain = {1}, method display = {2}, method display options = {3})",
                    assemblyDisplayName,
                    discoveryStarting.AppDomain ? string.Format(CultureInfo.CurrentCulture, "on [{0}shadow copy]", discoveryStarting.ShadowCopy ? "" : "no ") : "off",
                    discoveryStarting.DiscoveryOptions.GetMethodDisplayOrDefault(),
                    discoveryStarting.DiscoveryOptions.GetMethodDisplayOptionsOrDefault()
                );
#else
                Logger.LogImportantMessage(
                    "  Discovering: {0} (method display = {1}, method display options = {2})",
                    assemblyDisplayName,
                    discoveryStarting.DiscoveryOptions.GetMethodDisplayOrDefault(),
                    discoveryStarting.DiscoveryOptions.GetMethodDisplayOptionsOrDefault()
                );
#endif
            }
            else
                Logger.LogImportantMessage("  Discovering: {0}", assemblyDisplayName);
        }

        /// <summary>
        /// Called when <see cref="ITestAssemblyExecutionFinished"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestAssemblyExecutionFinished(MessageHandlerArgs<ITestAssemblyExecutionFinished> args)
        {
            var executionFinished = args.Message;
            Logger.LogImportantMessage("  Finished:    {0}", GetAssemblyDisplayName(executionFinished.Assembly));

            RemoveExecutionOptions(executionFinished.Assembly.AssemblyFilename);
        }

        /// <summary>
        /// Called when <see cref="ITestAssemblyExecutionStarting"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestAssemblyExecutionStarting(MessageHandlerArgs<ITestAssemblyExecutionStarting> args)
        {
            var executionStarting = args.Message;
            AddExecutionOptions(executionStarting.Assembly.AssemblyFilename, executionStarting.ExecutionOptions);

            var assemblyDisplayName = GetAssemblyDisplayName(executionStarting.Assembly);

            if (executionStarting.ExecutionOptions.GetDiagnosticMessagesOrDefault())
            {
                var threadCount = executionStarting.ExecutionOptions.GetMaxParallelThreadsOrDefault();
                var parallelAlgorithm = executionStarting.ExecutionOptions.GetParallelAlgorithmOrDefault();
                var parallelTestCollections =
                    executionStarting.ExecutionOptions.GetDisableParallelizationOrDefault()
                        ? "off"
                        : string.Format(
                            CultureInfo.CurrentCulture,
                            "on [{0} thread{1}{2}]",
                            threadCount < 0 ? "unlimited" : threadCount.ToString(CultureInfo.CurrentCulture),
                            threadCount == 1 ? string.Empty : "s",
                            threadCount > 0 && parallelAlgorithm == ParallelAlgorithm.Aggressive ? "/aggressive" : string.Empty
                        );

                Logger.LogImportantMessage(
                    "  Starting:    {0} (parallel test collections = {1}, stop on fail = {2})",
                    assemblyDisplayName,
                    parallelTestCollections,
                    executionStarting.ExecutionOptions.GetStopOnTestFailOrDefault() ? "on" : "off"
                );
            }
            else
                Logger.LogImportantMessage("  Starting:    {0}", assemblyDisplayName);
        }

        /// <summary>
        /// Called when <see cref="ITestAssemblyCleanupFailure"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
            => LogError(string.Format(CultureInfo.CurrentCulture, "Test Assembly Cleanup Failure ({0})", args.Message.TestAssembly.Assembly.AssemblyPath), args.Message);

        /// <summary>
        /// Called when <see cref="ITestCaseCleanupFailure"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
            => LogError(string.Format(CultureInfo.CurrentCulture, "Test Case Cleanup Failure ({0})", args.Message.TestCase.DisplayName), args.Message);

        /// <summary>
        /// Called when <see cref="ITestClassCleanupFailure"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestClassCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args)
            => LogError(string.Format(CultureInfo.CurrentCulture, "Test Class Cleanup Failure ({0})", args.Message.TestClass.Class.Name), args.Message);

        /// <summary>
        /// Called when <see cref="ITestCleanupFailure"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
            => LogError(string.Format(CultureInfo.CurrentCulture, "Test Cleanup Failure ({0})", args.Message.Test.DisplayName), args.Message);

        /// <summary>
        /// Called when <see cref="ITestCollectionCleanupFailure"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
            => LogError(string.Format(CultureInfo.CurrentCulture, "Test Collection Cleanup Failure ({0})", args.Message.TestCollection.DisplayName), args.Message);

        /// <summary>
        /// Called when <see cref="ITestExecutionSummary"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestExecutionSummary(MessageHandlerArgs<ITestExecutionSummary> args)
            => WriteDefaultSummary(Logger, args.Message);

        /// <summary>
        /// Called when <see cref="ITestFailed"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            var testFailed = args.Message;
            var frameInfo = StackFrameInfo.FromFailure(testFailed);

            lock (Logger.LockObject)
            {
                Logger.LogError(frameInfo, "    {0} [FAIL]", Escape(testFailed.Test.DisplayName));

                foreach (var messageLine in ExceptionUtility.CombineMessages(testFailed).Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    Logger.LogImportantMessage(frameInfo, "      {0}", messageLine);

                LogStackTrace(frameInfo, ExceptionUtility.CombineStackTraces(testFailed));
                LogOutput(frameInfo, testFailed.Output);
            }
        }

        /// <summary>
        /// Called when <see cref="ITestMethodCleanupFailure"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
            => LogError(string.Format(CultureInfo.CurrentCulture, "Test Method Cleanup Failure ({0})", args.Message.TestMethod.Method.Name), args.Message);

        /// <summary>
        /// Called when <see cref="ITestOutput"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestOutput(MessageHandlerArgs<ITestOutput> args)
        {
            var testOutput = args.Message;
            if (GetExecutionOptions(testOutput.TestAssembly.Assembly.AssemblyPath).GetShowLiveOutputOrDefault())
                Logger.LogMessage("    {0} [OUTPUT] {1}", Escape(testOutput.Test.DisplayName), Escape(testOutput.Output.TrimEnd()));
        }

        /// <summary>
        /// Called when <see cref="ITestPassed"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
        {
            var testPassed = args.Message;

            if (logPassingTestsWithOutput &&
                !string.IsNullOrEmpty(testPassed.Output) &&
                GetExecutionOptions(testPassed.TestAssembly.Assembly.AssemblyPath).GetDiagnosticMessagesOrDefault())
            {
                lock (Logger.LockObject)
                {
                    Logger.LogImportantMessage("    {0} [PASS]", Escape(testPassed.Test.DisplayName));
                    LogOutput(StackFrameInfo.None, testPassed.Output);
                }
            }
        }

        /// <summary>
        /// Called when <see cref="ITestSkipped"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            lock (Logger.LockObject)
            {
                var testSkipped = args.Message;
                Logger.LogWarning("    {0} [SKIP]", Escape(testSkipped.Test.DisplayName));
                Logger.LogImportantMessage("      {0}", Escape(testSkipped.Reason));
            }
        }

        /// <summary>
        /// Writes the default summary to the given logger. Can be used by other reporters who also wish to write the
        /// standard summary information.
        /// </summary>
        /// <param name="logger">The logger used to send result messages to.</param>
        /// <param name="executionSummary">The execution summary to display.</param>
        public static void WriteDefaultSummary(IRunnerLogger logger, ITestExecutionSummary executionSummary)
        {
            logger.LogImportantMessage("=== TEST EXECUTION SUMMARY ===");

            var longestAssemblyName = executionSummary.Summaries.Max(summary => summary.Key.Length);
            var allTotal = executionSummary.Summaries.Sum(summary => summary.Value.Total).ToString(CultureInfo.CurrentCulture);
            var allErrors = executionSummary.Summaries.Sum(summary => summary.Value.Errors).ToString(CultureInfo.CurrentCulture);
            var allFailed = executionSummary.Summaries.Sum(summary => summary.Value.Failed).ToString(CultureInfo.CurrentCulture);
            var allSkipped = executionSummary.Summaries.Sum(summary => summary.Value.Skipped).ToString(CultureInfo.CurrentCulture);
            var allTime = executionSummary.Summaries.Sum(summary => summary.Value.Time).ToString("0.000s", CultureInfo.CurrentCulture);

            foreach (var summary in executionSummary.Summaries)
            {
                if (summary.Value.Total == 0)
                    logger.LogImportantMessage("   {0}  Total: {1}", summary.Key.PadRight(longestAssemblyName), "0".PadLeft(allTotal.Length));
                else
                {
                    var total = summary.Value.Total.ToString(CultureInfo.CurrentCulture).PadLeft(allTotal.Length);
                    var errors = summary.Value.Errors.ToString(CultureInfo.CurrentCulture).PadLeft(allErrors.Length);
                    var failed = summary.Value.Failed.ToString(CultureInfo.CurrentCulture).PadLeft(allFailed.Length);
                    var skipped = summary.Value.Skipped.ToString(CultureInfo.CurrentCulture).PadLeft(allSkipped.Length);
                    var time = summary.Value.Time.ToString("0.000s", CultureInfo.CurrentCulture).PadLeft(allTime.Length);

                    logger.LogImportantMessage(
                        "   {0}  Total: {1}, Errors: {2}, Failed: {3}, Skipped: {4}, Time: {5}",
                        summary.Key.PadRight(longestAssemblyName),
                        total,
                        errors,
                        failed,
                        skipped,
                        time
                    );
                }
            }

            if (executionSummary.Summaries.Count > 1)
            {
                logger.LogImportantMessage(
                    "   {0}         {1}          {2}          {3}           {4}        {5}",
                    " ".PadRight(longestAssemblyName),
                    "-".PadRight(allTotal.Length, '-'),
                    "-".PadRight(allErrors.Length, '-'),
                    "-".PadRight(allFailed.Length, '-'),
                    "-".PadRight(allSkipped.Length, '-'),
                    "-".PadRight(allTime.Length, '-')
                );
                logger.LogImportantMessage(
                    "   {0} {1}          {2}          {3}           {4}        {5} ({6:0.000s})",
                    "GRAND TOTAL:".PadLeft(longestAssemblyName + 8),
                    allTotal,
                    allErrors,
                    allFailed,
                    allSkipped,
                    allTime,
                    executionSummary.ElapsedClockTime.TotalSeconds
                );
            }
        }

        class ReaderWriterLockWrapper : IDisposable
        {
            static readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();
            static readonly ReaderWriterLockWrapper lockForRead = new ReaderWriterLockWrapper(@lock.ExitReadLock);
            static readonly ReaderWriterLockWrapper lockForWrite = new ReaderWriterLockWrapper(@lock.ExitWriteLock);

            readonly Action unlock;

            ReaderWriterLockWrapper(Action unlock)
            {
                this.unlock = unlock;
            }

            public void Dispose()
            {
                unlock();
            }

            public static IDisposable ReadLock()
            {
                @lock.EnterReadLock();
                return lockForRead;
            }

            public static IDisposable WriteLock()
            {
                @lock.EnterWriteLock();
                return lockForWrite;
            }
        }
    }
}
