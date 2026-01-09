using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// The message handler for <see cref="NUnitResultWriter"/>.
/// </summary>
public class NUnitResultWriterMessageHandler : ResultMetadataMessageHandlerBase<NUnitResultWriterMessageHandler.ResultMetadata>, IResultWriterMessageHandler
{
	int assemblyID;
	bool disposed;
	readonly IFileSystem fileSystem;
	readonly XElement testRunElement;
	DateTimeOffset timeFinish = DateTimeOffset.MinValue;
	DateTimeOffset timeStart = DateTimeOffset.MaxValue;
	readonly NUnitExecutionSummary totals = new();
	readonly Lazy<XmlWriter> xmlWriter;

	/// <summary>
	/// Initializes a new instance of the <see cref="NUnitResultWriterMessageHandler"/> class.
	/// </summary>
	/// <param name="fileName">The output file name</param>
	public NUnitResultWriterMessageHandler(string fileName) :
		this(new Lazy<XmlWriter>(() => XmlWriter.Create(fileName, XmlUtility.WriterSettings), isThreadSafe: false), FileSystem.Instance)
	{ }

	/// <summary>
	/// This constructor is for testing purposes only. Please call the public constructor.
	/// </summary>
	protected NUnitResultWriterMessageHandler(
		XmlWriter xmlWriter,
		IFileSystem fileSystem) :
			this(new Lazy<XmlWriter>(() => xmlWriter), fileSystem)
	{ }

	NUnitResultWriterMessageHandler(
		Lazy<XmlWriter> xmlWriter,
		IFileSystem fileSystem)
	{
		this.xmlWriter = xmlWriter;
		this.fileSystem = fileSystem;

		testRunElement = new(
			"test-run",
			new XAttribute("id", "0"),
			new XAttribute("runstate", "Runnable"),
			new XElement("command-line", new XCData(string.Empty))
		);
	}

	internal override ResultMetadata CreateMetadata() =>
		new(Interlocked.Increment(ref assemblyID));

	/// <inheritdoc/>
	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		if (!disposed)
			try
			{
				var timeFinishText = timeFinish.ToUniversalTime().ToString("u", CultureInfo.InvariantCulture);
				var timeStartText = timeFinish == DateTimeOffset.MinValue ? timeFinishText : timeStart.ToUniversalTime().ToString("u", CultureInfo.InvariantCulture);

				testRunElement.Add(
					new XAttribute("testcasecount", totals.Total),
					new XAttribute("result", totals.Result),
					new XAttribute("total", totals.Total),
					new XAttribute("passed", totals.Passed),
					new XAttribute("failed", totals.Failed),
					new XAttribute("warnings", totals.Warnings),
					new XAttribute("inconclusive", "0"),
					new XAttribute("skipped", totals.Skipped + totals.NotRun),
					new XAttribute("asserts", "0"),
					new XAttribute("engine-version", ThisAssembly.AssemblyFileVersion),
					new XAttribute("clr-version", "4.0.30319"),
					new XAttribute("start-time", timeStartText),
					new XAttribute("end-time", timeFinishText),
					new XAttribute("duration", totals.Time.ToString("0.000000", CultureInfo.InvariantCulture))
				);

				testRunElement.Save(xmlWriter.Value);
				xmlWriter.Value.SafeDispose();
			}
			finally
			{
				disposed = true;
			}

