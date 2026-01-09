using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// The message handler for <see cref="JUnitResultWriter"/>.
/// </summary>
/// <remarks>
/// Uses the schema from <see href="https://github.com/junit-team/junit-framework/blob/4e775a88e4171749194ed87e3d58406d62e80a3f/platform-tests/src/test/resources/jenkins-junit.xsd"/>.
/// </remarks>
public class JUnitResultWriterMessageHandler : ResultMetadataMessageHandlerBase<JUnitResultWriterMessageHandler.ResultMetadata>, IResultWriterMessageHandler
{
	bool disposed;
	readonly XElement testSuitesElement;
	DateTimeOffset? timeStart;
	readonly ExecutionSummary totals = new();
	readonly Lazy<XmlWriter> xmlWriter;

	/// <summary>
	/// Initializes a new instance of the <see cref="JUnitResultWriterMessageHandler"/> class.
	/// </summary>
	/// <param name="fileName">The output file name</param>
	public JUnitResultWriterMessageHandler(string fileName) :
		this(new Lazy<XmlWriter>(() => XmlWriter.Create(fileName, XmlUtility.WriterSettings), isThreadSafe: false))
	{ }

	/// <summary>
	/// This constructor is for testing purposes only. Please call the public constructor.
	/// </summary>
	protected JUnitResultWriterMessageHandler(XmlWriter xmlWriter) :
		this(new Lazy<XmlWriter>(() => xmlWriter))
	{ }

	JUnitResultWriterMessageHandler(Lazy<XmlWriter> xmlWriter)
	{
		this.xmlWriter = xmlWriter;

		testSuitesElement =
			new(
				"testsuites",
				new XAttribute("name", "Test results")
			);
	}

	internal override ResultMetadata CreateMetadata() =>
		new(new XElement("testsuite"));

	/// <inheritdoc/>
	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		if (!disposed)
			try
			{
				testSuitesElement.Add(
					new XAttribute("tests", totals.Total),
					new XAttribute("failures", totals.Failed),
					new XAttribute("errors", totals.Errors),
					// The lack of "skipped" at the "testsuites" level is an odd omission, so we assume anybody
					// who wants to know the total number of "skipped" tests will have to visit the "testsuite" nodes
					// and get a combined count on their own.
					new XAttribute("disabled", totals.NotRun),
					new XAttribute("time", totals.Time.ToString("0.000000", CultureInfo.InvariantCulture)),
					new XAttribute("timestamp", (timeStart ?? DateTimeOffset.MinValue).ToUniversalTime().ToString("s", CultureInfo.InvariantCulture))
				);

				testSuitesElement.Save(xmlWriter.Value);
				xmlWriter.Value.SafeDispose();
			}
			finally
			{
				disposed = true;
			}

