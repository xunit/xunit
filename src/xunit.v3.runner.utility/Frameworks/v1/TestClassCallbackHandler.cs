#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v1
{
	/// <summary>
	/// A handler that dispatches v1 Executor messages from running a test class.
	/// </summary>
	public class TestClassCallbackHandler : XmlNodeCallbackHandler
	{
		volatile int currentTestIndex = 0;
		readonly Dictionary<string, Predicate<XmlNode>> handlers;
		readonly _IMessageSink messageSink;
		readonly IList<Xunit1TestCase> testCases;
		readonly Xunit1RunSummary testCaseResults = new Xunit1RunSummary();
		readonly Xunit1RunSummary testMethodResults = new Xunit1RunSummary();

		Xunit1TestCase? lastTestCase;
		bool startSeen;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestClassCallbackHandler" /> class.
		/// </summary>
		/// <param name="testCases">The test cases that are being run.</param>
		/// <param name="messageSink">The message sink to call with the translated results.</param>
		public TestClassCallbackHandler(
			IList<Xunit1TestCase> testCases,
			_IMessageSink messageSink)
				: base(lastNodeName: "class")
		{
			Guard.ArgumentNotNull(nameof(testCases), testCases);
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);

			this.messageSink = messageSink;
			this.testCases = testCases;

			handlers = new Dictionary<string, Predicate<XmlNode>> {
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
			if ((failureNode = xml.SelectSingleNode("failure")) != null)
			{
				var errorMetadata = Xunit1ExceptionUtility.ConvertToErrorMetadata(failureNode);
				var errorMessage = new _ErrorMessage
				{
					ExceptionParentIndices = errorMetadata.ExceptionParentIndices,
					ExceptionTypes = errorMetadata.ExceptionTypes,
					Messages = errorMetadata.Messages,
					StackTraces = errorMetadata.StackTraces
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
			if (testCase != null)
			{
				startSeen = true;
				Interlocked.Increment(ref currentTestIndex);
				var testDisplayName = xml.Attributes?["name"]?.Value ?? $"{typeName}.{methodName}";
				result = messageSink.OnMessage(ToTestStarting(testCase, testDisplayName)) && result;
			}

			return result;
		}

		bool OnTest(XmlNode xml)
		{
			var @continue = TestClassResults.Continue;
			var typeName = xml.Attributes?["type"]?.Value;
			var methodName = xml.Attributes?["method"]?.Value;
			var testCase = FindTestCase(typeName, methodName);

			if (testCase != null)
			{
				var time = decimal.Parse(xml.Attributes?["time"]?.Value ?? "0", CultureInfo.InvariantCulture);
				var outputElement = xml.SelectSingleNode("output");
				var output = outputElement == null ? string.Empty : outputElement.InnerText;
				_MessageSinkMessage? resultMessage = null;

				// There is no <start> node for skipped tests, or with xUnit prior to v1.1
				if (!startSeen)
					OnStart(xml);

				testCaseResults.Total++;
				testCaseResults.Time += time;

				switch (xml.Attributes?["result"]?.Value)
				{
					case "Pass":
						resultMessage = ToTestPassed(testCase, time, output);
						break;

					case "Fail":
						testCaseResults.Failed++;
						var failureNode = xml.SelectSingleNode("failure");
						if (failureNode != null)
							resultMessage = ToTestFailed(testCase, time, output, failureNode);
						break;

					case "Skip":
						testCaseResults.Skipped++;
						resultMessage = ToTestSkipped(testCase, xml.SelectSingleNode("reason/message")?.InnerText ?? "<unknown skip reason>");
						break;
				}

				// Since we don't get live output from xUnit.net v1, we just send a single output message just before
				// the result message (if there was any output).
				if (!string.IsNullOrEmpty(output))
					@continue = messageSink.OnMessage(ToTestOutput(testCase, output)) && @continue;

				if (resultMessage != null)
					@continue = messageSink.OnMessage(resultMessage) && @continue;

				@continue = messageSink.OnMessage(ToTestFinished(testCase, time, output)) && @continue;
				startSeen = false;
			}

			return @continue;
		}

		/// <inheritdoc/>
		public override bool OnXmlNode(XmlNode? node)
		{
			if (node != null)
				if (handlers.TryGetValue(node.Name, out var handler))
					TestClassResults.Continue = handler(node) && TestClassResults.Continue;

			return TestClassResults.Continue;
		}

		void SendTestCaseMessagesWhenAppropriate(Xunit1TestCase? current)
		{
			var results = TestClassResults;

			if (current != lastTestCase && lastTestCase != null)
			{
				var testCaseFinished = new _TestCaseFinished
				{
					AssemblyUniqueID = lastTestCase.AssemblyUniqueID,
					ExecutionTime = testCaseResults.Time,
					TestCaseUniqueID = lastTestCase.TestCaseUniqueID,
					TestClassUniqueID = lastTestCase.TestClassUniqueID,
					TestCollectionUniqueID = lastTestCase.TestCollectionUniqueID,
					TestMethodUniqueID = lastTestCase.TestMethodUniqueID,
					TestsFailed = testCaseResults.Failed,
					TestsRun = testCaseResults.Total,
					TestsSkipped = testCaseResults.Skipped
				};

				results.Continue = messageSink.OnMessage(testCaseFinished) && results.Continue;
				testMethodResults.Aggregate(testCaseResults);
				testCaseResults.Reset();

				if (current == null || lastTestCase.TestMethod != current.TestMethod)
				{
					var testMethodFinished = new _TestMethodFinished
					{
						AssemblyUniqueID = lastTestCase.AssemblyUniqueID,
						ExecutionTime = testMethodResults.Time,
						TestClassUniqueID = lastTestCase.TestClassUniqueID,
						TestCollectionUniqueID = lastTestCase.TestCollectionUniqueID,
						TestMethodUniqueID = lastTestCase.TestMethodUniqueID,
						TestsFailed = testMethodResults.Failed,
						TestsRun = testMethodResults.Total,
						TestsSkipped = testMethodResults.Skipped
					};

					results.Continue = messageSink.OnMessage(testMethodFinished) && results.Continue;

					testMethodResults.Reset();
				}
			}

			if (current != lastTestCase && current != null)
			{
				// Dispatch TestMethodStarting if we've moved onto a new method
				if (lastTestCase == null || lastTestCase.TestMethod != current.TestMethod)
				{
					var testMethodStarting = new _TestMethodStarting
					{
						AssemblyUniqueID = current.AssemblyUniqueID,
						TestClassUniqueID = current.TestClassUniqueID,
						TestCollectionUniqueID = current.TestCollectionUniqueID,
						TestMethod = current.TestMethod,
						TestMethodUniqueID = current.TestMethodUniqueID
					};
					results.Continue = messageSink.OnMessage(testMethodStarting) && results.Continue;
				}

				// Dispatch TestCaseStarting
				var testCaseStarting = new _TestCaseStarting
				{
					AssemblyUniqueID = current.AssemblyUniqueID,
					SkipReason = current.SkipReason,
					SourceFilePath = current.SourceFilePath,
					SourceLineNumber = current.SourceLineNumber,
					TestCaseDisplayName = current.TestCaseDisplayName,
					TestCaseUniqueID = current.TestCaseUniqueID,
					TestClassUniqueID = current.TestClassUniqueID,
					TestCollectionUniqueID = current.TestCollectionUniqueID,
					TestMethodUniqueID = current.TestMethodUniqueID,
					Traits = current.Traits
				};

				results.Continue = messageSink.OnMessage(testCaseStarting) && results.Continue;
			}

			lastTestCase = current;
		}

		_TestFailed ToTestFailed(
			Xunit1TestCase testCase,
			decimal executionTime,
			string output,
			XmlNode failure)
		{
			var errorMetadata = Xunit1ExceptionUtility.ConvertToErrorMetadata(failure);

			return new _TestFailed
			{
				AssemblyUniqueID = testCase.AssemblyUniqueID,
				ExceptionParentIndices = errorMetadata.ExceptionParentIndices,
				ExceptionTypes = errorMetadata.ExceptionTypes,
				ExecutionTime = executionTime,
				Messages = errorMetadata.Messages,
				Output = output,
				StackTraces = errorMetadata.StackTraces,
				TestCaseUniqueID = testCase.TestCaseUniqueID,
				TestClassUniqueID = testCase.TestClassUniqueID,
				TestCollectionUniqueID = testCase.TestCollectionUniqueID,
				TestMethodUniqueID = testCase.TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(testCase.TestCaseUniqueID, currentTestIndex)
			};
		}

		_TestFinished ToTestFinished(
			Xunit1TestCase testCase,
			decimal executionTime,
			string output)
		{
			return new _TestFinished
			{
				AssemblyUniqueID = testCase.AssemblyUniqueID,
				ExecutionTime = executionTime,
				Output = output,
				TestCaseUniqueID = testCase.TestCaseUniqueID,
				TestClassUniqueID = testCase.TestClassUniqueID,
				TestCollectionUniqueID = testCase.TestCollectionUniqueID,
				TestMethodUniqueID = testCase.TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(testCase.TestCaseUniqueID, currentTestIndex)
			};
		}

		_TestOutput ToTestOutput(
			Xunit1TestCase testCase,
			string output)
		{
			return new _TestOutput
			{
				AssemblyUniqueID = testCase.AssemblyUniqueID,
				Output = output,
				TestCaseUniqueID = testCase.TestCaseUniqueID,
				TestClassUniqueID = testCase.TestClassUniqueID,
				TestCollectionUniqueID = testCase.TestCollectionUniqueID,
				TestMethodUniqueID = testCase.TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(testCase.TestCaseUniqueID, currentTestIndex)
			};
		}

		_TestPassed ToTestPassed(
			Xunit1TestCase testCase,
			decimal executionTime,
			string output)
		{
			return new _TestPassed
			{
				AssemblyUniqueID = testCase.AssemblyUniqueID,
				ExecutionTime = executionTime,
				Output = output,
				TestCaseUniqueID = testCase.TestCaseUniqueID,
				TestClassUniqueID = testCase.TestClassUniqueID,
				TestCollectionUniqueID = testCase.TestCollectionUniqueID,
				TestMethodUniqueID = testCase.TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(testCase.TestCaseUniqueID, currentTestIndex)
			};
		}

		_TestSkipped ToTestSkipped(
			Xunit1TestCase testCase,
			string reason)
		{
			return new _TestSkipped
			{
				AssemblyUniqueID = testCase.AssemblyUniqueID,
				ExecutionTime = 0m,
				Output = "",
				Reason = reason,
				TestCaseUniqueID = testCase.TestCaseUniqueID,
				TestClassUniqueID = testCase.TestClassUniqueID,
				TestCollectionUniqueID = testCase.TestCollectionUniqueID,
				TestMethodUniqueID = testCase.TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(testCase.TestCaseUniqueID, currentTestIndex)
			};
		}

		_TestStarting ToTestStarting(
			Xunit1TestCase testCase,
			string testDisplayName)
		{
			return new _TestStarting
			{
				AssemblyUniqueID = testCase.AssemblyUniqueID,
				TestCaseUniqueID = testCase.TestCaseUniqueID,
				TestClassUniqueID = testCase.TestClassUniqueID,
				TestCollectionUniqueID = testCase.TestCollectionUniqueID,
				TestDisplayName = testDisplayName,
				TestMethodUniqueID = testCase.TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(testCase.TestCaseUniqueID, currentTestIndex)
			};
		}
	}
}

#endif
