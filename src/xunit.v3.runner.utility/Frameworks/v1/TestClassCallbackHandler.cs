#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.v1;

/// <summary>
/// A handler that dispatches v1 Executor messages from running a test class.
/// </summary>
public class TestClassCallbackHandler : XmlNodeCallbackHandler
{
	volatile int currentTestIndex;
	readonly Dictionary<string, Predicate<XmlNode>> handlers;
	readonly IMessageSink messageSink;
	readonly IList<Xunit1TestCase> testCases;
	readonly Xunit1RunSummary testCaseResults = new();
	readonly Xunit1RunSummary testMethodResults = new();

	Xunit1TestCase? lastTestCase;
	bool startSeen;

	/// <summary>
	/// Initializes a new instance of the <see cref="TestClassCallbackHandler" /> class.
	/// </summary>
	/// <param name="testCases">The test cases that are being run.</param>
	/// <param name="messageSink">The message sink to call with the translated results.</param>
	public TestClassCallbackHandler(
		IList<Xunit1TestCase> testCases,
		IMessageSink messageSink)
			: base(lastNodeName: "class")
	{
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageSink);

		this.messageSink = messageSink;
		this.testCases = testCases;

		handlers = new Dictionary<string, Predicate<XmlNode>>
		{
			{ "class", OnClass },
			{ "start", OnStart },
			{ "test", OnTest }
		};

		TestClassResults = new Xunit1RunSummary();
	}

	/// <summary>
	/// Gets the test class results, after the execution has completed.
	/// </summary>
	public Xunit1RunSummary TestClassResults { get; }

	Xunit1TestCase? FindTestCase(string? typeName, string? methodName) =>
		testCases.FirstOrDefault(tc => tc.TestClass == typeName && tc.TestMethod == methodName);

	bool OnClass(XmlNode xml)
	{
		SendTestCaseMessagesWhenAppropriate(null);

		var @continue = true;
		XmlNode? failureNode;
		if ((failureNode = xml.SelectSingleNode("failure")) is not null)
		{
			var (exceptionTypes, messages, stackTraces, exceptionParentIndices) = Xunit1ExceptionUtility.ConvertToErrorMetadata(failureNode);
			var errorMessage = new ErrorMessage
			{
				ExceptionParentIndices = exceptionParentIndices,
				ExceptionTypes = exceptionTypes,
				Messages = messages,
				StackTraces = stackTraces
			};

			@continue = messageSink.OnMessage(errorMessage);
		}

		TestClassResults.Time = decimal.Parse(xml.Attributes?["time"]?.Value ?? "0", CultureInfo.InvariantCulture);
		TestClassResults.Total = int.Parse(xml.Attributes?["total"]?.Value ?? "0", CultureInfo.InvariantCulture);
		TestClassResults.Failed = int.Parse(xml.Attributes?["failed"]?.Value ?? "0", CultureInfo.InvariantCulture);
		TestClassResults.Skipped = int.Parse(xml.Attributes?["skipped"]?.Value ?? "0", CultureInfo.InvariantCulture);
		return @continue && TestClassResults.Continue;
	}

	bool OnStart(XmlNode xml)
	{
		var typeName = xml.Attributes?["type"]?.Value;
		var methodName = xml.Attributes?["method"]?.Value;
		var testCase = FindTestCase(typeName, methodName);

		SendTestCaseMessagesWhenAppropriate(testCase);

		var result = TestClassResults.Continue;
		if (testCase is not null)
		{
			startSeen = true;
			Interlocked.Increment(ref currentTestIndex);
			var testDisplayName = xml.Attributes?["name"]?.Value ?? string.Format(CultureInfo.InvariantCulture, "{0}.{1}", typeName, methodName);
			result = messageSink.OnMessage(testCase.ToTestStarting(testDisplayName, currentTestIndex)) && result;
		}

		return result;
	}

	bool OnTest(XmlNode xml)
	{
		var @continue = TestClassResults.Continue;
		var typeName = xml.Attributes?["type"]?.Value;
		var methodName = xml.Attributes?["method"]?.Value;
		var testCase = FindTestCase(typeName, methodName);

		if (testCase is not null)
		{
			var time = decimal.Parse(xml.Attributes?["time"]?.Value ?? "0", CultureInfo.InvariantCulture);
			var outputElement = xml.SelectSingleNode("output");
			var output = outputElement is null ? string.Empty : outputElement.InnerText;
			IMessageSinkMessage? resultMessage = null;

			// There is no <start> node for skipped tests, or with xUnit prior to v1.1
			if (!startSeen)
				OnStart(xml);

			testCaseResults.Total++;
			testCaseResults.Time += time;

			switch (xml.Attributes?["result"]?.Value)
			{
				case "Pass":
					resultMessage = testCase.ToTestPassed(time, output, currentTestIndex);
					break;

				case "Fail":
					testCaseResults.Failed++;
					var failureNode = xml.SelectSingleNode("failure");
					if (failureNode is not null)
						resultMessage = testCase.ToTestFailed(time, output, failureNode, currentTestIndex);
					break;

				case "Skip":
					testCaseResults.Skipped++;
					resultMessage = testCase.ToTestSkipped(xml.SelectSingleNode("reason/message")?.InnerText ?? "<unknown skip reason>", currentTestIndex);
					break;

				default:
					break;
			}

			// Since we don't get live output from xUnit.net v1, we just send a single output message just before
			// the result message (if there was any output).
			if (!string.IsNullOrEmpty(output))
				@continue = messageSink.OnMessage(testCase.ToTestOutput(output, currentTestIndex)) && @continue;

			if (resultMessage is not null)
				@continue = messageSink.OnMessage(resultMessage) && @continue;

			@continue = messageSink.OnMessage(testCase.ToTestFinished(time, output, currentTestIndex)) && @continue;
			startSeen = false;
		}

		return @continue;
	}

	/// <inheritdoc/>
	public override bool OnXmlNode(XmlNode? node)
	{
		if (node is not null)
			if (handlers.TryGetValue(node.Name, out var handler))
				TestClassResults.Continue = handler(node) && TestClassResults.Continue;

		return TestClassResults.Continue;
	}

	void SendTestCaseMessagesWhenAppropriate(Xunit1TestCase? current)
	{
		var results = TestClassResults;

		if (current != lastTestCase && lastTestCase is not null)
		{
			var testCaseFinished = lastTestCase.ToTestCaseFinished(testCaseResults);

			results.Continue = messageSink.OnMessage(testCaseFinished) && results.Continue;
			testMethodResults.Aggregate(testCaseResults);
			testCaseResults.Reset();

			if (current is null || lastTestCase.TestMethod != current.TestMethod)
			{
				var testMethodFinished = lastTestCase.ToTestMethodFinished(testMethodResults);
				results.Continue = messageSink.OnMessage(testMethodFinished) && results.Continue;

				testMethodResults.Reset();
			}
		}

		if (current != lastTestCase && current is not null)
		{
			// Dispatch TestMethodStarting if we've moved onto a new method
			if (lastTestCase is null || lastTestCase.TestMethod != current.TestMethod)
			{
				var testMethodStarting = current.ToTestMethodStarting();
				results.Continue = messageSink.OnMessage(testMethodStarting) && results.Continue;
			}

			// Dispatch TestCaseStarting
			var testCaseStarting = current.ToTestCaseStarting();
			results.Continue = messageSink.OnMessage(testCaseStarting) && results.Continue;
		}

		lastTestCase = current;
	}
}

#endif
