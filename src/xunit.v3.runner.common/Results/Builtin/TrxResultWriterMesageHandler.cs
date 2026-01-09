using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// The message handler for <see cref="TrxResultWriter"/>.
/// </summary>
public class TrxResultWriterMessageHandler : ResultMetadataMessageHandlerBase<TrxResultWriterMessageHandler.ResultMetadata>, IResultWriterMessageHandler
{
	static readonly XNamespace ns = XNamespace.Get("http://microsoft.com/schemas/VisualStudio/TeamTest/2010");

	readonly string computer;
	bool disposed;
	readonly IFileSystem fileSystem;
	readonly XElement resultsElement;
	readonly XElement testDefinitionsElement;
	readonly XElement testEntriesElement;
	readonly string testRunID = Guid.NewGuid().ToString();
	readonly Lazy<XmlWriter> textWriter;
	DateTimeOffset timeFinish = DateTimeOffset.MinValue;
	DateTimeOffset timeStart = DateTimeOffset.MaxValue;
	int totalErrors;
	int totalFailed;
	int totalNotRun;
	int totalRun;
	int totalSkipped;
	readonly string user;

	/// <summary>
	/// Initializes a new instance of the <see cref="TrxResultWriterMessageHandler"/> class.
	/// </summary>
	/// <param name="fileName">The output file name</param>
	public TrxResultWriterMessageHandler(string fileName) :
		this(new Lazy<XmlWriter>(() => XmlWriter.Create(fileName, XmlUtility.WriterSettings), isThreadSafe: false), FileSystem.Instance)
	{ }

	/// <summary>
	/// This constructor is for testing purposes only. Please call the public constructor.
	/// </summary>
	protected TrxResultWriterMessageHandler(
		XmlWriter textWriter,
		IFileSystem fileSystem) :
			this(new Lazy<XmlWriter>(() => textWriter), fileSystem)
	{ }

	TrxResultWriterMessageHandler(
		Lazy<XmlWriter> textWriter,
		IFileSystem fileSystem)
	{
		this.textWriter = textWriter;
		this.fileSystem = fileSystem;

		resultsElement = new XElement(ns + "Results");
		testDefinitionsElement = new XElement(ns + "TestDefinitions");
		testEntriesElement = new XElement(ns + "TestEntries");
		computer = EnvironmentUtility.Computer ?? "unknown";
		user = EnvironmentUtility.User ?? "unknown";
	}

	internal override ResultMetadata CreateMetadata() =>
		new();

	/// <inheritdoc/>
	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		if (!disposed)
			try
			{
				var timeFinishRtf = timeFinish.ToRtf();
				var timeStartRtf = timeFinish == DateTimeOffset.MinValue ? timeFinishRtf : timeStart.ToRtf();

				var testRunElement =
					new XElement(ns + "TestRun",
						new XAttribute("id", testRunID),
						new XAttribute("name", $"{user}@{computer} {timeStartRtf}"),
						new XAttribute("runUser", user),
						new XElement(ns + "Times",
							new XAttribute("creation", timeStartRtf),
							new XAttribute("queuing", timeStartRtf),
							new XAttribute("start", timeStartRtf),
							new XAttribute("finish", timeFinishRtf)
						),
						new XElement(ns + "TestSettings",
							new XAttribute("name", "default"),
							new XAttribute("id", "6c4d5628-128d-4c3b-a1a4-ab366a4594ad")
						),
						resultsElement,
						testDefinitionsElement,
						testEntriesElement,
						new XElement(ns + "TestLists",
							new XElement(ns + "TestList",
								new XAttribute("name", "Results Not in a List"),
								new XAttribute("id", "8c84fa94-04c1-424b-9868-57a2d4851a1d")
							),
							new XElement(ns + "TestList",
								new XAttribute("name", "All Loaded Results"),
								new XAttribute("id", "19431567-8539-422a-85d7-44ee4e166bda")
							)
						),
						new XElement(ns + "ResultSummary",
							new XAttribute("outcome", totalErrors + totalFailed > 0 ? "Failed" : "Completed"),
							new XElement(ns + "Counters",
								new XAttribute("total", totalRun),
								new XAttribute("executed", totalRun - totalSkipped - totalNotRun),
								new XAttribute("passed", totalRun - totalSkipped - totalNotRun - totalFailed),
								new XAttribute("failed", totalFailed),
								new XAttribute("error", totalErrors),
								new XAttribute("timeout", 0),
								new XAttribute("aborted", 0),
								new XAttribute("inconclusive", 0),
								new XAttribute("passedButRunAborted", 0),
								new XAttribute("notRunnable", totalNotRun),
								new XAttribute("notExecuted", totalSkipped),
								new XAttribute("disconnected", 0),
								new XAttribute("warning", 0),
								new XAttribute("completed", 0),
								new XAttribute("inProgress", 0),
								new XAttribute("pending", 0)
							)
						)
					);

				testRunElement.Save(textWriter.Value);
				textWriter.Value.SafeDispose();
			}
			finally
			{
				disposed = true;
			}

