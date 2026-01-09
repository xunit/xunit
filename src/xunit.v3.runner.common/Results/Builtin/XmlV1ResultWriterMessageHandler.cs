using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// The message handler for <see cref="XmlV1ResultWriter"/>.
/// </summary>
public class XmlV1ResultWriterMessageHandler : XunitXmlResultWriterMessageHandlerBase<XmlV1ResultWriterMessageHandler.ResultMetadata>
{
	const string DefaultTestClassUniqueID = "<no-test-class>";

	/// <summary>
	/// Initializes a new instance of the <see cref="XmlV1ResultWriterMessageHandler"/> class.
	/// </summary>
	/// <param name="fileName">The output file name</param>
	public XmlV1ResultWriterMessageHandler(string fileName) :
		base(new Lazy<XmlWriter>(() => XmlWriter.Create(fileName, XmlUtility.WriterSettings), isThreadSafe: false))
	{ }

	/// <summary>
	/// This constructor is for testing purposes only. Please call the public constructor.
	/// </summary>
	protected XmlV1ResultWriterMessageHandler(XmlWriter xmlWriter) :
		base(new Lazy<XmlWriter>(() => xmlWriter))
	{ }

	internal override ResultMetadata CreateMetadata() =>
		new(AssembliesElement);

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

		var classElement = GetTestClassElement(resultMetadata, testResult.TestClassUniqueID ?? DefaultTestClassUniqueID);
		var testResultElement =
			new XElement("test",
				new XAttribute("name", XmlUtility.Escape(testMetadata.TestDisplayName)),
				new XAttribute("result", resultText),
				new XAttribute("time", testResult.ExecutionTime.ToString(CultureInfo.InvariantCulture))
			);

		var type = testClassMetadata?.TestClassName;
		if (type is not null)
			testResultElement.Add(new XAttribute("type", type));

		var method = testMethodMetadata?.MethodName;
		if (method is not null)
			testResultElement.Add(new XAttribute("method", method));

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

		lock (classElement)
			classElement.Add(testResultElement);

		return testResultElement;
	}

	static XElement GetTestClassElement(
		ResultMetadata resultMetadata,
		string testClassUniqueID) =>
			resultMetadata.TestClasses.GetOrAdd(testClassUniqueID, _ => new XElement("class"));

	void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		var testElement = CreateTestResultElement(resultMetadata, message, "Fail");
		testElement.Add(CreateFailureElement(message));
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
		testElement.Add(new XElement("reason", new XElement("message", XmlUtility.Escape(message.Reason, escapeNewlines: false))));
	}

	/// <inheritdoc/>
	public override bool OnMessage(IMessageSinkMessage message)
	{
		message.DispatchWhen<ITestFailed>(HandleTestFailed);
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
				new XAttribute("failed", message.TestsFailed),
				new XAttribute("passed", message.TestsTotal - message.TestsFailed - message.TestsNotRun - message.TestsSkipped),
				new XAttribute("skipped", message.TestsSkipped),
				new XAttribute("time", message.ExecutionTime.ToString("0.000", CultureInfo.InvariantCulture)),
				new XAttribute("total", message.TestsTotal)
			);

			foreach (var element in resultMetadata.TestClasses.Values)
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
				new XAttribute("name", message.AssemblyPath ?? "<dynamic>"),
				new XAttribute("run-date", message.StartTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
				new XAttribute("run-time", message.StartTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture)),
				new XAttribute("test-framework", message.TestFrameworkDisplayName)
			);

			if (message.ConfigFilePath is not null)
				resultMetadata.AssemblyElement.Add(new XAttribute("configFile", message.ConfigFilePath));
		}
	}

	internal override void OnTestClassFinished(
		ITestClassFinished message,
		ResultMetadata resultMetadata)
	{
		var classElement = GetTestClassElement(resultMetadata, message.TestClassUniqueID ?? DefaultTestClassUniqueID);

		lock (classElement)
			classElement.Add(
				new XAttribute("failed", message.TestsFailed),
				new XAttribute("passed", message.TestsTotal - message.TestsFailed - message.TestsSkipped - message.TestsNotRun),
				new XAttribute("skipped", message.TestsSkipped),
				new XAttribute("time", message.ExecutionTime.ToString("0.000", CultureInfo.InvariantCulture)),
				new XAttribute("total", message.TestsTotal)
			);
	}

	internal override void OnTestClassStarting(
		ITestClassStarting message,
		ResultMetadata resultMetadata)
	{
		var classElement = GetTestClassElement(resultMetadata, message.TestClassUniqueID ?? DefaultTestClassUniqueID);

		lock (classElement)
			classElement.Add(new XAttribute("name", XmlUtility.Escape(message.TestClassName)));
	}

	/// <summary/>
	public sealed class ResultMetadata : ResultMetadataBase
	{
		internal ResultMetadata(XElement assembliesElement)
		{
			AssemblyElement = new XElement("assembly");

			Guard.ArgumentNotNull(assembliesElement).Add(AssemblyElement);
		}

		internal XElement AssemblyElement { get; }

		internal ConcurrentDictionary<string, XElement> TestClasses { get; } = [];
	}
}
