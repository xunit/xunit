using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// A delegating implementation of <see cref="IExecutionSink"/> which is responsible for
	/// creating the xUnit.net v2/v3 XML output from the execution test results.
	/// </summary>
	public class DelegatingXmlCreationSink : IExecutionSink
	{
		readonly XElement assemblyElement;
		bool disposed;
		readonly XElement errorsElement;
		readonly IExecutionSink innerSink;
		readonly MessageMetadataCache metadataCache = new();
		readonly Dictionary<string, XElement> testCollectionElements = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegatingXmlCreationSink"/> class.
		/// </summary>
		/// <param name="innerSink"></param>
		/// <param name="assemblyElement"></param>
		public DelegatingXmlCreationSink(
			IExecutionSink innerSink,
			XElement assemblyElement)
		{
			Guard.ArgumentNotNull(nameof(innerSink), innerSink);
			Guard.ArgumentNotNull(nameof(assemblyElement), assemblyElement);

			this.innerSink = innerSink;
			this.assemblyElement = assemblyElement;

			errorsElement = new XElement("errors");
			assemblyElement.Add(errorsElement);
		}

		/// <inheritdoc/>
		public ExecutionSummary ExecutionSummary => innerSink.ExecutionSummary;

		/// <inheritdoc/>
		public ManualResetEvent Finished => innerSink.Finished;

		/// <inheritdoc/>
		public bool OnMessage(_MessageSinkMessage message)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			// Call the inner sink first, because we want to be able to depend on ExecutionSummary
			// being correctly filled out.
			var result = innerSink.OnMessage(message);

			return message.DispatchWhen<_ErrorMessage>(HandleErrorMessage)

				&& message.DispatchWhen<_TestAssemblyCleanupFailure>(HandleTestAssemblyCleanupFailure)
				&& message.DispatchWhen<_TestAssemblyFinished>(HandleTestAssemblyFinished)
				&& message.DispatchWhen<_TestAssemblyStarting>(HandleTestAssemblyStarting)

				&& message.DispatchWhen<_TestCaseCleanupFailure>(HandleTestCaseCleanupFailure)
				&& message.DispatchWhen<_TestCaseFinished>(HandleTestCaseFinished)
				&& message.DispatchWhen<_TestCaseStarting>(HandleTestCaseStarting)

				&& message.DispatchWhen<_TestClassCleanupFailure>(HandleTestClassCleanupFailure)
				&& message.DispatchWhen<_TestClassFinished>(HandleTestClassFinished)
				&& message.DispatchWhen<_TestClassStarting>(HandleTestClassStarting)

				&& message.DispatchWhen<_TestCollectionCleanupFailure>(HandleTestCollectionCleanupFailure)
				&& message.DispatchWhen<_TestCollectionFinished>(HandleTestCollectionFinished)
				&& message.DispatchWhen<_TestCollectionStarting>(HandleTestCollectionStarting)

				&& message.DispatchWhen<_TestMethodCleanupFailure>(HandleTestMethodCleanupFailure)
				&& message.DispatchWhen<_TestMethodFinished>(HandleTestMethodFinished)
				&& message.DispatchWhen<_TestMethodStarting>(HandleTestMethodStarting)

				&& message.DispatchWhen<_TestCleanupFailure>(HandleTestCleanupFailure)
				&& message.DispatchWhen<_TestFailed>(HandleTestFailed)
				&& message.DispatchWhen<_TestFinished>(HandleTestFinished)
				&& message.DispatchWhen<_TestPassed>(HandleTestPassed)
				&& message.DispatchWhen<_TestSkipped>(HandleTestSkipped)
				&& message.DispatchWhen<_TestStarting>(HandleTestStarting)

				&& result;
		}

		void AddError(
			string type,
			string? name,
			_IErrorMetadata errorMetadata)
		{
			var errorElement = new XElement("error", new XAttribute("type", type), CreateFailureElement(errorMetadata));
			if (name != null)
				errorElement.Add(new XAttribute("name", name));

			errorsElement.Add(errorElement);
		}

		static XElement CreateFailureElement(_IErrorMetadata errorMetadata) =>
			new(
				"failure",
				new XAttribute("exception-type", errorMetadata.ExceptionTypes[0] ?? "<unknown type>"),
				new XElement("message", new XCData(XmlEscape(ExceptionUtility.CombineMessages(errorMetadata)))),
				new XElement("stack-trace", new XCData(ExceptionUtility.CombineStackTraces(errorMetadata) ?? string.Empty))
			);

		XElement CreateTestResultElement(
			_TestResultMessage testResult,
			string resultText)
		{
			var testMetadata = Guard.NotNull($"Cannot find test metadata for ID {testResult.TestUniqueID}", metadataCache.TryGetTestMetadata(testResult));
			var testCaseMetadata = Guard.NotNull($"Cannot find test case metadata for ID {testResult.TestCaseUniqueID}", metadataCache.TryGetTestCaseMetadata(testResult));
			var testMethodMetadata = Guard.NotNull($"Cannot find test method metadata for ID {testResult.TestMethodUniqueID}", metadataCache.TryGetMethodMetadata(testResult));
			var testClassMetadata = Guard.NotNull($"Cannot find test class metadata for ID {testResult.TestClassUniqueID}", metadataCache.TryGetClassMetadata(testResult));

			var collectionElement = GetTestCollectionElement(testResult.TestCollectionUniqueID);
			var testResultElement =
				new XElement("test",
					new XAttribute("name", XmlEscape(testMetadata.TestDisplayName)),
					new XAttribute("type", testClassMetadata.TestClass),
					new XAttribute("method", testMethodMetadata.TestMethod),
					new XAttribute("time", testResult.ExecutionTime.ToString(CultureInfo.InvariantCulture)),
					new XAttribute("result", resultText)
				);

			var testOutput = testResult.Output;
			if (!string.IsNullOrWhiteSpace(testOutput))
				testResultElement.Add(new XElement("output", new XCData(testOutput)));

			var fileName = testCaseMetadata.SourceFilePath;
			if (fileName != null)
				testResultElement.Add(new XAttribute("source-file", fileName));

			var lineNumber = testCaseMetadata.SourceLineNumber;
			if (lineNumber != null)
				testResultElement.Add(new XAttribute("source-line", lineNumber.GetValueOrDefault()));

			var traits = testCaseMetadata.Traits;
			if (traits != null && traits.Count > 0)
			{
				var traitsElement = new XElement("traits");

				foreach (var keyValuePair in traits)
					foreach (var val in keyValuePair.Value)
						traitsElement.Add(
							new XElement("trait",
								new XAttribute("name", XmlEscape(keyValuePair.Key)),
								new XAttribute("value", XmlEscape(val))
							)
						);

				testResultElement.Add(traitsElement);
			}

			collectionElement.Add(testResultElement);

			return testResultElement;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			innerSink.Dispose();
		}

		XElement GetTestCollectionElement(string testCollectionUniqueID)
		{
			lock (testCollectionElements)
				return testCollectionElements.GetOrAdd(testCollectionUniqueID, () => new XElement("collection"));
		}

		void HandleErrorMessage(MessageHandlerArgs<_ErrorMessage> args)
			=> AddError("fatal", null, args.Message);

		void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<_TestAssemblyCleanupFailure> args)
		{
			var metadata = metadataCache.TryGetAssemblyMetadata(args.Message);

			if (metadata != null)
				AddError("assembly-cleanup", metadata.AssemblyPath, args.Message);
			else
				AddError("assembly-cleanup", "<unknown test assembly>", args.Message);
		}

		void HandleTestAssemblyFinished(MessageHandlerArgs<_TestAssemblyFinished> args)
		{
			assemblyElement.Add(
				new XAttribute("total", ExecutionSummary.Total),
				new XAttribute("passed", ExecutionSummary.Total - ExecutionSummary.Failed - ExecutionSummary.Skipped),
				new XAttribute("failed", ExecutionSummary.Failed),
				new XAttribute("skipped", ExecutionSummary.Skipped),
				new XAttribute("time", ExecutionSummary.Time.ToString("0.000", CultureInfo.InvariantCulture)),
				new XAttribute("errors", ExecutionSummary.Errors)
			);

			foreach (var element in testCollectionElements.Values)
				assemblyElement.Add(element);

			metadataCache.TryRemove(args.Message);
		}

		void HandleTestAssemblyStarting(MessageHandlerArgs<_TestAssemblyStarting> args)
		{
			var assemblyStarting = args.Message;
			assemblyElement.Add(
				new XAttribute("name", assemblyStarting.AssemblyPath ?? "<unknown assembly>"),
				new XAttribute("environment", assemblyStarting.TestEnvironment),
				new XAttribute("test-framework", assemblyStarting.TestFrameworkDisplayName),
				new XAttribute("run-date", assemblyStarting.StartTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
				new XAttribute("run-time", assemblyStarting.StartTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture))
			);

			if (assemblyStarting.ConfigFilePath != null)
				assemblyElement.Add(new XAttribute("config-file", assemblyStarting.ConfigFilePath));
			if (assemblyStarting.TargetFramework != null)
				assemblyElement.Add(new XAttribute("target-framework", assemblyStarting.TargetFramework));

			metadataCache.Set(assemblyStarting);
		}

		void HandleTestCaseCleanupFailure(MessageHandlerArgs<_TestCaseCleanupFailure> args)
		{
			var metadata = metadataCache.TryGetTestCaseMetadata(args.Message);
			if (metadata != null)
				AddError("test-case-cleanup", metadata.TestCaseDisplayName, args.Message);
			else
				AddError("test-case-cleanup", "<unknown test case>", args.Message);
		}

		void HandleTestCaseFinished(MessageHandlerArgs<_TestCaseFinished> args) =>
			metadataCache.TryRemove(args.Message);

		void HandleTestCaseStarting(MessageHandlerArgs<_TestCaseStarting> args) =>
			metadataCache.Set(args.Message);

		void HandleTestClassCleanupFailure(MessageHandlerArgs<_TestClassCleanupFailure> args)
		{
			var metadata = metadataCache.TryGetClassMetadata(args.Message);
			if (metadata != null)
				AddError("test-class-cleanup", metadata.TestClass, args.Message);
			else
				AddError("test-class-cleanup", "<unknown test class>", args.Message);
		}

		void HandleTestClassFinished(MessageHandlerArgs<_TestClassFinished> args) =>
			metadataCache.TryRemove(args.Message);

		void HandleTestClassStarting(MessageHandlerArgs<_TestClassStarting> args) =>
			metadataCache.Set(args.Message);

		void HandleTestCleanupFailure(MessageHandlerArgs<_TestCleanupFailure> args)
		{
			var metadata = metadataCache.TryGetTestMetadata(args.Message);
			if (metadata != null)
				AddError("test-cleanup", metadata.TestDisplayName, args.Message);
			else
				AddError("test-cleanup", "<unknown test>", args.Message);
		}

		void HandleTestCollectionCleanupFailure(MessageHandlerArgs<_TestCollectionCleanupFailure> args)
		{
			var metadata = metadataCache.TryGetCollectionMetadata(args.Message);
			if (metadata != null)
				AddError("test-collection-cleanup", metadata.TestCollectionDisplayName, args.Message);
			else
				AddError("test-collection-cleanup", "<unknown test collection>", args.Message);
		}

		void HandleTestCollectionFinished(MessageHandlerArgs<_TestCollectionFinished> args)
		{
			var testCollectionFinished = args.Message;
			var collectionElement = GetTestCollectionElement(testCollectionFinished.TestCollectionUniqueID);
			collectionElement.Add(
				new XAttribute("total", testCollectionFinished.TestsRun),
				new XAttribute("passed", testCollectionFinished.TestsRun - testCollectionFinished.TestsFailed - testCollectionFinished.TestsSkipped),
				new XAttribute("failed", testCollectionFinished.TestsFailed),
				new XAttribute("skipped", testCollectionFinished.TestsSkipped),
				new XAttribute("time", testCollectionFinished.ExecutionTime.ToString("0.000", CultureInfo.InvariantCulture))
			);

			metadataCache.TryRemove(testCollectionFinished);
		}

		void HandleTestCollectionStarting(MessageHandlerArgs<_TestCollectionStarting> args)
		{
			var testCollectionStarting = args.Message;
			var collectionElement = GetTestCollectionElement(testCollectionStarting.TestCollectionUniqueID);

			collectionElement.Add(new XAttribute("name", XmlEscape(testCollectionStarting.TestCollectionDisplayName)));

			metadataCache.Set(testCollectionStarting);
		}

		void HandleTestFailed(MessageHandlerArgs<_TestFailed> args)
		{
			var testFailed = args.Message;
			var testElement = CreateTestResultElement(testFailed, "Fail");
			testElement.Add(CreateFailureElement(testFailed));
		}

		void HandleTestFinished(MessageHandlerArgs<_TestFinished> args) =>
			metadataCache.TryRemove(args.Message);

		void HandleTestMethodCleanupFailure(MessageHandlerArgs<_TestMethodCleanupFailure> args)
		{
			var metadata = metadataCache.TryGetMethodMetadata(args.Message);
			if (metadata != null)
				AddError("test-method-cleanup", metadata.TestMethod, args.Message);
			else
				AddError("test-method-cleanup", "<unknown test method>", args.Message);
		}

		void HandleTestMethodFinished(MessageHandlerArgs<_TestMethodFinished> args) =>
			metadataCache.TryRemove(args.Message);

		void HandleTestMethodStarting(MessageHandlerArgs<_TestMethodStarting> args) =>
			metadataCache.Set(args.Message);

		void HandleTestPassed(MessageHandlerArgs<_TestPassed> args)
			=> CreateTestResultElement(args.Message, "Pass");

		void HandleTestSkipped(MessageHandlerArgs<_TestSkipped> args)
		{
			var testSkipped = args.Message;
			var testElement = CreateTestResultElement(testSkipped, "Skip");
			testElement.Add(new XElement("reason", new XCData(XmlEscape(testSkipped.Reason))));
		}

		void HandleTestStarting(MessageHandlerArgs<_TestStarting> args) =>
			metadataCache.Set(args.Message);

		/// <summary>
		/// Escapes a string for placing into the XML.
		/// </summary>
		/// <param name="value">The value to be escaped.</param>
		/// <returns>The escaped value.</returns>
		static string XmlEscape(string? value)
		{
			if (value == null)
				return string.Empty;

			value =
				value
					.Replace("\\", "\\\\")
					.Replace("\r", "\\r")
					.Replace("\n", "\\n")
					.Replace("\t", "\\t")
					.Replace("\0", "\\0")
					.Replace("\a", "\\a")
					.Replace("\b", "\\b")
					.Replace("\v", "\\v")
					.Replace("\"", "\\\"")
					.Replace("\f", "\\f");

			var escapedValue = new StringBuilder(value.Length);
			for (var idx = 0; idx < value.Length; ++idx)
			{
				var ch = value[idx];
				if (ch < 32)
					escapedValue.Append($@"\x{+ch:x2}");
				else if (char.IsSurrogatePair(value, idx)) // Takes care of the case when idx + 1 == value.Length
				{
					escapedValue.Append(ch); // Append valid surrogate chars like normal
					escapedValue.Append(value[++idx]);
				}
				// Check for invalid chars and append them like \x----
				else if (char.IsSurrogate(ch) || ch == '\uFFFE' || ch == '\uFFFF')
					escapedValue.Append($@"\x{+ch:x4}");
				else
					escapedValue.Append(ch);
			}

			return escapedValue.ToString();
		}
	}
}