		return default;
	}

	void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		var resultElement = RecordResult(resultMetadata, message, "Failed");
		if (resultElement is not null)
		{
			var errorInfo = new XElement(ns + "ErrorInfo");

			var exceptionMessage = ExceptionUtility.CombineMessages(message);
			if (!string.IsNullOrWhiteSpace(exceptionMessage))
				errorInfo.Add(new XElement(ns + "Message", exceptionMessage));

			var exceptionStackTrace = ExceptionUtility.CombineStackTraces(message);
			if (!string.IsNullOrWhiteSpace(exceptionStackTrace))
				errorInfo.Add(new XElement(ns + "StackTrace", exceptionStackTrace));

			var output = resultElement.Element(ns + "Output");
			if (output is null)
			{
				output = new XElement(ns + "Output");
				resultElement.Add(output);
			}

			output.Add(errorInfo);
		}

		RecordTestDefinition(resultMetadata, message);
		RecordTestEntry(resultMetadata, message);
	}

	void HandleTestNotRun(MessageHandlerArgs<ITestNotRun> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		RecordResult(resultMetadata, message, "NotRunnable");
		RecordTestDefinition(resultMetadata, message);
		RecordTestEntry(resultMetadata, message);
	}

	void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		RecordResult(resultMetadata, message, "Passed");
		RecordTestDefinition(resultMetadata, message);
		RecordTestEntry(resultMetadata, message);
	}

	void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		var resultElement = RecordResult(resultMetadata, message, "NotExecuted");
		if (resultElement is not null)
		{
			var output = resultElement.Element(ns + "Output");
			if (output is null)
			{
				output = new XElement(ns + "Output");
				resultElement.Add(output);
			}

			output.Add(new XElement(ns + "StdOut", message.Reason));
		}

		RecordTestDefinition(resultMetadata, message);
		RecordTestEntry(resultMetadata, message);
	}

	/// <inheritdoc/>
	public override bool OnMessage(IMessageSinkMessage message)
	{
		if (message is IErrorMessage
			|| message is ITestAssemblyCleanupFailure
			|| message is ITestCaseCleanupFailure
			|| message is ITestClassCleanupFailure
			|| message is ITestCleanupFailure
			|| message is ITestCollectionCleanupFailure
			|| message is ITestMethodCleanupFailure)
			++totalErrors;

		message.DispatchWhen<ITestFailed>(HandleTestFailed);
		message.DispatchWhen<ITestNotRun>(HandleTestNotRun);
		message.DispatchWhen<ITestPassed>(HandleTestPassed);
		message.DispatchWhen<ITestSkipped>(HandleTestSkipped);

		return base.OnMessage(message);
	}

	internal override void OnTestAssemblyFinished(
		ITestAssemblyFinished message,
		ResultMetadata resultMetadata)
	{
		if (message.FinishTime > timeFinish)
			timeFinish = message.FinishTime;

		totalFailed += message.TestsFailed;
		totalNotRun += message.TestsNotRun;
		totalRun += message.TestsTotal;
		totalSkipped += message.TestsSkipped;
	}

	internal override void OnTestAssemblyStarting(
		ITestAssemblyStarting message,
		ResultMetadata resultMetadata)
	{
		if (message.StartTime < timeStart)
			timeStart = message.StartTime;
	}

	internal override void OnTestFinished(
		ITestFinished message,
		ResultMetadata resultMetadata)
	{
		resultMetadata.TestIDs.TryRemove(message.TestUniqueID, out var _);
		if (!resultMetadata.TestResults.TryRemove(message.TestUniqueID, out var unitTestResultElement) || message.Attachments.Count == 0)
			return;

		var resultFiles = new XElement(ns + "ResultFiles");
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
				resultFiles.Add(
					new XElement(ns + "ResultFile",
						new XAttribute("path", localFilePath)
					)
				);
		}

		unitTestResultElement.Add(resultFiles);
	}

	XElement? RecordResult(
		ResultMetadata resultMetadata,
		ITestResultMessage testResult,
		string outcome)
	{
		var testMetadata = resultMetadata.MetadataCache.TryGetTestMetadata(testResult);
		if (testMetadata is null)
			return null;

		var testId = resultMetadata.TestIDs.GetOrAdd(testResult.TestUniqueID, _ => Guid.NewGuid().ToString());
		var testStartTime = (testMetadata as ITestStarting)?.StartTime ?? testResult.FinishTime;
		var unitTestResultElement = new XElement(ns + "UnitTestResult",
			new XAttribute("testName", testMetadata.TestDisplayName),
			new XAttribute("outcome", outcome),
			new XAttribute("testType", "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b"),
			new XAttribute("testListId", "8c84fa94-04c1-424b-9868-57a2d4851a1d"),
			new XAttribute("testId", testId),
			new XAttribute("executionId", testId),
			new XAttribute("computerName", computer),
			new XAttribute("duration", testResult.ExecutionTime.ToTimespanRtf()),
			new XAttribute("startTime", testStartTime.ToRtf()),
			new XAttribute("endTime", testResult.FinishTime.ToRtf())
		);

		if (!string.IsNullOrWhiteSpace(testResult.Output))
		{
			var textMessages = new XElement(ns + "TextMessages");
			var output = new XElement(ns + "Output", textMessages);

			foreach (var line in testResult.Output.TrimEnd('\r', '\n').Split(["\r\n"], StringSplitOptions.None))
				textMessages.Add(new XElement(ns + "Message", new XText(line)));

			unitTestResultElement.Add(output);
		}

		resultMetadata.TestResults.TryAdd(testResult.TestUniqueID, unitTestResultElement);

		lock (resultsElement)
			resultsElement.Add(unitTestResultElement);

		return unitTestResultElement;
	}

	void RecordTestDefinition(
		ResultMetadata resultMetadata,
		ITestResultMessage testResult)
	{
		var assemblyMetadata = resultMetadata.MetadataCache.TryGetAssemblyMetadata(testResult);
		var classMetadata =
			testResult.TestClassUniqueID is not null
				? resultMetadata.MetadataCache.TryGetClassMetadata(testResult.TestClassUniqueID)
				: null;
		var methodMetadata =
			testResult.TestMethodUniqueID is not null
				? resultMetadata.MetadataCache.TryGetMethodMetadata(testResult.TestMethodUniqueID)
				: null;
		var testMetadata = resultMetadata.MetadataCache.TryGetTestMetadata(testResult);
		var testId = resultMetadata.TestIDs.GetOrAdd(testResult.TestUniqueID, _ => Guid.NewGuid().ToString());

		if (assemblyMetadata is null || testMetadata is null)
			return;

		var unitTestElement =
			new XElement(ns + "UnitTest",
				new XAttribute("name", testMetadata.TestDisplayName),
				new XAttribute("id", testId),
				new XAttribute("storage", assemblyMetadata.AssemblyPath),
				new XElement(ns + "Execution",
					new XAttribute("id", testId)
				)
			);

		if (classMetadata is not null && methodMetadata is not null)
			unitTestElement.Add(
				new XElement(ns + "TestMethod",
					new XAttribute("codeBase", assemblyMetadata.AssemblyPath),
					new XAttribute("className", classMetadata.TestClassName),
					new XAttribute("name", methodMetadata.MethodName),
					new XAttribute("adapterTypeName", $"executor://{testRunID}/xunit.v3/{ThisAssembly.AssemblyFileVersion}")
				)
			);

		lock (testDefinitionsElement)
			testDefinitionsElement.Add(unitTestElement);
	}

	void RecordTestEntry(
		ResultMetadata resultMetadata,
		ITestResultMessage testResult)
	{
		var testId = resultMetadata.TestIDs.GetOrAdd(testResult.TestUniqueID, _ => Guid.NewGuid().ToString());

		lock (testEntriesElement)
			testEntriesElement.Add(
				new XElement(ns + "TestEntry",
					new XAttribute("testListId", "8c84fa94-04c1-424b-9868-57a2d4851a1d"),
					new XAttribute("testId", testId),
					new XAttribute("executionId", testId)
				)
			);
	}

	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
	public sealed class ResultMetadata : ResultMetadataBase
	{
		internal ConcurrentDictionary<string, string> TestIDs { get; } = [];

		internal ConcurrentDictionary<string, XElement> TestResults { get; } = [];
	}
}