		return default;
	}

	static string FormatDateTime(DateTimeOffset dateTime) =>
		dateTime.ToUniversalTime().ToString("yyyy-MM-dd\\THH:mm:ss.fffffffZ", CultureInfo.InvariantCulture);

	static ElementWithExtraCounts GetTestClass(
		ResultMetadata resultMetadata,
		string? uniqueID) =>
			resultMetadata.TestClasses.GetOrAdd(uniqueID ?? string.Empty, _ => new(new XElement("test-suite")));

	static ElementWithExtraCounts GetTestCollection(
		ResultMetadata resultMetadata,
		string uniqueID) =>
			resultMetadata.TestCollections.GetOrAdd(uniqueID, _ => new(new XElement("test-suite")));

	void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
	{
		var message = args.Message;
		if (message.AssemblyUniqueID is null || !TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		lock (resultMetadata)
		{
			++resultMetadata.Errors;
			resultMetadata.LocalErrors.Add((ExceptionUtility.CombineMessages(message), ExceptionUtility.CombineStackTraces(message)));
		}
	}

	void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		lock (resultMetadata)
		{
			++resultMetadata.Errors;
			resultMetadata.LocalErrors.Add((ExceptionUtility.CombineMessages(message), ExceptionUtility.CombineStackTraces(message)));
		}
	}

	void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args) =>
		HandleTestFixtureError(args.Message);

	void HandleTestClassCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args) =>
		HandleTestFixtureError(args.Message);

	void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args) =>
		HandleTestFixtureError(args.Message);

	void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		var collection = GetTestCollection(resultMetadata, message.TestCollectionUniqueID);
		lock (collection)
		{
			++collection.Errors;
			collection.LocalErrors.Add((ExceptionUtility.CombineMessages(message), ExceptionUtility.CombineStackTraces(message)));
		}
	}

	void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;
		if (resultMetadata.MetadataCache.TryGetTestMetadata(message) is not ITestStarting testStarting)
			return;

		var testClassMetadata = resultMetadata.MetadataCache.TryGetClassMetadata(message);
		var testMethodMetadata = resultMetadata.MetadataCache.TryGetMethodMetadata(message);
		var testClass = GetTestClass(resultMetadata, message.TestClassUniqueID);

		lock (testClass)
		{
			var testCaseElement = new XElement("test-case");
			testClass.Element.Add(testCaseElement);

			RecordTestResult(testCaseElement, "Runnable", "Failed", testClassMetadata, testMethodMetadata, testStarting, message, resultMetadata);
		}
	}

	void HandleTestFixtureError<T>(T error)
		where T : IErrorMetadata, ITestClassMessage
	{
		if (!TryGetResultMetadata(error.AssemblyUniqueID, out var resultMetadata))
			return;

		var @class = GetTestClass(resultMetadata, error.TestClassUniqueID);
		lock (@class)
		{
			++@class.Errors;
			@class.LocalErrors.Add((ExceptionUtility.CombineMessages(error), ExceptionUtility.CombineStackTraces(error)));
		}
	}

	void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args) =>
		HandleTestFixtureError(args.Message);

	void HandleTestNotRun(MessageHandlerArgs<ITestNotRun> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;
		if (resultMetadata.MetadataCache.TryGetTestMetadata(message) is not ITestStarting testStarting)
			return;

		var testClassMetadata = resultMetadata.MetadataCache.TryGetClassMetadata(message);
		var testMethodMetadata = resultMetadata.MetadataCache.TryGetMethodMetadata(message);
		var testClass = GetTestClass(resultMetadata, message.TestClassUniqueID);

		lock (testClass)
		{
			var testCaseElement = new XElement("test-case");
			testClass.Element.Add(testCaseElement);

			RecordTestResult(testCaseElement, "Explicit", "Skipped", testClassMetadata, testMethodMetadata, testStarting, message, resultMetadata);

			testCaseElement.Add(new XAttribute("label", "Explicit"));
		}
	}

	void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;
		if (resultMetadata.MetadataCache.TryGetTestMetadata(message) is not ITestStarting testStarting)
			return;

		var testClassMetadata = resultMetadata.MetadataCache.TryGetClassMetadata(message);
		var testMethodMetadata = resultMetadata.MetadataCache.TryGetMethodMetadata(message);
		var testClass = GetTestClass(resultMetadata, message.TestClassUniqueID);

		lock (testClass)
		{
			var testCaseElement = new XElement("test-case");
			testClass.Element.Add(testCaseElement);

			var hasWarnings = message.Warnings is not null && message.Warnings.Length != 0;

			if (hasWarnings)
				++testClass.Warnings;

			RecordTestResult(testCaseElement, "Runnable", hasWarnings ? "Warning" : "Passed", testClassMetadata, testMethodMetadata, testStarting, message, resultMetadata);
		}
	}

	void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;
		if (resultMetadata.MetadataCache.TryGetTestMetadata(message) is not ITestStarting testStarting)
			return;

		var testClassMetadata = resultMetadata.MetadataCache.TryGetClassMetadata(message);
		var testMethodMetadata = resultMetadata.MetadataCache.TryGetMethodMetadata(message);
		var testClass = GetTestClass(resultMetadata, message.TestClassUniqueID);

		lock (testClass)
		{
			var testCaseElement = new XElement("test-case");
			testClass.Element.Add(testCaseElement);

			var propertiesElement = RecordTestResult(testCaseElement, "Ignored", "Skipped", testClassMetadata, testMethodMetadata, testStarting, message, resultMetadata);

			if (propertiesElement is null)
			{
				propertiesElement = new XElement("properties");
				testCaseElement.Add(propertiesElement);
			}

			propertiesElement.Add(
				new XElement("property",
					new XAttribute("name", "_SKIPREASON"),
					new XAttribute("value", message.Reason)
				)
			);

			testCaseElement.Add(
				new XAttribute("label", "Ignored"),
				new XElement("reason", new XElement("message", new XCData(message.Reason)))
			);
		}
	}

	/// <inheritdoc/>
	public override bool OnMessage(IMessageSinkMessage message)
	{
		message.DispatchWhen<IErrorMessage>(HandleErrorMessage);
		message.DispatchWhen<ITestAssemblyCleanupFailure>(HandleTestAssemblyCleanupFailure);
		message.DispatchWhen<ITestCollectionCleanupFailure>(HandleTestCollectionCleanupFailure);
		message.DispatchWhen<ITestClassCleanupFailure>(HandleTestClassCleanupFailure);
		message.DispatchWhen<ITestMethodCleanupFailure>(HandleTestMethodCleanupFailure);
		message.DispatchWhen<ITestCaseCleanupFailure>(HandleTestCaseCleanupFailure);
		message.DispatchWhen<ITestCleanupFailure>(HandleTestCleanupFailure);

		message.DispatchWhen<ITestFailed>(HandleTestFailed);
		message.DispatchWhen<ITestNotRun>(HandleTestNotRun);
		message.DispatchWhen<ITestPassed>(HandleTestPassed);
		message.DispatchWhen<ITestSkipped>(HandleTestSkipped);

		return base.OnMessage(message);
	}

	internal override void OnTestAssemblyFinished(
		ITestAssemblyFinished finished,
		ResultMetadata resultMetadata)
	{
		if (resultMetadata.MetadataCache.TryGetAssemblyMetadata(finished.AssemblyUniqueID) is not ITestAssemblyStarting starting)
			return;

		if (timeStart > starting.StartTime)
			timeStart = starting.StartTime;
		if (timeFinish < finished.FinishTime)
			timeFinish = finished.FinishTime;

		var clrVersion = default(string);
		var targetRuntimeFramework = default(string);

		if (starting.TargetFramework is not null)
			try
			{
				var framework = new FrameworkName(starting.TargetFramework);
				clrVersion = framework.Version.ToString();
				targetRuntimeFramework = (framework.Identifier == ".NETFramework" ? "net-" : "netcore-") + clrVersion;
			}
			catch { }

		var computer = EnvironmentUtility.Computer ?? string.Empty;
		var cwd = Path.GetDirectoryName(starting.AssemblyPath) ?? string.Empty;
		var assemblyTestSuiteElement =
			new XElement("test-suite",
				new XElement("environment",
					new XAttribute("framework-version", ThisAssembly.AssemblyFileVersion),
					new XAttribute("clr-version", clrVersion ?? string.Empty),
					new XAttribute("os-version", RuntimeInformation.OSDescription.Trim()),
					new XAttribute("platform", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Win32NT" : "Unix"),
					new XAttribute("cwd", cwd),
					new XAttribute("machine-name", computer),
					new XAttribute("user", EnvironmentUtility.User ?? string.Empty),
					new XAttribute("user-domain", EnvironmentUtility.Domain ?? computer),
					// TODO: Culture information could come back via ITestAssemblyStarting
					new XAttribute("culture", string.Empty),
					new XAttribute("uiculture", string.Empty),
#pragma warning disable CA1308 // We don't control the casing of this value
					new XAttribute("os-architecture", RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant())
#pragma warning restore CA1308
				),
				new XElement("settings",
					new XElement("setting",
						new XAttribute("name", "WorkDirectory"),
						new XAttribute("value", cwd)
					),
					new XElement("setting",
						new XAttribute("name", "ImageTargetFrameworkName"),
						new XAttribute("value", starting.TargetFramework ?? string.Empty)
					),
					new XElement("setting",
						new XAttribute("name", "TargetRuntimeFramework"),
						new XAttribute("value", targetRuntimeFramework ?? string.Empty)
					)
				),
				resultMetadata.TestCollections.Values.Select(tc => tc.Element)
			);

		RecordTestSuiteStarting(assemblyTestSuiteElement, starting, resultMetadata);
		RecordTestSuiteFinished(assemblyTestSuiteElement, finished, resultMetadata.Warnings, resultMetadata.Errors, resultMetadata.LocalErrors);

		lock (testRunElement)
		{
			testRunElement.Add(assemblyTestSuiteElement);
			totals.AddFrom(finished, resultMetadata.Errors, resultMetadata.Warnings);
		}
	}

	internal override void OnTestClassFinished(
		ITestClassFinished message,
		ResultMetadata resultMetadata)
	{
		var @class = GetTestClass(resultMetadata, message.TestClassUniqueID);
		lock (@class)
			RecordTestSuiteFinished(@class.Element, message, @class.Warnings, @class.Errors, @class.LocalErrors);

		var collection = GetTestCollection(resultMetadata, message.TestCollectionUniqueID);
		lock (collection)
		{
			collection.Element.Add(@class.Element);
			collection.Errors += @class.Errors;
			collection.Warnings += @class.Warnings;
		}
	}

	internal override void OnTestClassStarting(
		ITestClassStarting message,
		ResultMetadata resultMetadata)
	{
		var @class = GetTestClass(resultMetadata, message.TestClassUniqueID);
		lock (@class)
			RecordTestSuiteStarting(@class.Element, message, resultMetadata);
	}

	internal override void OnTestCollectionFinished(
		ITestCollectionFinished message,
		ResultMetadata resultMetadata)
	{
		var collection = GetTestCollection(resultMetadata, message.TestCollectionUniqueID);
		lock (collection)
			RecordTestSuiteFinished(collection.Element, message, collection.Warnings, collection.Errors, collection.LocalErrors);

		lock (resultMetadata)
		{
			resultMetadata.Errors += collection.Errors;
			resultMetadata.Warnings += collection.Warnings;
		}
	}

	internal override void OnTestCollectionStarting(
		ITestCollectionStarting message,
		ResultMetadata resultMetadata)
	{
		var collection = GetTestCollection(resultMetadata, message.TestCollectionUniqueID);
		lock (collection)
			RecordTestSuiteStarting(collection.Element, message, resultMetadata);
	}

	internal override void OnTestFinished(
		ITestFinished message,
		ResultMetadata resultMetadata)
	{
		if (!resultMetadata.TestResults.TryRemove(message.TestUniqueID, out var testCaseElement) || message.Attachments.Count == 0)
			return;

		var attachmentsElement = new XElement("attachments");
		var basePath = Path.Combine(Path.GetTempPath(), message.TestUniqueID);
		fileSystem.CreateDirectory(basePath);

		foreach (var attachment in message.Attachments)
		{
			var fileName = attachment.Key;
			var localFilePath = default(string);

			switch (attachment.Value.AttachmentType)
			{
				case TestAttachmentType.ByteArray:
					var binaryData = attachment.Value.AsByteArray();
					localFilePath = Path.Combine(basePath, MediaTypeUtility.GetSanitizedFileNameWithExtension(fileName, binaryData.MediaType));
					fileSystem.WriteAllBytes(localFilePath, binaryData.ByteArray);
					break;

				case TestAttachmentType.String:
					localFilePath = Path.Combine(basePath, MediaTypeUtility.GetSanitizedFileNameWithExtension(fileName, "text/plain"));
					fileSystem.WriteAllText(localFilePath, attachment.Value.AsString());
					break;
			}

			if (localFilePath is not null)
				attachmentsElement.Add(
					new XElement("attachment",
						new XAttribute("filePath", localFilePath),
						new XAttribute("description", attachment.Key)
					)
				);
		}

		testCaseElement.Add(attachmentsElement);
	}

	static XElement? RecordTestResult(
		XElement testCaseElement,
		string runState,
		string result,
		ITestClassMetadata? testClassMetadata,
		ITestMethodMetadata? testMethodMetadata,
		ITestStarting testStarting,
		ITestResultMessage testResult,
		ResultMetadata resultMetadata)
	{
		resultMetadata.TestResults[testResult.TestUniqueID] = testCaseElement;

		testCaseElement.Add(
			new XAttribute("id", resultMetadata.NextID()),
			new XAttribute("name", testMethodMetadata?.MethodName ?? testStarting.TestDisplayName),
			new XAttribute("fullname", testStarting.TestDisplayName),
			new XAttribute("methodname", testMethodMetadata?.MethodName ?? string.Empty),
			new XAttribute("classname", testClassMetadata?.TestClassName ?? string.Empty),
			new XAttribute("runstate", runState),
			new XAttribute("result", result),
			new XAttribute("start-time", FormatDateTime(testStarting.StartTime)),
			new XAttribute("end-time", FormatDateTime(testResult.FinishTime)),
			new XAttribute("duration", testResult.ExecutionTime.ToString("0.000000", CultureInfo.InvariantCulture)),
			new XAttribute("asserts", "0")
		);

		if (!string.IsNullOrWhiteSpace(testResult.Output))
			testCaseElement.Add(new XElement("output", testResult.Output));

		var assertions = new List<(string Result, string Message, string? StackTrace)>();

		if (testResult.Warnings is not null)
			foreach (var warning in testResult.Warnings)
				assertions.Add(("Warning", warning, null));

		if (testResult is IErrorMetadata error)
			assertions.Add(("Failed", ExceptionUtility.CombineMessages(error), ExceptionUtility.CombineStackTraces(error)));

		if (assertions.Count != 0)
		{
			string summaryMessage;
			string? summaryStackTrace = default;

			if (assertions.Count == 1)
			{
				summaryMessage = assertions[0].Message;
				summaryStackTrace = assertions[0].StackTrace;
			}
			else
				summaryMessage =
					"Multiple failures or warnings in test:\r\n\r\n" +
					string.Join("\r\n\r\n", assertions.Select((a, idx) => $"  {idx + 1}) {a.Message}"));

			testCaseElement.Add(
				new XElement(result == "Failed" ? "failure" : "reason",
					new XElement("message", new XCData(summaryMessage)),
					new XElement("stack-trace", new XCData(summaryStackTrace ?? string.Empty))
				),
				new XElement("assertions",
					assertions.Select(a =>
						new XElement("assertion",
							new XAttribute("result", a.Result),
							new XElement("message", new XCData(a.Message)),
							new XElement("stack-trace", new XCData(a.StackTrace ?? string.Empty))
						)
					)
				)
			);
		}

		var propertiesElement = default(XElement);

		if (testStarting.Traits.Count != 0)
		{
			propertiesElement = new XElement("properties");
			testCaseElement.Add(propertiesElement);

			foreach (var kvp in testStarting.Traits)
				foreach (var value in kvp.Value)
					propertiesElement.Add(new XElement("property", new XAttribute("name", kvp.Key), new XAttribute("value", value)));
		}

		return propertiesElement;
	}

	static void RecordTestSuiteStarting(
		XElement testSuiteElement,
		IStartingMessage starting,
		ResultMetadata resultMetadata) =>
			testSuiteElement.Add(
				new XAttribute("type", starting switch
				{
					ITestAssemblyStarting => "Assembly",
					ITestCollectionStarting => "TestSuite",
					ITestClassStarting => "TestFixture",
					_ => throw new ArgumentException("Unknown starting type", nameof(starting)),
				}),
				new XAttribute("id", resultMetadata.NextID()),
				new XAttribute("name", starting switch
				{
					ITestAssemblyStarting assemblyStarting => Path.GetFileName(assemblyStarting.AssemblyPath),
					ITestCollectionStarting collectionStarting => collectionStarting.TestCollectionDisplayName,
					ITestClassStarting classStarting => classStarting.TestClassSimpleName,
					_ => throw new ArgumentException("Unknown starting type", nameof(starting)),
				}),
				new XAttribute("fullname", starting switch
				{
					ITestAssemblyStarting assemblyStarting => assemblyStarting.AssemblyPath,
					ITestCollectionStarting collectionStarting => collectionStarting.TestCollectionClassName ?? string.Empty,
					ITestClassStarting classStarting => classStarting.TestClassName,
					_ => throw new ArgumentException("Unknown starting type", nameof(starting)),
				}),
				new XAttribute("runstate", "Runnable"),
				new XAttribute("start-time", FormatDateTime(starting.StartTime))
			);

	static void RecordTestSuiteFinished(
		XElement testSuiteElement,
		IExecutionSummaryMetadata finished,
		int warnings,
		int errorCount,
		IReadOnlyList<(string Message, string? StackTrace)> localErrors)
	{
		testSuiteElement.Add(
			new XAttribute("end-time", FormatDateTime(finished.FinishTime)),
			new XAttribute("testcasecount", finished.TestsTotal),
			new XAttribute("result", ToResult(finished.TestsFailed + errorCount, warnings)),
			new XAttribute("duration", finished.ExecutionTime.ToString("0.000000", CultureInfo.InvariantCulture)),
			new XAttribute("total", finished.TestsTotal),
			new XAttribute("passed", finished.TestsTotal - finished.TestsFailed - finished.TestsSkipped - finished.TestsNotRun - warnings),
			new XAttribute("failed", finished.TestsFailed),
			new XAttribute("warnings", warnings),
			new XAttribute("inconclusive", "0"),
			new XAttribute("skipped", finished.TestsSkipped + finished.TestsNotRun),
			new XAttribute("asserts", "0")
		);

		if (finished.TestsFailed != 0 || errorCount != 0 || localErrors.Count != 0)
			testSuiteElement.Add(
				new XAttribute("site", localErrors.Count == 0 ? "Child" : "TearDown"),
				ToFailureElement(localErrors)
			);
		else if (warnings != 0)
			testSuiteElement.Add(
				new XAttribute("site", "Child"),
				new XElement("failure", new XElement("message", new XCData("One or more child tests had warnings")))
			);
	}

	static XElement ToFailureElement(IReadOnlyList<(string Message, string? StackTrace)> errors)
	{
		string message;
		string? stackTrace = default;

		if (errors.Count == 0)
			message = "One or more child tests had errors";
		else if (errors.Count == 1)
		{
			message = errors[0].Message;
			stackTrace = errors[0].StackTrace;
		}
		else
			message =
				"Multiple failures in clean-up:\r\n\r\n" +
				string.Join("\r\n\r\n", errors.Select((a, idx) => $"{idx + 1}) {a.Message}{(a.StackTrace is null ? "" : "\r\n" + a.StackTrace)}"));

		var result = new XElement("failure", new XElement("message", new XCData(message)));
		if (stackTrace is not null)
			result.Add(new XElement("stack-trace", stackTrace));

		return result;
	}

	static string ToResult(
		int failCount,
		int warningCount) =>
			failCount != 0 ? "Failed" : warningCount != 0 ? "Warning" : "Passed";

	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
	public sealed class ResultMetadata(int id) : ResultMetadataBase
	{
		int innerID = 999;

		internal int Errors;

		internal List<(string Message, string? StackTrace)> LocalErrors = [];

		internal int Warnings;

		internal string NextID() =>
			$"{id}-{Interlocked.Increment(ref innerID)}";

		internal ConcurrentDictionary<string, ElementWithExtraCounts> TestClasses { get; } = [];

		internal ConcurrentDictionary<string, ElementWithExtraCounts> TestCollections { get; } = [];

		internal ConcurrentDictionary<string, XElement> TestResults { get; } = [];
	}

	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
	public sealed class ElementWithExtraCounts(XElement element)
	{
		internal XElement Element => element;

		internal int Errors;

		internal List<(string Message, string? StackTrace)> LocalErrors = [];

		internal int Warnings;
	}

	internal sealed class NUnitExecutionSummary : ExecutionSummary
	{
		public override int Passed =>
			base.Passed - Warnings;

		public string Result =>
			ToResult(Failed + Errors, Warnings);

		public int Warnings { get; set; }

		public void AddFrom(
			IExecutionSummaryMetadata summary,
			int errors,
			int warnings)
		{
			Errors += errors;
			Failed += summary.TestsFailed;
			NotRun += summary.TestsNotRun;
			Skipped += summary.TestsSkipped;
			Time += summary.ExecutionTime;
			Total += summary.TestsTotal;
			Warnings += warnings;
		}
	}
}
