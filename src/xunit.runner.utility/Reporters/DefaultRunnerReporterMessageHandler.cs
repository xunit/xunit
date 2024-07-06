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
    /// Default implementation of <see cref="IMessageSink"/> used to report
    /// messages for test runners.
    /// </summary>
    [Obsolete("This class has poor performance; please use DefaultRunnerReporterWithTypesMessageHandler instead.")]
    public class DefaultRunnerReporterMessageHandler : TestMessageVisitor
    {
        readonly string defaultDirectory = null;
        readonly ITestFrameworkExecutionOptions defaultExecutionOptions = TestFrameworkOptions.ForExecution();
        readonly Dictionary<string, ITestFrameworkExecutionOptions> executionOptionsByAssembly = new Dictionary<string, ITestFrameworkExecutionOptions>(StringComparer.OrdinalIgnoreCase);
        readonly bool logPassingTestsWithOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRunnerReporterMessageHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger used to report messages</param>
        public DefaultRunnerReporterMessageHandler(IRunnerLogger logger)
        {
#if NETFRAMEWORK
            defaultDirectory = Directory.GetCurrentDirectory();
#endif

            logPassingTestsWithOutput = string.IsNullOrEmpty(EnvironmentHelper.GetEnvironmentVariable(DefaultRunnerReporterWithTypesMessageHandler.EnvVar_HidePassingOutput));

            Logger = logger;
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
        {
            return Path.GetFileNameWithoutExtension(assemblyMessage.TestAssembly.Assembly.AssemblyPath);
        }

        /// <summary>
        /// Gets the display name of a test assembly from a test assembly message.
        /// </summary>
        /// <param name="assembly">The test assembly</param>
        /// <returns>The assembly display name</returns>
        protected virtual string GetAssemblyDisplayName(XunitProjectAssembly assembly)
        {
            return Path.GetFileNameWithoutExtension(assembly.AssemblyFilename);
        }

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

        /// <inheritdoc/>
        protected override bool Visit(IErrorMessage error)
        {
            LogError("FATAL ERROR", error);

            return base.Visit(error);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestAssemblyDiscoveryFinished discoveryFinished)
        {
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

            return base.Visit(discoveryFinished);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestAssemblyDiscoveryStarting discoveryStarting)
        {
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

            return base.Visit(discoveryStarting);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestAssemblyExecutionFinished executionFinished)
        {
            Logger.LogImportantMessage("  Finished:    {0}", GetAssemblyDisplayName(executionFinished.Assembly));

            RemoveExecutionOptions(executionFinished.Assembly.AssemblyFilename);

            return base.Visit(executionFinished);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestAssemblyExecutionStarting executionStarting)
        {
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

            return base.Visit(executionStarting);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestAssemblyCleanupFailure cleanupFailure)
        {
            LogError(string.Format(CultureInfo.CurrentCulture, "Test Assembly Cleanup Failure ({0})", cleanupFailure.TestAssembly.Assembly.AssemblyPath), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestCaseCleanupFailure cleanupFailure)
        {
            LogError(string.Format(CultureInfo.CurrentCulture, "Test Case Cleanup Failure ({0})", cleanupFailure.TestCase.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestClassCleanupFailure cleanupFailure)
        {
            LogError(string.Format(CultureInfo.CurrentCulture, "Test Class Cleanup Failure ({0})", cleanupFailure.TestClass.Class.Name), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestCleanupFailure cleanupFailure)
        {
            LogError(string.Format(CultureInfo.CurrentCulture, "Test Cleanup Failure ({0})", cleanupFailure.Test.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestCollectionCleanupFailure cleanupFailure)
        {
            LogError(string.Format(CultureInfo.CurrentCulture, "Test Collection Cleanup Failure ({0})", cleanupFailure.TestCollection.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestExecutionSummary executionSummary)
        {
            WriteDefaultSummary(Logger, executionSummary);

            return base.Visit(executionSummary);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestFailed testFailed)
        {
            var frameInfo = StackFrameInfo.FromFailure(testFailed);

            lock (Logger.LockObject)
            {
                Logger.LogError(frameInfo, "    {0} [FAIL]", Escape(testFailed.Test.DisplayName));

                foreach (var messageLine in ExceptionUtility.CombineMessages(testFailed).Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    Logger.LogImportantMessage(frameInfo, "      {0}", messageLine);

                LogStackTrace(frameInfo, ExceptionUtility.CombineStackTraces(testFailed));
                LogOutput(frameInfo, testFailed.Output);
            }

            return base.Visit(testFailed);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestMethodCleanupFailure cleanupFailure)
        {
            LogError(string.Format(CultureInfo.CurrentCulture, "Test Method Cleanup Failure ({0})", cleanupFailure.TestMethod.Method.Name), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestOutput testCaseOutput)
        {
            if (GetExecutionOptions(testCaseOutput.TestAssembly.Assembly.AssemblyPath).GetShowLiveOutputOrDefault())
                Logger.LogMessage("    {0} [OUTPUT] {1}", Escape(testCaseOutput.Test.DisplayName), Escape(testCaseOutput.Output.TrimEnd()));

            return base.Visit(testCaseOutput);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestPassed testPassed)
        {
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

            return base.Visit(testPassed);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestSkipped testSkipped)
        {
            lock (Logger.LockObject)
            {
                Logger.LogWarning("    {0} [SKIP]", Escape(testSkipped.Test.DisplayName));
                Logger.LogImportantMessage("      {0}", Escape(testSkipped.Reason));
            }

            return base.Visit(testSkipped);
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