		return default;
	}

	void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
	{
		var message = args.Message;
		if (message.AssemblyUniqueID is null || !TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		lock (resultMetadata)
			resultMetadata.Errors.Add((ExceptionUtility.CombineMessages(message), ExceptionUtility.CombineStackTraces(message)));
	}

	void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
	{
		var message = args.Message;
		var testCaseElement = RecordTestCase(message);

		if (testCaseElement is not null)
		{
			var failureElement = new XElement("failure", new XAttribute("message", ExceptionUtility.CombineMessages(message)));
			var stackTrace = ExceptionUtility.CombineStackTraces(message);
			if (!string.IsNullOrWhiteSpace(stackTrace))
				failureElement.Add(stackTrace);

			testCaseElement.AddFirst(failureElement);
		}
	}

	void HandleTestPassed(MessageHandlerArgs<ITestPassed> args) =>
		RecordTestCase(args.Message);

	void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
	{
		var message = args.Message;
		var testCaseElement = RecordTestCase(message);

		testCaseElement?.AddFirst(new XElement("skipped", message.Reason));
	}

	/// <inheritdoc/>
	public override bool OnMessage(IMessageSinkMessage message)
	{
		if (message is ITestAssemblyCleanupFailure ||
				message is ITestCollectionCleanupFailure ||
				message is ITestClassCleanupFailure ||
				message is ITestMethodCleanupFailure ||
				message is ITestCaseCleanupFailure ||
				message is ITestCleanupFailure)
			OnErrorMetadata((ITestAssemblyMessage)message, (IErrorMetadata)message);

		message.DispatchWhen<IErrorMessage>(HandleErrorMessage);
		message.DispatchWhen<ITestFailed>(HandleTestFailed);
		message.DispatchWhen<ITestPassed>(HandleTestPassed);
		message.DispatchWhen<ITestSkipped>(HandleTestSkipped);

		return base.OnMessage(message);
	}

	void OnErrorMetadata(
		ITestAssemblyMessage assemblyMessage,
		IErrorMetadata metadata)
	{
		if (!TryGetResultMetadata(assemblyMessage.AssemblyUniqueID, out var resultMetadata))
			return;

		lock (resultMetadata)
			resultMetadata.Errors.Add((ExceptionUtility.CombineMessages(metadata), ExceptionUtility.CombineStackTraces(metadata)));
	}

	internal override void OnTestAssemblyFinished(
		ITestAssemblyFinished finished,
		ResultMetadata resultMetadata)
	{
		if (resultMetadata.MetadataCache.TryGetAssemblyMetadata(finished.AssemblyUniqueID) is not ITestAssemblyStarting starting)
			return;

		if (!timeStart.HasValue || timeStart > starting.StartTime)
			timeStart = starting.StartTime;

		lock (testSuitesElement)
		{
			resultMetadata.Element.Add(
				new XAttribute("name", starting.AssemblyPath),
				new XAttribute("tests", finished.TestsTotal),
				new XAttribute("failures", finished.TestsFailed),
				new XAttribute("errors", resultMetadata.Errors.Count),
				new XAttribute("disabled", finished.TestsNotRun),
				new XAttribute("skipped", finished.TestsSkipped),
				new XAttribute("time", finished.ExecutionTime.ToString("0.000000", CultureInfo.InvariantCulture)),
				new XAttribute("timestamp", starting.StartTime.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture)),
				new XAttribute("hostname", EnvironmentUtility.Computer ?? "localhost")
			);

			if (resultMetadata.Errors.Count != 0)
			{
				var systemErr =
					"One or more exceptions occurred during cleanup:\r\n\r\n" +
					string.Join("\r\n\r\n", resultMetadata.Errors.Select(a => $"{a.Message}{(string.IsNullOrWhiteSpace(a.StackTrace) ? "" : "\r\n" + a.StackTrace)}"));

				resultMetadata.Element.Add(new XElement("system-err", systemErr));
			}

			testSuitesElement.Add(resultMetadata.Element);

			totals.Errors += resultMetadata.Errors.Count;
			totals.Failed += finished.TestsFailed;
			totals.NotRun += finished.TestsNotRun;
			totals.Skipped += finished.TestsSkipped;
			totals.Time += finished.ExecutionTime;
			totals.Total += finished.TestsTotal;
		}
	}

	XElement? RecordTestCase(ITestResultMessage testResult)
	{
		if (!TryGetResultMetadata(testResult.AssemblyUniqueID, out var resultMetadata))
			return null;

		var test = resultMetadata.MetadataCache.TryGetTestMetadata(testResult);
		if (test is null)
			return null;

		var testClass = resultMetadata.MetadataCache.TryGetClassMetadata(testResult);

		var testCaseElement =
			new XElement("testcase",
				new XAttribute("name", test.TestDisplayName),
				new XAttribute("time", testResult.ExecutionTime.ToString("0.000000", CultureInfo.InvariantCulture))
			);

		if (testClass?.TestClassName is not null)
			testCaseElement.Add(new XAttribute("classname", testClass.TestClassName));

		var output = default(string);

		if (!string.IsNullOrWhiteSpace(testResult.Output))
			output = "<<< Test output >>>\r\n\r\n" + testResult.Output.TrimEnd('\r', '\n');

		if (testResult.Warnings is not null && testResult.Warnings.Length != 0)
		{
			if (output is null)
				output = string.Empty;
			else
				output += "\r\n\r\n";

			output += "<<< Warnings >>>\r\n\r\n" + string.Join("\r\n\r\n", testResult.Warnings.Select((warning, idx) => $"{idx + 1}. {warning}"));
		}

		if (output is not null)
			testCaseElement.Add(new XElement("system-out", output));

		lock (resultMetadata)
			resultMetadata.Element.Add(testCaseElement);

		return testCaseElement;
	}

	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
	public class ResultMetadata(XElement testSuiteElement) : ResultMetadataBase
	{
		internal XElement Element => testSuiteElement;

		internal readonly List<(string Message, string? StackTrace)> Errors = [];
	}
}
