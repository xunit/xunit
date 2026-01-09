using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// The message handler for <see cref="XmlV2ResultWriter"/>.
/// </summary>
public class XmlV2ResultWriterMessageHandler : XunitXmlResultWriterMessageHandlerBase<XmlV2ResultWriterMessageHandler.ResultMetadata>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XmlV2ResultWriterMessageHandler"/> class.
	/// </summary>
	/// <param name="fileName">The output file name</param>
	public XmlV2ResultWriterMessageHandler(string fileName) :
		this(new Lazy<XmlWriter>(() => XmlWriter.Create(fileName, XmlUtility.WriterSettings), isThreadSafe: false))
	{ }

	/// <summary>
	/// This constructor is for testing purposes only. Please call the public constructor.
	/// </summary>
	protected XmlV2ResultWriterMessageHandler(XmlWriter xmlWriter) :
		this(new Lazy<XmlWriter>(() => xmlWriter))
	{ }

	XmlV2ResultWriterMessageHandler(Lazy<XmlWriter> xmlWriter) :
		base(xmlWriter)
	{
		AssembliesElement.Add(
			new XAttribute("schema-version", "3"),
			new XAttribute("id", Guid.NewGuid().ToString("d"))
		);

		var computer = EnvironmentUtility.Computer;
		if (computer is not null)
			AssembliesElement.Add(new XAttribute("computer", computer));

		var user = EnvironmentUtility.User;
		if (user is not null)
			AssembliesElement.Add(new XAttribute("user", user));
	}

	static void AddError(
		string type,
		string? name,
		ResultMetadata resultMetadata,
		IErrorMetadata errorMetadata)
	{
		var errorElement = new XElement(
			"error",
			new XAttribute("type", type),
			CreateFailureElement(errorMetadata)
		);

		if (name is not null)
			errorElement.Add(new XAttribute("name", name));

		lock (resultMetadata.ErrorsElement.Value)
			resultMetadata.ErrorsElement.Value.Add(errorElement);
	}

	static XElement CreateTestResultElement(
		ResultMetadata resultMetadata,
		ITestResultMessage testResult,
		string resultText)
	{
		var testMetadata = Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Cannot find test metadata for ID {0}", testResult.TestUniqueID), resultMetadata.MetadataCache.TryGetTestMetadata(testResult));
		var testStartTime = (testMetadata as ITestStarting)?.StartTime ?? testResult.FinishTime;
		var testCaseMetadata = Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Cannot find test case metadata for ID {0}", testResult.TestCaseUniqueID), resultMetadata.MetadataCache.TryGetTestCaseMetadata(testResult));
		var testMethodMetadata = resultMetadata.MetadataCache.TryGetMethodMetadata(testResult);
		var testClassMetadata = resultMetadata.MetadataCache.TryGetClassMetadata(testResult);

		var collectionElement = GetTestCollectionElement(resultMetadata, testResult.TestCollectionUniqueID);
		var testResultElement =
			new XElement("test",
				new XAttribute("id", Guid.NewGuid().ToString("d")),
				new XAttribute("name", XmlUtility.Escape(testMetadata.TestDisplayName)),
				new XAttribute("result", resultText),
				new XAttribute("time", testResult.ExecutionTime.ToString(CultureInfo.InvariantCulture)),
				new XAttribute("time-rtf", testResult.ExecutionTime.ToTimespanRtf()),
				new XAttribute("start-rtf", testStartTime.ToRtf()),
				new XAttribute("finish-rtf", testResult.FinishTime.ToRtf())
			);

		var type = testClassMetadata?.TestClassName;
		if (type is not null)
			testResultElement.Add(new XAttribute("type", type));

		var method = testMethodMetadata?.MethodName;
		if (method is not null)
			testResultElement.Add(new XAttribute("method", method));

		var testOutput = testResult.Output;
		if (!string.IsNullOrWhiteSpace(testOutput))
			testResultElement.Add(new XElement("output", AnsiUtility.RemoveAnsiEscapeCodes(testOutput)));

		if (testResult.Warnings is not null && testResult.Warnings.Length > 0)
		{
			var warningsElement = new XElement("warnings");

			foreach (var warning in testResult.Warnings)
				warningsElement.Add(new XElement("warning", warning));

			testResultElement.Add(warningsElement);
		}

		var fileName = testCaseMetadata.SourceFilePath;
		if (fileName is not null)
			testResultElement.Add(new XAttribute("source-file", fileName));

		var lineNumber = testCaseMetadata.SourceLineNumber;
		if (lineNumber is not null)
			testResultElement.Add(new XAttribute("source-line", lineNumber.GetValueOrDefault()));

		var traits = testCaseMetadata.Traits;
		if (traits is not null && traits.Count > 0)
		{
			var traitsElement = new XElement("traits");

			foreach (var keyValuePair in traits)
				foreach (var val in keyValuePair.Value)
					traitsElement.Add(
						new XElement("trait",
							new XAttribute("name", XmlUtility.Escape(keyValuePair.Key)),
							new XAttribute("value", XmlUtility.Escape(val))
						)
					);

			testResultElement.Add(traitsElement);
		}

		lock (collectionElement)
			collectionElement.Add(testResultElement);

		resultMetadata.TestResults[testResult.TestUniqueID] = testResultElement;
		return testResultElement;
	}

	internal override ResultMetadata CreateMetadata() =>
		new(AssembliesElement);

	internal override void FinalizeXml()
	{
		var start = default(string);
		var finish = default(string);

		var metadataValues = ResultMetadataValues;
		if (metadataValues.Count > 0)
		{
			start = metadataValues.Select(a => a.AssemblyElement.Attribute("start-rtf")?.Value).WhereNotNull().Min();
			finish = metadataValues.Select(a => a.AssemblyElement.Attribute("finish-rtf")?.Value).WhereNotNull().Max();
		}

		start ??= DateTimeOffset.MinValue.ToString("o", CultureInfo.InvariantCulture);
		finish ??= DateTimeOffset.MinValue.ToString("o", CultureInfo.InvariantCulture);

		var finishTimestamp = DateTime.Parse(finish, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);

		AssembliesElement.Add(
			new XAttribute("start-rtf", start),
			new XAttribute("finish-rtf", finish),
			new XAttribute("timestamp", finishTimestamp)
		);
	}

	static XElement GetTestCollectionElement(
		ResultMetadata resultMetadata,
		string testCollectionUniqueID) =>
			resultMetadata.TestCollections.GetOrAdd(testCollectionUniqueID, _ => new XElement("collection"));

	void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
	{
		var message = args.Message;
		if (message.AssemblyUniqueID is null || !TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		AddError("fatal", null, resultMetadata, message);
	}

	void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		AddError("assembly-cleanup", resultMetadata.MetadataCache.TryGetAssemblyMetadata(message)?.AssemblyPath, resultMetadata, message);
	}

	void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		AddError("test-case-cleanup", resultMetadata.MetadataCache.TryGetTestCaseMetadata(message)?.TestCaseDisplayName, resultMetadata, message);
	}

	void HandleTestClassCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		AddError("test-class-cleanup", resultMetadata.MetadataCache.TryGetClassMetadata(message)?.TestClassName, resultMetadata, message);
	}

	void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		AddError("test-cleanup", resultMetadata.MetadataCache.TryGetTestMetadata(message)?.TestDisplayName, resultMetadata, message);
	}

	void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		AddError("test-collection-cleanup", resultMetadata.MetadataCache.TryGetCollectionMetadata(message)?.TestCollectionDisplayName, resultMetadata, message);
	}

	void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		var testElement = CreateTestResultElement(resultMetadata, message, "Fail");
		testElement.Add(CreateFailureElement(message));
	}

	void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		AddError("test-method-cleanup", resultMetadata.MetadataCache.TryGetMethodMetadata(message)?.MethodName, resultMetadata, message);
	}

	void HandleTestNotRun(MessageHandlerArgs<ITestNotRun> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		CreateTestResultElement(resultMetadata, message, "NotRun");
	}

	void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		CreateTestResultElement(resultMetadata, message, "Pass");
	}

	void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		var testElement = CreateTestResultElement(resultMetadata, message, "Skip");
		testElement.Add(new XElement("reason", XmlUtility.Escape(message.Reason, escapeNewlines: false)));
	}

	/// <inheritdoc/>
	public override bool OnMessage(IMessageSinkMessage message)
	{
		message.DispatchWhen<IErrorMessage>(HandleErrorMessage);
		message.DispatchWhen<ITestAssemblyCleanupFailure>(HandleTestAssemblyCleanupFailure);
		message.DispatchWhen<ITestCaseCleanupFailure>(HandleTestCaseCleanupFailure);
		message.DispatchWhen<ITestClassCleanupFailure>(HandleTestClassCleanupFailure);
		message.DispatchWhen<ITestCleanupFailure>(HandleTestCleanupFailure);
		message.DispatchWhen<ITestCollectionCleanupFailure>(HandleTestCollectionCleanupFailure);
		message.DispatchWhen<ITestFailed>(HandleTestFailed);
		message.DispatchWhen<ITestMethodCleanupFailure>(HandleTestMethodCleanupFailure);
		message.DispatchWhen<ITestNotRun>(HandleTestNotRun);
		message.DispatchWhen<ITestPassed>(HandleTestPassed);
		message.DispatchWhen<ITestSkipped>(HandleTestSkipped);

		return base.OnMessage(message);
	}

	internal override void OnTestAssemblyFinished(
		ITestAssemblyFinished message,
		ResultMetadata resultMetadata)
	{
		lock (resultMetadata.AssemblyElement)
		{
			resultMetadata.AssemblyElement.Add(
				new XAttribute("errors", resultMetadata.ErrorsElement.IsValueCreated ? resultMetadata.ErrorsElement.Value.Elements().Count() : 0),
				new XAttribute("failed", message.TestsFailed),
				new XAttribute("finish-rtf", message.FinishTime.ToRtf()),
				new XAttribute("not-run", message.TestsNotRun),
				new XAttribute("passed", message.TestsTotal - message.TestsFailed - message.TestsSkipped - message.TestsNotRun),
				new XAttribute("skipped", message.TestsSkipped),
				new XAttribute("time", message.ExecutionTime.ToString("0.000", CultureInfo.InvariantCulture)),
				new XAttribute("time-rtf", TimeSpan.FromSeconds((double)message.ExecutionTime).ToString("c", CultureInfo.InvariantCulture)),
				new XAttribute("total", message.TestsTotal)
			);

			foreach (var element in resultMetadata.TestCollections.Values)
				resultMetadata.AssemblyElement.Add(element);
		}
	}

	internal override void OnTestAssemblyStarting(
		ITestAssemblyStarting message,
		ResultMetadata resultMetadata)
	{
		lock (resultMetadata.AssemblyElement)
		{
			resultMetadata.AssemblyElement.Add(
				new XAttribute("environment", message.TestEnvironment),
				new XAttribute("id", Guid.NewGuid().ToString("d")),
				new XAttribute("name", message.AssemblyPath ?? "<dynamic>"),
				new XAttribute("run-date", message.StartTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
				new XAttribute("run-time", message.StartTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture)),
				new XAttribute("start-rtf", message.StartTime.ToRtf()),
				new XAttribute("test-framework", message.TestFrameworkDisplayName)
			);

			if (message.ConfigFilePath is not null)
				resultMetadata.AssemblyElement.Add(new XAttribute("config-file", message.ConfigFilePath));
			if (message.TargetFramework is not null)
				resultMetadata.AssemblyElement.Add(new XAttribute("target-framework", message.TargetFramework));
		}
	}

	internal override void OnTestCollectionFinished(
		ITestCollectionFinished message,
		ResultMetadata resultMetadata)
	{
		var collectionElement = GetTestCollectionElement(resultMetadata, message.TestCollectionUniqueID);

		lock (collectionElement)
			collectionElement.Add(
				new XAttribute("failed", message.TestsFailed),
				new XAttribute("not-run", message.TestsNotRun),
				new XAttribute("passed", message.TestsTotal - message.TestsFailed - message.TestsSkipped - message.TestsNotRun),
				new XAttribute("skipped", message.TestsSkipped),
				new XAttribute("time", message.ExecutionTime.ToString("0.000", CultureInfo.InvariantCulture)),
				new XAttribute("time-rtf", TimeSpan.FromSeconds((double)message.ExecutionTime).ToString("c", CultureInfo.InvariantCulture)),
				new XAttribute("total", message.TestsTotal)
			);
	}

	internal override void OnTestCollectionStarting(
		ITestCollectionStarting message,
		ResultMetadata resultMetadata)
	{
		var collectionElement = GetTestCollectionElement(resultMetadata, message.TestCollectionUniqueID);

		lock (collectionElement)
			collectionElement.Add(
				new XAttribute("name", XmlUtility.Escape(message.TestCollectionDisplayName)),
				new XAttribute("id", Guid.NewGuid().ToString("d"))
			);
	}

	internal override void OnTestFinished(
		ITestFinished message,
		ResultMetadata resultMetadata)
	{
		if (!resultMetadata.TestResults.TryRemove(message.TestUniqueID, out var testResultElement) || message.Attachments.Count == 0)
			return;

		var attachmentsElement = new XElement("attachments");

		foreach (var attachment in message.Attachments)
		{
			var attachmentElement = new XElement("attachment", new XAttribute("name", attachment.Key));
			if (attachment.Value.AttachmentType == TestAttachmentType.String)
				attachmentElement.Add(attachment.Value.AsString());
			else
			{
				var (byteArray, mediaType) = attachment.Value.AsByteArray();

				attachmentElement.Add(new XAttribute("media-type", mediaType));
				attachmentElement.SetValue(Convert.ToBase64String(byteArray));
			}

			attachmentsElement.Add(attachmentElement);
		}

		testResultElement.Add(attachmentsElement);
	}

	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
	public sealed class ResultMetadata : ResultMetadataBase
	{
		internal ResultMetadata(XElement assembliesElement)
		{
			AssemblyElement = new XElement("assembly");
			ErrorsElement = new(CreateErrors, isThreadSafe: false);

			Guard.ArgumentNotNull(assembliesElement).Add(AssemblyElement);
		}

		internal XElement AssemblyElement { get; }

		internal Lazy<XElement> ErrorsElement { get; }

		internal ConcurrentDictionary<string, XElement> TestCollections { get; } = [];

		internal ConcurrentDictionary<string, XElement> TestResults { get; } = [];

		XElement CreateErrors()
		{
			var result = new XElement("errors");
			AssemblyElement.Add(result);

			return result;
		}
	}
}
