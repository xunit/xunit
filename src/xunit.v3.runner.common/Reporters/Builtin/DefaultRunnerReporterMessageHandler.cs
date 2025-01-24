using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerReporterMessageHandler" /> that supports <see cref="DefaultRunnerReporter" />.
/// </summary>
public class DefaultRunnerReporterMessageHandler : TestMessageSink, IRunnerReporterMessageHandler
{
	/// <summary>
	/// Gets the environment variable that's used to hide passing tests with output
	/// when diagnostics messages are enabled.
	/// </summary>
	public const string EnvVar_HidePassingOutput = EnvironmentVariables.HidePassingOutputDiagnostics;

	readonly string? defaultDirectory;
	readonly ITestFrameworkExecutionOptions defaultExecutionOptions = TestFrameworkOptions.Empty();
	readonly Dictionary<string, ITestFrameworkExecutionOptions> executionOptionsByAssembly = new(StringComparer.OrdinalIgnoreCase);
	readonly bool logPassingTestsWithOutput;

	/// <summary>
	/// Initializes a new instance of the <see cref="DefaultRunnerReporterMessageHandler"/> class.
	/// </summary>
	/// <param name="logger">The logger used to report messages</param>
	public DefaultRunnerReporterMessageHandler(IRunnerLogger logger)
	{
		Guard.ArgumentNotNull(logger);

		defaultDirectory = Directory.GetCurrentDirectory();

		Logger = logger;

		logPassingTestsWithOutput = string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EnvVar_HidePassingOutput));

		Diagnostics.ErrorMessageEvent += HandleErrorMessage;

		Execution.TestAssemblyCleanupFailureEvent += HandleTestAssemblyCleanupFailure;
		Execution.TestAssemblyFinishedEvent += HandleTestAssemblyFinished;
		Execution.TestAssemblyStartingEvent += HandleTestAssemblyStarting;

		Execution.TestClassCleanupFailureEvent += HandleTestClassCleanupFailure;
		Execution.TestClassFinishedEvent += HandleTestClassFinished;
		Execution.TestClassStartingEvent += HandleTestClassStarting;

		Execution.TestCaseCleanupFailureEvent += HandleTestCaseCleanupFailure;
		Execution.TestCaseFinishedEvent += HandleTestCaseFinished;
		Execution.TestCaseStartingEvent += HandleTestCaseStarting;

		Execution.TestCollectionCleanupFailureEvent += HandleTestCollectionCleanupFailure;
		Execution.TestCollectionFinishedEvent += HandleTestCollectionFinished;
		Execution.TestCollectionStartingEvent += HandleTestCollectionStarting;

		Execution.TestMethodCleanupFailureEvent += HandleTestMethodCleanupFailure;
		Execution.TestMethodFinishedEvent += HandleTestMethodFinished;
		Execution.TestMethodStartingEvent += HandleTestMethodStarting;

		Execution.TestCleanupFailureEvent += HandleTestCleanupFailure;
		Execution.TestFinishedEvent += HandleTestFinished;
		Execution.TestFailedEvent += HandleTestFailed;
		Execution.TestOutputEvent += HandleTestOutput;
		Execution.TestPassedEvent += HandleTestPassed;
		Execution.TestSkippedEvent += HandleTestSkipped;
		Execution.TestStartingEvent += HandleTestStarting;

		Runner.TestAssemblyDiscoveryFinishedEvent += HandleTestAssemblyDiscoveryFinished;
		Runner.TestAssemblyDiscoveryStartingEvent += HandleTestAssemblyDiscoveryStarting;
		Runner.TestAssemblyExecutionFinishedEvent += HandleTestAssemblyExecutionFinished;
		Runner.TestAssemblyExecutionStartingEvent += HandleTestAssemblyExecutionStarting;
		Runner.TestExecutionSummariesEvent += HandleTestExecutionSummaries;
	}

	/// <summary>
	/// Get the logger used to report messages.
	/// </summary>
	protected IRunnerLogger Logger { get; }

	/// <summary>
	/// Gets the metadata cache.
	/// </summary>
	protected MessageMetadataCache MetadataCache { get; } = new();

	void AddExecutionOptions(
		string? assemblyFileName,
		ITestFrameworkExecutionOptions executionOptions)
	{
		if (assemblyFileName is not null)
			using (ReaderWriterLockWrapper.WriteLock())
				executionOptionsByAssembly[Path.GetFileNameWithoutExtension(assemblyFileName)] = executionOptions;
	}

	/// <summary>
	/// Escapes text for display purposes.
	/// </summary>
	/// <param name="text">The text to be escaped</param>
	/// <returns>The escaped text</returns>
	protected virtual string Escape(string? text) =>
		text is not null
			? text.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\0", "\\0")
			: string.Empty;

	/// <summary>
	/// Escapes multi-line text for display purposes, indenting on newlines.
	/// </summary>
	/// <param name="text">The text to be escaped</param>
	/// <param name="indent">The indent to use for multiple line text</param>
	/// <returns>The escaped text</returns>
	protected virtual string EscapeMultiLineIndent(
		string? text,
		string indent) =>
			text is not null
				? text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine + indent).Replace("\0", "\\0")
				: string.Empty;

	/// <summary>
	/// Gets the display name of a test assembly from a test assembly message.
	/// </summary>
	/// <param name="assembly">The test assembly</param>
	/// <returns>The assembly display name</returns>
	protected virtual string GetAssemblyDisplayName(XunitProjectAssembly assembly)
	{
		Guard.ArgumentNotNull(assembly);

		return assembly.AssemblyDisplayName;
	}

	/// <summary>
	/// Get the test framework options for the given assembly. If it cannot find them, then it
	/// returns a default set of options.
	/// </summary>
	/// <param name="assemblyFileName">The test assembly filename</param>
	protected ITestFrameworkExecutionOptions GetExecutionOptions(string? assemblyFileName)
	{
		if (assemblyFileName is not null)
			using (ReaderWriterLockWrapper.ReadLock())
				if (executionOptionsByAssembly.TryGetValue(Path.GetFileNameWithoutExtension(assemblyFileName), out var result))
					return result;

		return defaultExecutionOptions;
	}

	/// <summary>
	/// Logs an error message to the logger.
	/// </summary>
	/// <param name="errorMetadata">The failure information</param>
	/// <param name="failureType">The type of the failure</param>
	protected void LogError(
		IErrorMetadata errorMetadata,
		string failureType)
	{
		Guard.ArgumentNotNull(failureType);
		Guard.ArgumentNotNull(errorMetadata);

		var frameInfo = StackFrameInfo.FromErrorMetadata(errorMetadata);

		lock (Logger.LockObject)
		{
			Logger.LogError(frameInfo, "    [{0}] {1}", failureType, Escape(errorMetadata.ExceptionTypes.FirstOrDefault() ?? "(Unknown Exception Type)"));

			foreach (var messageLine in ExceptionUtility.CombineMessages(errorMetadata).Split([Environment.NewLine], StringSplitOptions.None))
				Logger.LogImportantMessage(frameInfo, "      " + messageLine);

			LogStackTrace(frameInfo, ExceptionUtility.CombineStackTraces(errorMetadata));
		}
	}

	/// <summary>
	/// Logs an error message to the logger.
	/// </summary>
	/// <param name="errorMetadata">The failure information</param>
	/// <param name="failureTypeFormat">The type of the failure, in message format</param>
	/// <param name="args">The arguments to format <paramref name="failureTypeFormat"/> with</param>
	protected void LogError(
		IErrorMetadata errorMetadata,
		string failureTypeFormat,
		params object?[] args) =>
			LogError(errorMetadata, string.Format(CultureInfo.CurrentCulture, failureTypeFormat, args));


	/// <summary>
	/// Logs a stack trace to the logger.
	/// </summary>
	protected virtual void LogStackTrace(
		StackFrameInfo frameInfo,
		string? stackTrace)
	{
		if (string.IsNullOrEmpty(stackTrace))
			return;

		Logger.LogMessage(frameInfo, "      Stack Trace:");

		foreach (var stackFrame in stackTrace.Split([Environment.NewLine], StringSplitOptions.None))
			Logger.LogImportantMessage(frameInfo, "        " + StackFrameTransformer.TransformFrame(stackFrame, defaultDirectory));
	}

	/// <summary>
	/// Logs test output to the logger.
	/// </summary>
	protected virtual void LogOutput(
		StackFrameInfo frameInfo,
		string? output)
	{
		if (string.IsNullOrEmpty(output))
			return;

		// The test output helper terminates everything with NewLine, but we really don't need that
		// extra blank line in our output.
		if (output.EndsWith(Environment.NewLine, StringComparison.Ordinal))
			output = output.Substring(0, output.Length - Environment.NewLine.Length);

		Logger.LogMessage(frameInfo, "      Output:");

		foreach (var line in output.Split([Environment.NewLine], StringSplitOptions.None))
			Logger.LogImportantMessage(frameInfo, "        " + line);
	}

	/// <summary>
	/// Logs warnings to the logger.
	/// </summary>
	protected virtual void LogWarnings(
		StackFrameInfo frameInfo,
		string[]? warnings)
	{
		if (warnings is null || warnings.Length == 0)
			return;

		Logger.LogMessage(frameInfo, "      Warnings:");

		foreach (var warning in warnings)
		{
			var lines = warning.Split([Environment.NewLine], StringSplitOptions.None);
			for (var idx = 0; idx < lines.Length; ++idx)
				Logger.LogWarning(frameInfo, "        {0} {1}", idx == 0 ? '\u2022' : ' ', lines[idx]);
		}
	}

	void RemoveExecutionOptions(string assemblyIdentifier)
	{
		using (ReaderWriterLockWrapper.WriteLock())
			executionOptionsByAssembly.Remove(assemblyIdentifier);
	}

	/// <summary>
	/// Called when <see cref="IErrorMessage"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
	{
		Guard.ArgumentNotNull(args);

		LogError(args.Message, "FATAL ERROR");
	}

	/// <summary>
	/// Called when <see cref="TestAssemblyDiscoveryFinished"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestAssemblyDiscoveryFinished(MessageHandlerArgs<TestAssemblyDiscoveryFinished> args)
	{
		Guard.ArgumentNotNull(args);

		var discoveryFinished = args.Message;
		var assemblyDisplayName = GetAssemblyDisplayName(discoveryFinished.Assembly);

		if (discoveryFinished.DiscoveryOptions.GetDiagnosticMessagesOrDefault())
			Logger.LogImportantMessage("  Discovered:  {0} ({1} test case{2} to be run)", assemblyDisplayName, discoveryFinished.TestCasesToRun, discoveryFinished.TestCasesToRun == 1 ? "" : "s");
		else
			Logger.LogImportantMessage("  Discovered:  {0}", assemblyDisplayName);
	}

	/// <summary>
	/// Called when <see cref="TestAssemblyDiscoveryStarting"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestAssemblyDiscoveryStarting(MessageHandlerArgs<TestAssemblyDiscoveryStarting> args)
	{
		Guard.ArgumentNotNull(args);

		var discoveryStarting = args.Message;
		var assemblyDisplayName = GetAssemblyDisplayName(discoveryStarting.Assembly);

		if (discoveryStarting.DiscoveryOptions.GetDiagnosticMessagesOrDefault())
		{
			var appDomainText = discoveryStarting.AppDomain switch
			{
				AppDomainOption.Enabled => string.Format(CultureInfo.CurrentCulture, "app domain = on [{0}shadow copy], ", discoveryStarting.ShadowCopy ? "" : "no "),
				AppDomainOption.Disabled => "app domain = off, ",
				_ => "",
			};

			Logger.LogImportantMessage(
				"  Discovering: {0} ({1}method display = {2}, method display options = {3})",
				assemblyDisplayName,
				appDomainText,
				discoveryStarting.DiscoveryOptions.GetMethodDisplayOrDefault(),
				discoveryStarting.DiscoveryOptions.GetMethodDisplayOptionsOrDefault()
			);
		}
		else
			Logger.LogImportantMessage("  Discovering: {0}", assemblyDisplayName);
	}

	/// <summary>
	/// Called when <see cref="TestAssemblyExecutionFinished"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestAssemblyExecutionFinished(MessageHandlerArgs<TestAssemblyExecutionFinished> args)
	{
		Guard.ArgumentNotNull(args);

		var executionFinished = args.Message;
		Logger.LogImportantMessage("  Finished:    {0}", GetAssemblyDisplayName(executionFinished.Assembly));

		RemoveExecutionOptions(executionFinished.Assembly.Identifier);
	}

	/// <summary>
	/// Called when <see cref="TestAssemblyExecutionStarting"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestAssemblyExecutionStarting(MessageHandlerArgs<TestAssemblyExecutionStarting> args)
	{
		Guard.ArgumentNotNull(args);

		var executionStarting = args.Message;
		AddExecutionOptions(executionStarting.Assembly.AssemblyFileName, executionStarting.ExecutionOptions);

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
#pragma warning disable CA1308 // This is converted to lower case for display purposes, not normalization purposes
			var @explicit = executionStarting.ExecutionOptions.GetExplicitOptionOrDefault().ToString().ToLowerInvariant();
#pragma warning restore CA1308
			var culture = executionStarting.ExecutionOptions.GetCulture();
			if (culture?.Length == 0)
				culture = "invariant";

			Logger.LogImportantMessage(
				"  Starting:    {0} (parallel test collections = {1}, stop on fail = {2}, explicit = {3}{4}{5})",
				assemblyDisplayName,
				parallelTestCollections,
				executionStarting.ExecutionOptions.GetStopOnTestFailOrDefault() ? "on" : "off",
				@explicit,
				executionStarting.Seed is null ? "" : string.Format(CultureInfo.CurrentCulture, ", seed = {0}", executionStarting.Seed),
				culture is null ? "" : string.Format(CultureInfo.CurrentCulture, ", culture = {0}", culture)
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
	{
		Guard.ArgumentNotNull(args);

		var metadata = MetadataCache.TryGetAssemblyMetadata(args.Message);

		LogError(args.Message, "Test Assembly Cleanup Failure ({0})", metadata?.AssemblyPath ?? "<unknown test assembly>");
	}

	/// <summary>
	/// Called when <see cref="ITestAssemblyFinished"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args) =>
		// We don't remove this metadata from the cache, because the assembly ID is how we map
		// execution results. We need the cache to still contain that mapping so we can print
		// results at the end of execution.
		Guard.ArgumentNotNull(args);

	/// <summary>
	/// Called when <see cref="ITestAssemblyStarting"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestAssemblyStarting(MessageHandlerArgs<ITestAssemblyStarting> args)
	{
		Guard.ArgumentNotNull(args);

		MetadataCache.Set(args.Message);
	}

	/// <summary>
	/// Called when <see cref="ITestCaseCleanupFailure"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
	{
		Guard.ArgumentNotNull(args);

		var metadata = MetadataCache.TryGetTestCaseMetadata(args.Message);

		LogError(args.Message, "Test Case Cleanup Failure ({0})", metadata?.TestCaseDisplayName ?? "<unknown test case>");
	}

	/// <summary>
	/// Called when <see cref="ITestCaseFinished"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestCaseFinished(MessageHandlerArgs<ITestCaseFinished> args)
	{
		Guard.ArgumentNotNull(args);

		MetadataCache.TryRemove(args.Message);
	}

	/// <summary>
	/// Called when <see cref="ITestCaseStarting"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestCaseStarting(MessageHandlerArgs<ITestCaseStarting> args)
	{
		Guard.ArgumentNotNull(args);

		MetadataCache.Set(args.Message);
	}

	/// <summary>
	/// Called when <see cref="ITestClassCleanupFailure"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestClassCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args)
	{
		Guard.ArgumentNotNull(args);

		var metadata = MetadataCache.TryGetClassMetadata(args.Message);

		LogError(args.Message, "Test Class Cleanup Failure ({0})", metadata?.TestClassName ?? "<unknown test class>");
	}

	/// <summary>
	/// Called when <see cref="ITestClassFinished"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestClassFinished(MessageHandlerArgs<ITestClassFinished> args)
	{
		Guard.ArgumentNotNull(args);

		MetadataCache.TryRemove(args.Message);
	}

	/// <summary>
	/// Called when <see cref="ITestClassStarting"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestClassStarting(MessageHandlerArgs<ITestClassStarting> args)
	{
		Guard.ArgumentNotNull(args);

		MetadataCache.Set(args.Message);
	}

	/// <summary>
	/// Called when <see cref="ITestCleanupFailure"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
	{
		Guard.ArgumentNotNull(args);

		var metadata = MetadataCache.TryGetTestMetadata(args.Message);

		LogError(args.Message, "Test Cleanup Failure ({0})", metadata?.TestDisplayName ?? "<unknown test>");
	}

	/// <summary>
	/// Called when <see cref="ITestCollectionCleanupFailure"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
	{
		Guard.ArgumentNotNull(args);

		var metadata = MetadataCache.TryGetCollectionMetadata(args.Message);

		LogError(args.Message, "Test Collection Cleanup Failure ({0})", metadata?.TestCollectionDisplayName ?? "<unknown test collection>");
	}

	/// <summary>
	/// Called when <see cref="ITestCollectionFinished"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestCollectionFinished(MessageHandlerArgs<ITestCollectionFinished> args)
	{
		Guard.ArgumentNotNull(args);

		MetadataCache.TryRemove(args.Message);
	}

	/// <summary>
	/// Called when <see cref="ITestCollectionStarting"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestCollectionStarting(MessageHandlerArgs<ITestCollectionStarting> args)
	{
		Guard.ArgumentNotNull(args);

		MetadataCache.Set(args.Message);
	}

	/// <summary>
	/// Called when <see cref="TestExecutionSummaries"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestExecutionSummaries(MessageHandlerArgs<TestExecutionSummaries> args)
	{
		Guard.ArgumentNotNull(args);

		WriteDefaultSummary(Logger, args.Message);
	}

	/// <summary>
	/// Called when <see cref="ITestFailed"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
	{
		Guard.ArgumentNotNull(args);

		var testFailed = args.Message;
		var frameInfo = StackFrameInfo.FromErrorMetadata(testFailed);
		var metadata = MetadataCache.TryGetTestMetadata(testFailed);

		lock (Logger.LockObject)
		{
			Logger.LogError(frameInfo, "    {0} [FAIL]", Escape(metadata?.TestDisplayName ?? "<unknown test>"));

			foreach (var messageLine in ExceptionUtility.CombineMessages(testFailed).Split([Environment.NewLine], StringSplitOptions.None))
				Logger.LogImportantMessage(frameInfo, "      {0}", messageLine);

			LogStackTrace(frameInfo, ExceptionUtility.CombineStackTraces(testFailed));
			LogOutput(frameInfo, testFailed.Output);
			LogWarnings(frameInfo, testFailed.Warnings);
		}
	}

	/// <summary>
	/// Called when <see cref="ITestFinished"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestFinished(MessageHandlerArgs<ITestFinished> args)
	{
		Guard.ArgumentNotNull(args);

		MetadataCache.TryRemove(args.Message);
	}

	/// <summary>
	/// Called when <see cref="ITestMethodCleanupFailure"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
	{
		Guard.ArgumentNotNull(args);

		var cleanupFailure = args.Message;
		var metadata = MetadataCache.TryGetMethodMetadata(args.Message);

		LogError(cleanupFailure, "Test Method Cleanup Failure ({0})", metadata?.MethodName ?? "<unknown test method>");
	}

	/// <summary>
	/// Called when <see cref="ITestMethodFinished"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestMethodFinished(MessageHandlerArgs<ITestMethodFinished> args)
	{
		Guard.ArgumentNotNull(args);

		MetadataCache.TryRemove(args.Message);
	}

	/// <summary>
	/// Called when <see cref="ITestMethodStarting"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestMethodStarting(MessageHandlerArgs<ITestMethodStarting> args)
	{
		Guard.ArgumentNotNull(args);

		MetadataCache.Set(args.Message);
	}

	/// <inheritdoc/>
	protected virtual void HandleTestOutput(MessageHandlerArgs<ITestOutput> args)
	{
		Guard.ArgumentNotNull(args);

		var testOutput = args.Message;
		var assemblyMetadata = MetadataCache.TryGetAssemblyMetadata(testOutput);
		var showLiveOutput = GetExecutionOptions(assemblyMetadata?.AssemblyPath).GetShowLiveOutputOrDefault();

		if (showLiveOutput)
			lock (Logger.LockObject)
			{
				var testMetadata = MetadataCache.TryGetTestMetadata(testOutput);

				Logger.LogMessage("    {0} [OUTPUT] {1}", Escape(testMetadata?.TestDisplayName ?? "<unknown test>"), Escape(testOutput.Output.TrimEnd()));
			}
	}

	/// <summary>
	/// Called when <see cref="ITestPassed"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
	{
		Guard.ArgumentNotNull(args);

		var testPassed = args.Message;
		var assemblyMetadata = MetadataCache.TryGetAssemblyMetadata(testPassed);
		var diagnosticMessages = GetExecutionOptions(assemblyMetadata?.AssemblyPath).GetDiagnosticMessagesOrDefault();

		if (testPassed.Warnings?.Length > 0 || (logPassingTestsWithOutput && diagnosticMessages && !string.IsNullOrEmpty(testPassed.Output)))
		{
			lock (Logger.LockObject)
			{
				var testMetadata = MetadataCache.TryGetTestMetadata(testPassed);

				Logger.LogImportantMessage("    {0} [PASS]", Escape(testMetadata?.TestDisplayName ?? "<unknown test>"));

				LogOutput(StackFrameInfo.None, testPassed.Output);
				LogWarnings(StackFrameInfo.None, testPassed.Warnings);
			}
		}
	}

	/// <summary>
	/// Called when <see cref="ITestSkipped"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
	{
		Guard.ArgumentNotNull(args);

		lock (Logger.LockObject)
		{
			var testSkipped = args.Message;
			var testMetadata = MetadataCache.TryGetTestMetadata(testSkipped);

			Logger.LogWarning("    {0} [SKIP]", Escape(testMetadata?.TestDisplayName ?? "<unknown test>"));
			Logger.LogImportantMessage("      {0}", EscapeMultiLineIndent(testSkipped.Reason, "      "));

			LogOutput(StackFrameInfo.None, testSkipped.Output);
			LogWarnings(StackFrameInfo.None, testSkipped.Warnings);
		}
	}

	/// <summary>
	/// Called when <see cref="ITestStarting"/> is raised.
	/// </summary>
	/// <param name="args">An object that contains the event data.</param>
	protected virtual void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
	{
		Guard.ArgumentNotNull(args);

		MetadataCache.Set(args.Message);
	}

	/// <summary>
	/// Writes the default summary to the given logger. Can be used by other reporters who also wish to write the
	/// standard summary information.
	/// </summary>
	/// <param name="logger">The logger used to send result messages to.</param>
	/// <param name="summaries">The execution summary to display.</param>
	public void WriteDefaultSummary(
		IRunnerLogger logger,
		TestExecutionSummaries summaries)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(summaries);

		logger.LogImportantMessage("=== TEST EXECUTION SUMMARY ===");

		var summariesWithDisplayName =
			summaries
				.SummariesByAssemblyUniqueID
				.Select(
					summary => (
						summary.Summary,
						summary.AssemblyUniqueID,
						AssemblyDisplayName: MetadataCache.TryGetAssemblyMetadata(summary.AssemblyUniqueID)?.SimpleAssemblyName() ?? "<unknown assembly>"
					)
				).OrderBy(summary => summary.AssemblyDisplayName)
				.ToList();

		var longestAssemblyName = summariesWithDisplayName.Max(summary => summary.AssemblyDisplayName.Length);
		var allTotal = summaries.SummariesByAssemblyUniqueID.Sum(summary => summary.Summary.Total).ToString(CultureInfo.CurrentCulture);
		var allErrors = summaries.SummariesByAssemblyUniqueID.Sum(summary => summary.Summary.Errors).ToString(CultureInfo.CurrentCulture);
		var allFailed = summaries.SummariesByAssemblyUniqueID.Sum(summary => summary.Summary.Failed).ToString(CultureInfo.CurrentCulture);
		var allSkipped = summaries.SummariesByAssemblyUniqueID.Sum(summary => summary.Summary.Skipped).ToString(CultureInfo.CurrentCulture);
		var allNotRun = summaries.SummariesByAssemblyUniqueID.Sum(summary => summary.Summary.NotRun).ToString(CultureInfo.CurrentCulture);
		var allTime = summaries.SummariesByAssemblyUniqueID.Sum(summary => summary.Summary.Time).ToString("0.000s", CultureInfo.CurrentCulture);

		foreach (var (summary, assemblyUniqueID, assemblyDisplayName) in summariesWithDisplayName)
		{
			if (summary.Total == 0)
				logger.LogImportantMessage("   {0}  Total: {1}", assemblyDisplayName.PadRight(longestAssemblyName), "0".PadLeft(allTotal.Length));
			else
			{
				var total = summary.Total.ToString(CultureInfo.CurrentCulture).PadLeft(allTotal.Length);
				var errors = summary.Errors.ToString(CultureInfo.CurrentCulture).PadLeft(allErrors.Length);
				var failed = summary.Failed.ToString(CultureInfo.CurrentCulture).PadLeft(allFailed.Length);
				var skipped = summary.Skipped.ToString(CultureInfo.CurrentCulture).PadLeft(allSkipped.Length);
				var notRun = summary.NotRun.ToString(CultureInfo.CurrentCulture).PadLeft(allNotRun.Length);
				var time = summary.Time.ToString("0.000s", CultureInfo.CurrentCulture).PadLeft(allTime.Length);

				logger.LogImportantMessage(
					"   {0}  Total: {1}, Errors: {2}, Failed: {3}, Skipped: {4}, Not Run: {5}, Time: {6}",
					assemblyDisplayName.PadRight(longestAssemblyName),
					total,
					errors,
					failed,
					skipped,
					notRun,
					time
				);
			}
		}

		if (summaries.SummariesByAssemblyUniqueID.Count > 1)
		{
			logger.LogImportantMessage(
				"   {0}         {1}          {2}          {3}           {4}           {5}        {6}",
				" ".PadRight(longestAssemblyName),
				"-".PadRight(allTotal.Length, '-'),
				"-".PadRight(allErrors.Length, '-'),
				"-".PadRight(allFailed.Length, '-'),
				"-".PadRight(allSkipped.Length, '-'),
				"-".PadRight(allNotRun.Length, '-'),
				"-".PadRight(allTime.Length, '-')
			);
			logger.LogImportantMessage(
				"   {0} {1}          {2}          {3}           {4}           {5}        {6} ({7:0.000s})",
				"GRAND TOTAL:".PadLeft(longestAssemblyName + 8),
				allTotal,
				allErrors,
				allFailed,
				allSkipped,
				allNotRun,
				allTime,
				summaries.ElapsedClockTime.TotalSeconds
			);
		}
	}

	sealed class ReaderWriterLockWrapper : IDisposable
	{
		static readonly ReaderWriterLockSlim @lock = new();
		static readonly ReaderWriterLockWrapper lockForRead = new(@lock.ExitReadLock);
		static readonly ReaderWriterLockWrapper lockForWrite = new(@lock.ExitWriteLock);

		readonly Action unlock;

		ReaderWriterLockWrapper(Action unlock) =>
			this.unlock = unlock;

		public void Dispose() =>
			unlock();

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
