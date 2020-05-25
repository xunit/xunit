using System;
using System.Collections.Generic;
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
        readonly string defaultDirectory = null;
        readonly ITestFrameworkExecutionOptions defaultExecutionOptions = TestFrameworkOptions.ForExecution();
        readonly Dictionary<string, ITestFrameworkExecutionOptions> executionOptionsByAssembly = new Dictionary<string, ITestFrameworkExecutionOptions>(StringComparer.OrdinalIgnoreCase);

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

            Diagnostics.ErrorMessageEvent += HandleErrorMessage;

            Execution.TestAssemblyCleanupFailureEvent += HandleTestAssemblyCleanupFailure;
            Execution.TestClassCleanupFailureEvent += HandleTestClassCleanupFailure;
            Execution.TestCaseCleanupFailureEvent += HandleTestCaseCleanupFailure;
            Execution.TestCollectionCleanupFailureEvent += HandleTestCollectionCleanupFailure;
            Execution.TestCleanupFailureEvent += HandleTestCleanupFailure;
            Execution.TestFailedEvent += HandleTestFailed;
            Execution.TestMethodCleanupFailureEvent += HandleTestMethodCleanupFailure;
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
                        ? discoveryFinished.TestCasesDiscovered.ToString()
                        : $"{discoveryFinished.TestCasesToRun} of {discoveryFinished.TestCasesDiscovered}";

                Logger.LogImportantMessage($"  Discovered:  {assemblyDisplayName} (found {count} test case{(discoveryFinished.TestCasesToRun == 1 ? "" : "s")})");
            }
            else
                Logger.LogImportantMessage($"  Discovered:  {assemblyDisplayName}");
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
                Logger.LogImportantMessage($"  Discovering: {assemblyDisplayName} (app domain = {(discoveryStarting.AppDomain ? $"on [{(discoveryStarting.ShadowCopy ? "shadow copy" : "no shadow copy")}]" : "off")}, method display = {discoveryStarting.DiscoveryOptions.GetMethodDisplayOrDefault()}, method display options = {discoveryStarting.DiscoveryOptions.GetMethodDisplayOptionsOrDefault()})");
#else
                Logger.LogImportantMessage($"  Discovering: {assemblyDisplayName} (method display = {discoveryStarting.DiscoveryOptions.GetMethodDisplayOrDefault()}, method display options = {discoveryStarting.DiscoveryOptions.GetMethodDisplayOptionsOrDefault()})");
#endif
            }
            else
                Logger.LogImportantMessage($"  Discovering: {assemblyDisplayName}");
        }

        /// <summary>
        /// Called when <see cref="ITestAssemblyExecutionFinished"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestAssemblyExecutionFinished(MessageHandlerArgs<ITestAssemblyExecutionFinished> args)
        {
            var executionFinished = args.Message;
            Logger.LogImportantMessage($"  Finished:    {GetAssemblyDisplayName(executionFinished.Assembly)}");

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
                var threadCountText = threadCount < 0 ? "unlimited" : threadCount.ToString();
                Logger.LogImportantMessage($"  Starting:    {assemblyDisplayName} (parallel test collections = {(!executionStarting.ExecutionOptions.GetDisableParallelizationOrDefault() ? "on" : "off")}, max threads = {threadCountText})");
            }
            else
                Logger.LogImportantMessage($"  Starting:    {assemblyDisplayName}");
        }

        /// <summary>
        /// Called when <see cref="ITestAssemblyCleanupFailure"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
            => LogError($"Test Assembly Cleanup Failure ({args.Message.TestAssembly.Assembly.AssemblyPath})", args.Message);

        /// <summary>
        /// Called when <see cref="ITestCaseCleanupFailure"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
            => LogError($"Test Case Cleanup Failure ({args.Message.TestCase.DisplayName})", args.Message);

        /// <summary>
        /// Called when <see cref="ITestClassCleanupFailure"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestClassCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args)
            => LogError($"Test Class Cleanup Failure ({args.Message.TestClass.Class.Name})", args.Message);

        /// <summary>
        /// Called when <see cref="ITestCleanupFailure"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
            => LogError($"Test Cleanup Failure ({args.Message.Test.DisplayName})", args.Message);

        /// <summary>
        /// Called when <see cref="ITestCollectionCleanupFailure"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
            => LogError($"Test Collection Cleanup Failure ({args.Message.TestCollection.DisplayName})", args.Message);

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
                Logger.LogError(frameInfo, $"    {Escape(testFailed.Test.DisplayName)} [FAIL]");

                foreach (var messageLine in ExceptionUtility.CombineMessages(testFailed).Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    Logger.LogImportantMessage(frameInfo, $"      {messageLine}");

                LogStackTrace(frameInfo, ExceptionUtility.CombineStackTraces(testFailed));
                LogOutput(frameInfo, testFailed.Output);
            }
        }

        /// <summary>
        /// Called when <see cref="ITestMethodCleanupFailure"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
            => LogError($"Test Method Cleanup Failure ({args.Message.TestMethod.Method.Name})", args.Message);

        /// <summary>
        /// Called when <see cref="ITestPassed"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
        {
            var testPassed = args.Message;
            if (!string.IsNullOrEmpty(testPassed.Output) &&
                GetExecutionOptions(testPassed.TestAssembly.Assembly.AssemblyPath).GetDiagnosticMessagesOrDefault())
            {
                lock (Logger.LockObject)
                {
                    Logger.LogImportantMessage($"    {Escape(testPassed.Test.DisplayName)} [PASS]");
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
                Logger.LogWarning($"    {Escape(testSkipped.Test.DisplayName)} [SKIP]");
                Logger.LogImportantMessage($"      {Escape(testSkipped.Reason)}");
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
