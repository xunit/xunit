#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml;
using Xunit.Abstractions;
using Xunit.Internal;
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
		readonly IList<_ITestCase> testCases;
		readonly Xunit1RunSummary testCaseResults = new Xunit1RunSummary();
		readonly Xunit1RunSummary testMethodResults = new Xunit1RunSummary();

		_ITest? currentTest;
		_ITestCase? lastTestCase;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestClassCallbackHandler" /> class.
		/// </summary>
		/// <param name="testCases">The test cases that are being run.</param>
		/// <param name="messageSink">The message sink to call with the translated results.</param>
		public TestClassCallbackHandler(
			IList<_ITestCase> testCases,
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

		_ITestCase FindTestCase(string typeName, string methodName) =>
			testCases
				.FirstOrDefault(tc => tc.TestMethod.TestClass.Class.Name == typeName && tc.TestMethod.Method.Name == methodName);

		bool OnClass(XmlNode xml)
		{
			SendTestCaseMessagesWhenAppropriate(null);

			var @continue = true;
			XmlNode failureNode;
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

			TestClassResults.Time = decimal.Parse(xml.Attributes["time"].Value, CultureInfo.InvariantCulture);
			TestClassResults.Total = int.Parse(xml.Attributes["total"].Value, CultureInfo.InvariantCulture);
			TestClassResults.Failed = int.Parse(xml.Attributes["failed"].Value, CultureInfo.InvariantCulture);
			TestClassResults.Skipped = int.Parse(xml.Attributes["skipped"].Value, CultureInfo.InvariantCulture);
			return @continue && TestClassResults.Continue;
		}

		bool OnStart(XmlNode xml)
		{
			var testCase = FindTestCase(xml.Attributes["type"].Value, xml.Attributes["method"].Value);
			currentTest = new Xunit1Test(testCase, xml.Attributes["name"].Value, Interlocked.Increment(ref currentTestIndex));
			SendTestCaseMessagesWhenAppropriate(testCase);
			return messageSink.OnMessage(ToTestStarting(currentTest)) && TestClassResults.Continue;
		}

		bool OnTest(XmlNode xml)
		{
			var @continue = true;
			var testCase = FindTestCase(xml.Attributes["type"].Value, xml.Attributes["method"].Value);
			var timeAttribute = xml.Attributes["time"];
			var time = timeAttribute == null ? 0M : decimal.Parse(timeAttribute.Value, CultureInfo.InvariantCulture);
			var outputElement = xml.SelectSingleNode("output");
			var output = outputElement == null ? string.Empty : outputElement.InnerText;
			_MessageSinkMessage? resultMessage = null;

			// There is no <start> node for skipped tests, or with xUnit prior to v1.1
			if (currentTest == null)
				currentTest = new Xunit1Test(testCase, xml.Attributes["name"].Value, Interlocked.Increment(ref currentTestIndex));

			testCaseResults.Total++;
			testCaseResults.Time += time;

			switch (xml.Attributes["result"].Value)
			{
				case "Pass":
					resultMessage = ToTestPassed(currentTest, time, output);
					break;

				case "Fail":
					testCaseResults.Failed++;
					resultMessage = ToTestFailed(currentTest, time, output, xml.SelectSingleNode("failure"));
					break;

				case "Skip":
					testCaseResults.Skipped++;
					// TODO: What's special about Skip here that we dispatch test case changes?
					if (testCase != lastTestCase)
					{
						SendTestCaseMessagesWhenAppropriate(testCase);
						@continue = messageSink.OnMessage(ToTestStarting(currentTest)) && @continue;
					}
					resultMessage = ToTestSkipped(currentTest, xml.SelectSingleNode("reason/message").InnerText);
					break;
			}

			// Since we don't get live output from xUnit.net v1, we just send a single output message just before
			// the result message (if there was any output).
			if (!string.IsNullOrEmpty(output))
				@continue = messageSink.OnMessage(ToTestOutput(currentTest, output)) && @continue;

			if (resultMessage != null)
				@continue = messageSink.OnMessage(resultMessage) && @continue;

			@continue = messageSink.OnMessage(ToTestFinished(currentTest, time, output)) && @continue;
			currentTest = null;

			return @continue && TestClassResults.Continue;
		}

		/// <inheritdoc/>
		public override bool OnXmlNode(XmlNode? node)
		{
			if (node != null)
				if (handlers.TryGetValue(node.Name, out var handler))
					TestClassResults.Continue = handler(node) && TestClassResults.Continue;

			return TestClassResults.Continue;
		}

		void SendTestCaseMessagesWhenAppropriate(_ITestCase? current)
		{
			var results = TestClassResults;

			if (current != lastTestCase && lastTestCase != null)
			{
				var assemblyUniqueID = GetAssemblyUniqueID(lastTestCase.TestMethod.TestClass.TestCollection.TestAssembly);
				var collectionUniqueID = GetCollectionUniqueID(assemblyUniqueID, lastTestCase.TestMethod.TestClass.TestCollection);
				var classUniqueID = GetClassUniqueID(collectionUniqueID, lastTestCase.TestMethod.TestClass);
				var methodUniqueID = GetMethodUniqueID(classUniqueID, lastTestCase.TestMethod);

				var testCaseFinished = new _TestCaseFinished
				{
					AssemblyUniqueID = assemblyUniqueID,
					ExecutionTime = testCaseResults.Time,
					TestCaseUniqueID = lastTestCase.UniqueID,
					TestClassUniqueID = classUniqueID,
					TestCollectionUniqueID = collectionUniqueID,
					TestMethodUniqueID = methodUniqueID,
					TestsFailed = testCaseResults.Failed,
					TestsRun = testCaseResults.Total,
					TestsSkipped = testCaseResults.Skipped
				};

				results.Continue = messageSink.OnMessage(testCaseFinished) && results.Continue;
				testMethodResults.Aggregate(testCaseResults);
				testCaseResults.Reset();

				if (current == null || lastTestCase.TestMethod.Method.Name != current.TestMethod.Method.Name)
				{
					var testMethodFinished = new _TestMethodFinished
					{
						AssemblyUniqueID = assemblyUniqueID,
						ExecutionTime = testMethodResults.Time,
						TestClassUniqueID = classUniqueID,
						TestCollectionUniqueID = collectionUniqueID,
						TestMethodUniqueID = methodUniqueID,
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
				var assemblyUniqueID = GetAssemblyUniqueID(current.TestMethod.TestClass.TestCollection.TestAssembly);
				var collectionUniqueID = GetCollectionUniqueID(assemblyUniqueID, current.TestMethod.TestClass.TestCollection);
				var classUniqueID = GetClassUniqueID(collectionUniqueID, current.TestMethod.TestClass);
				var methodUniqueID = GetMethodUniqueID(classUniqueID, current.TestMethod);

				// Dispatch TestMethodStarting if we've moved onto a new method
				if (lastTestCase == null || lastTestCase.TestMethod.Method.Name != current.TestMethod.Method.Name)
				{
					var testMethodStarting = new _TestMethodStarting
					{
						AssemblyUniqueID = assemblyUniqueID,
						TestClassUniqueID = classUniqueID,
						TestCollectionUniqueID = collectionUniqueID,
						TestMethod = current.TestMethod.Method.Name,
						TestMethodUniqueID = methodUniqueID
					};
					results.Continue = messageSink.OnMessage(testMethodStarting) && results.Continue;
				}

				// Dispatch TestCaseStarting
				var testCaseStarting = new _TestCaseStarting
				{
					AssemblyUniqueID = assemblyUniqueID,
					SkipReason = current.SkipReason,
					SourceFilePath = current.SourceInformation?.FileName,
					SourceLineNumber = current.SourceInformation?.LineNumber,
					TestCaseDisplayName = current.DisplayName,
					TestCaseUniqueID = current.UniqueID,
					TestClassUniqueID = classUniqueID,
					TestCollectionUniqueID = collectionUniqueID,
					TestMethodUniqueID = methodUniqueID,
					Traits = current.Traits
				};

				results.Continue = messageSink.OnMessage(testCaseStarting) && results.Continue;
			}

			lastTestCase = current;
		}

		// TODO: These conversions doesn't afford us much chance for caching, so this could be expensive, recomputing
		// unique IDs for every single test. Perhaps we can do a bit of shortcut here by caching the last computations
		// based on object identity, so that we don't have to recompute every time...?

		_TestFailed ToTestFailed(
			_ITest test,
			decimal executionTime,
			string output,
			XmlNode failure)
		{
			var testCase = test.TestCase;
			var assemblyUniqueID = GetAssemblyUniqueID(testCase.TestMethod.TestClass.TestCollection.TestAssembly);
			var collectionUniqueID = GetCollectionUniqueID(assemblyUniqueID, testCase.TestMethod.TestClass.TestCollection);
			var classUniqueID = GetClassUniqueID(collectionUniqueID, testCase.TestMethod.TestClass);
			var methodUniqueID = GetMethodUniqueID(classUniqueID, testCase.TestMethod);
			var errorMetadata = Xunit1ExceptionUtility.ConvertToErrorMetadata(failure);

			return new _TestFailed
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExceptionParentIndices = errorMetadata.ExceptionParentIndices,
				ExceptionTypes = errorMetadata.ExceptionTypes,
				ExecutionTime = executionTime,
				Messages = errorMetadata.Messages,
				Output = output,
				StackTraces = errorMetadata.StackTraces,
				TestCaseUniqueID = testCase.UniqueID,
				TestClassUniqueID = classUniqueID,
				TestCollectionUniqueID = collectionUniqueID,
				TestMethodUniqueID = methodUniqueID,
				TestUniqueID = test.UniqueID
			};
		}

		_TestFinished ToTestFinished(
			_ITest test,
			decimal executionTime,
			string output)
		{
			var testCase = test.TestCase;
			var assemblyUniqueID = GetAssemblyUniqueID(testCase.TestMethod.TestClass.TestCollection.TestAssembly);
			var collectionUniqueID = GetCollectionUniqueID(assemblyUniqueID, testCase.TestMethod.TestClass.TestCollection);
			var classUniqueID = GetClassUniqueID(collectionUniqueID, testCase.TestMethod.TestClass);
			var methodUniqueID = GetMethodUniqueID(classUniqueID, testCase.TestMethod);

			return new _TestFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = executionTime,
				Output = output,
				TestCaseUniqueID = testCase.UniqueID,
				TestClassUniqueID = classUniqueID,
				TestCollectionUniqueID = collectionUniqueID,
				TestMethodUniqueID = methodUniqueID,
				TestUniqueID = test.UniqueID
			};
		}

		_TestOutput ToTestOutput(
			_ITest test,
			string output)
		{
			var testCase = test.TestCase;
			var assemblyUniqueID = GetAssemblyUniqueID(testCase.TestMethod.TestClass.TestCollection.TestAssembly);
			var collectionUniqueID = GetCollectionUniqueID(assemblyUniqueID, testCase.TestMethod.TestClass.TestCollection);
			var classUniqueID = GetClassUniqueID(collectionUniqueID, testCase.TestMethod.TestClass);
			var methodUniqueID = GetMethodUniqueID(classUniqueID, testCase.TestMethod);

			return new _TestOutput
			{
				AssemblyUniqueID = assemblyUniqueID,
				Output = output,
				TestCaseUniqueID = testCase.UniqueID,
				TestClassUniqueID = classUniqueID,
				TestCollectionUniqueID = collectionUniqueID,
				TestMethodUniqueID = methodUniqueID,
				TestUniqueID = test.UniqueID
			};
		}

		_TestPassed ToTestPassed(
			_ITest test,
			decimal executionTime,
			string output)
		{
			var testCase = test.TestCase;
			var assemblyUniqueID = GetAssemblyUniqueID(testCase.TestMethod.TestClass.TestCollection.TestAssembly);
			var collectionUniqueID = GetCollectionUniqueID(assemblyUniqueID, testCase.TestMethod.TestClass.TestCollection);
			var classUniqueID = GetClassUniqueID(collectionUniqueID, testCase.TestMethod.TestClass);
			var methodUniqueID = GetMethodUniqueID(classUniqueID, testCase.TestMethod);

			return new _TestPassed
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = executionTime,
				Output = output,
				TestCaseUniqueID = testCase.UniqueID,
				TestClassUniqueID = classUniqueID,
				TestCollectionUniqueID = collectionUniqueID,
				TestMethodUniqueID = methodUniqueID,
				TestUniqueID = test.UniqueID
			};
		}

		_TestSkipped ToTestSkipped(
			_ITest test,
			string reason)
		{
			var testCase = test.TestCase;
			var assemblyUniqueID = GetAssemblyUniqueID(testCase.TestMethod.TestClass.TestCollection.TestAssembly);
			var collectionUniqueID = GetCollectionUniqueID(assemblyUniqueID, testCase.TestMethod.TestClass.TestCollection);
			var classUniqueID = GetClassUniqueID(collectionUniqueID, testCase.TestMethod.TestClass);
			var methodUniqueID = GetMethodUniqueID(classUniqueID, testCase.TestMethod);

			return new _TestSkipped
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = 0m,
				Output = "",
				Reason = reason,
				TestCaseUniqueID = testCase.UniqueID,
				TestClassUniqueID = classUniqueID,
				TestCollectionUniqueID = collectionUniqueID,
				TestMethodUniqueID = methodUniqueID,
				TestUniqueID = test.UniqueID
			};
		}

		_TestStarting ToTestStarting(_ITest test)
		{
			var testCase = test.TestCase;
			var assemblyUniqueID = GetAssemblyUniqueID(testCase.TestMethod.TestClass.TestCollection.TestAssembly);
			var collectionUniqueID = GetCollectionUniqueID(assemblyUniqueID, testCase.TestMethod.TestClass.TestCollection);
			var classUniqueID = GetClassUniqueID(collectionUniqueID, testCase.TestMethod.TestClass);
			var methodUniqueID = GetMethodUniqueID(classUniqueID, testCase.TestMethod);

			return new _TestStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCase.UniqueID,
				TestClassUniqueID = classUniqueID,
				TestCollectionUniqueID = collectionUniqueID,
				TestDisplayName = test.DisplayName,
				TestMethodUniqueID = methodUniqueID,
				TestUniqueID = test.UniqueID
			};
		}

		// TODO: These should go away once the test case object hierarchy includes UniqueID at all levels

		string GetAssemblyUniqueID(_ITestAssembly testAssembly) =>
			UniqueIDGenerator.ForAssembly(
				testAssembly.Assembly.Name,
				testAssembly.Assembly.AssemblyPath,
				testAssembly.ConfigFileName
			);

		string? GetClassUniqueID(
			string collectionUniqueID,
			_ITestClass testClass) =>
				UniqueIDGenerator.ForTestClass(collectionUniqueID, testClass.Class?.Name);

		string GetCollectionUniqueID(
			string assemblyUniqueID,
			_ITestCollection testCollection) =>
				UniqueIDGenerator.ForTestCollection(assemblyUniqueID, testCollection.DisplayName, testCollection.CollectionDefinition?.Name);

		string? GetMethodUniqueID(
			string? classUniqueID,
			_ITestMethod testMethod) =>
				UniqueIDGenerator.ForTestMethod(classUniqueID, testMethod.Method?.Name);
	}
}

#endif
