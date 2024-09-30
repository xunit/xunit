using System.Collections.Generic;
using System.Linq;
using Microsoft.Testing.Extensions.TrxReport.Abstractions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Messages;
using Microsoft.Testing.Platform.TestHost;
using Xunit.Sdk;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

internal static class TestNodeExtensions
{
	public static void AddMetadata(
		this TestNode result,
		ITestCaseMetadata testCase,
		string assemblyFullName,
		ITestResultMessage? testResult = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null)
	{
		if (testCase.TestClassName is not null && testCase.TestMethodName is not null)
			result.Properties.Add(
				new TestMethodIdentifierProperty(
					assemblyFullName,
					testCase.TestClassNamespace ?? string.Empty,
					testCase.TestClassName,
					testCase.TestMethodName,
					testCase.TestMethodParameterTypesVSTest ?? [],
					testCase.TestMethodReturnTypeVSTest ?? typeof(void).ToVSTestTypeName()
				)
			);

#pragma warning disable TPEXP  // StandardOutputProperty is experimental

		if (testResult is not null && testResult.Output.Length != 0)
			result.Properties.Add(new StandardOutputProperty(testResult.Output));

#pragma warning restore TPEXP

		var sourceFile = testCase.SourceFilePath;
		var sourceLine = testCase.SourceLineNumber;
		if (sourceFile is not null && sourceLine.HasValue)
		{
			var linePosition = new LinePosition(sourceLine.Value, -1);
			var span = new LinePositionSpan(linePosition, linePosition);
			result.Properties.Add(new TestFileLocationProperty(sourceFile, span));
		}

		traits ??= testCase.Traits;
		if (traits.Count != 0)
			foreach (var kvp in traits)
				foreach (var value in kvp.Value)
					result.Properties.Add(new TestMetadataProperty(kvp.Key, value));
	}

	public static void AddTrxMetadata(
		this TestNode result,
		ITestCaseMetadata testCase,
		ITestMessage testMessage,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits)
	{
		if (testCase.TestClassName is not null)
			result.Properties.Add(new TrxFullyQualifiedTypeNameProperty(testCase.TestClassName));

		if (traits.TryGetValue("category", out var values))
			result.Properties.Add(new TrxCategoriesProperty(values.ToArray()));

		List<string> stdOutMessages = [];

		if (testMessage is ITestResultMessage testResult && testResult.Output.Length != 0)
			stdOutMessages.Add(testResult.Output);

		if (testMessage is ITestSkipped testSkipped)
			stdOutMessages.Add(testSkipped.Reason);

		if (stdOutMessages.Count > 0)
			result.Properties.Add(new TrxMessagesProperty(stdOutMessages.Select(msg => new StandardOutputTrxMessage(msg)).ToArray()));

		if (testMessage is ITestFailed testFailed)
			result.Properties.Add(new TrxExceptionProperty(ExceptionUtility.CombineMessages(testFailed), ExceptionUtility.CombineStackTraces(testFailed)));
	}

	public static void SendUpdate(
		this TestNode testNode,
		IDataProducer producer,
		SessionUid sessionUid,
		IMessageBus messageBus) =>
			messageBus.PublishAsync(producer, new TestNodeUpdateMessage(sessionUid, testNode)).SpinWait();
}
