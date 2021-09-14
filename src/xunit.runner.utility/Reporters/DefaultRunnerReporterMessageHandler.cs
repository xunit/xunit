using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRunnerReporterMessageHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger used to report messages</param>
        public DefaultRunnerReporterMessageHandler(IRunnerLogger logger)
        {
#if NETFRAMEWORK
            defaultDirectory = Directory.GetCurrentDirectory();
#endif

            Logger = logger;
        }

        /// <summary>
        /// Get the logger used to report messages.
        /// </summary>
        protected IRunnerLogger Logger { get; private set; }

        void AddExecutionOptions(string assemblyFilename, ITestFrameworkExecutionOptions executionOptions)
        {
            using (ReaderWriterLockWrapper.WriteLock())
                executionOptionsByAssembly[assemblyFilename] = executionOptions;
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
                if (!executionOptionsByAssembly.TryGetValue(assemblyFilename, out result))
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
                Logger.LogError(frameInfo, $"    [{failureType}] {Escape(failureInfo.ExceptionTypes.FirstOrDefault() ?? "(Unknown Exception Type)")}");

                foreach (var messageLine in ExceptionUtility.CombineMessages(failureInfo).Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    Logger.LogImportantMessage(frameInfo, $"      {messageLine}");

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
                Logger.LogImportantMessage(frameInfo, $"        {StackFrameTransformer.TransformFrame(stackFrame, defaultDirectory)}");
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
                Logger.LogImportantMessage(frameInfo, $"        {line}");
        }

        void RemoveExecutionOptions(string assemblyFilename)
        {
            using (ReaderWriterLockWrapper.WriteLock())
                executionOptionsByAssembly.Remove(assemblyFilename);
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
                        ? discoveryFinished.TestCasesDiscovered.ToString()
                        : $"{discoveryFinished.TestCasesToRun} of {discoveryFinished.TestCasesDiscovered}";

                Logger.LogImportantMessage($"  Discovered:  {assemblyDisplayName} (found {count} test case{(discoveryFinished.TestCasesToRun == 1 ? "" : "s")})");
            }
            else
                Logger.LogImportantMessage($"  Discovered:  {assemblyDisplayName}");

            return base.Visit(discoveryFinished);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestAssemblyDiscoveryStarting discoveryStarting)
        {
            var assemblyDisplayName = GetAssemblyDisplayName(discoveryStarting.Assembly);

            if (discoveryStarting.DiscoveryOptions.GetDiagnosticMessagesOrDefault())
            {
#if NETFRAMEWORK
                Logger.LogImportantMessage($"  Discovering: {assemblyDisplayName} (app domain = {(discoveryStarting.AppDomain ? $"on [{(discoveryStarting.ShadowCopy ? "shadow copy" : "no shadow copy")}]" : "off")}, method display = {discoveryStarting.DiscoveryOptions.GetMethodDisplayOrDefault()}, method display options = {discoveryStarting.DiscoveryOptions.GetMethodDisplayOptionsOrDefault()})");
#else
                Logger.LogImportantMessage($"  Discovering: {assemblyDisplayName} (method display = {discoveryStarting.DiscoveryOptions.GetMethodDisplayOrDefault()}, method display options = {discoveryStarting.DiscoveryOptions.GetMethodDisplayOptionsOrDefault()})");
#endif
            }
            else
                Logger.LogImportantMessage($"  Discovering: {assemblyDisplayName}");

            return base.Visit(discoveryStarting);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestAssemblyExecutionFinished executionFinished)
        {
            Logger.LogImportantMessage($"  Finished:    {GetAssemblyDisplayName(executionFinished.Assembly)}");

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
                var threadCountText = threadCount < 0 ? "unlimited" : threadCount.ToString();
                Logger.LogImportantMessage($"  Starting:    {assemblyDisplayName} (parallel test collections = {(!executionStarting.ExecutionOptions.GetDisableParallelizationOrDefault() ? "on" : "off")}, max threads = {threadCountText})");
            }
            else
                Logger.LogImportantMessage($"  Starting:    {assemblyDisplayName}");

            return base.Visit(executionStarting);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestAssemblyCleanupFailure cleanupFailure)
        {
            LogError($"Test Assembly Cleanup Failure ({cleanupFailure.TestAssembly.Assembly.AssemblyPath})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestCaseCleanupFailure cleanupFailure)
        {
            LogError($"Test Case Cleanup Failure ({cleanupFailure.TestCase.DisplayName})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestClassCleanupFailure cleanupFailure)
        {
            LogError($"Test Class Cleanup Failure ({cleanupFailure.TestClass.Class.Name})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestCleanupFailure cleanupFailure)
        {
            LogError($"Test Cleanup Failure ({cleanupFailure.Test.DisplayName})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestCollectionCleanupFailure cleanupFailure)
        {
            LogError($"Test Collection Cleanup Failure ({cleanupFailure.TestCollection.DisplayName})", cleanupFailure);

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
                Logger.LogError(frameInfo, $"    {Escape(testFailed.Test.DisplayName)} [FAIL]");

                foreach (var messageLine in ExceptionUtility.CombineMessages(testFailed).Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    Logger.LogImportantMessage(frameInfo, $"      {messageLine}");

                LogStackTrace(frameInfo, ExceptionUtility.CombineStackTraces(testFailed));
                LogOutput(frameInfo, testFailed.Output);
            }

            return base.Visit(testFailed);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestMethodCleanupFailure cleanupFailure)
        {
            LogError($"Test Method Cleanup Failure ({cleanupFailure.TestMethod.Method.Name})", cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        /// <inheritdoc/>
        protected override bool Visit(ITestPassed testPassed)
        {
            if (!string.IsNullOrEmpty(testPassed.Output) &&
                GetExecutionOptions(testPassed.TestAssembly.Assembly.AssemblyPath).GetDiagnosticMessagesOrDefault())
            {
                lock (Logger.LockObject)
                {
                    Logger.LogImportantMessage($"    {Escape(testPassed.Test.DisplayName)} [PASS]");
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
                Logger.LogWarning($"    {Escape(testSkipped.Test.DisplayName)} [SKIP]");
                Logger.LogImportantMessage($"      {Escape(testSkipped.Reason)}");
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

            var totalTestsRun = executionSummary.Summaries.Sum(summary => summary.Value.Total);
            var totalTestsFailed = executionSummary.Summaries.Sum(summary => summary.Value.Failed);
            var totalTestsSkipped = executionSummary.Summaries.Sum(summary => summary.Value.Skipped);
            var totalTime = executionSummary.Summaries.Sum(summary => summary.Value.Time).ToString("0.000s");
            var totalErrors = executionSummary.Summaries.Sum(summary => summary.Value.Errors);
            var longestAssemblyName = executionSummary.Summaries.Max(summary => summary.Key.Length);
            var longestTotal = totalTestsRun.ToString().Length;
            var longestFailed = totalTestsFailed.ToString().Length;
            var longestSkipped = totalTestsSkipped.ToString().Length;
            var longestTime = totalTime.Length;
            var longestErrors = totalErrors.ToString().Length;

            foreach (var summary in executionSummary.Summaries)
            {
                if (summary.Value.Total == 0)
                    logger.LogImportantMessage($"   {summary.Key.PadRight(longestAssemblyName)}  Total: {"0".PadLeft(longestTotal)}");
                else
                    logger.LogImportantMessage($"   {summary.Key.PadRight(longestAssemblyName)}  Total: {summary.Value.Total.ToString().PadLeft(longestTotal)}, Errors: {summary.Value.Errors.ToString().PadLeft(longestErrors)}, Failed: {summary.Value.Failed.ToString().PadLeft(longestFailed)}, Skipped: {summary.Value.Skipped.ToString().PadLeft(longestSkipped)}, Time: {summary.Value.Time.ToString("0.000s").PadLeft(longestTime)}");

            }

            if (executionSummary.Summaries.Count > 1)
            {
                logger.LogImportantMessage($"   {" ".PadRight(longestAssemblyName)}         {"-".PadRight(longestTotal, '-')}          {"-".PadRight(longestErrors, '-')}          {"-".PadRight(longestFailed, '-')}           {"-".PadRight(longestSkipped, '-')}        {"-".PadRight(longestTime, '-')}");
                logger.LogImportantMessage($"   {"GRAND TOTAL:".PadLeft(longestAssemblyName + 8)} {totalTestsRun}          {totalErrors}          {totalTestsFailed}           {totalTestsSkipped}        {totalTime} ({executionSummary.ElapsedClockTime.TotalSeconds.ToString("0.000s")})");
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
