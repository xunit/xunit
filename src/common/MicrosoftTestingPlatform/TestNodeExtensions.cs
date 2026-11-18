using Microsoft.Testing.Extensions.TrxReport.Abstractions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Messages;
using Microsoft.Testing.Platform.TestHost;
using Xunit.Sdk;

namespace Xunit.MicrosoftTestingPlatform;

internal static class TestNodeExtensions
{
	static string voidVSTestTypeName = typeof(void).ToVSTestTypeName();

	public static void AddMetadata(
		this TestNode result,
		ITestCaseMetadata testCase,
		string assemblyFullName,
		bool isTrxEnabled,
		ITestResultMessage? testResult = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null)
	{
		if (testCase.TestClassSimpleName is not null && testCase.TestMethodName is not null)
			result.Properties.Add(
				new TestMethodIdentifierProperty(
					assemblyFullName,
					testCase.TestClassNamespace ?? string.Empty,
					testCase.TestClassSimpleName,
					testCase.TestMethodName,
					testCase.TestMethodArity ?? 0,
					testCase.TestMethodParameterTypesVSTest ?? [],
					testCase.TestMethodReturnTypeVSTest ?? voidVSTestTypeName
				)
			);

		if (testResult is not null && testResult.Output.Length != 0)
			result.Properties.Add(new StandardOutputProperty(testResult.Output));

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

		if (!isTrxEnabled)
			return;

		result.Properties.Add(new TrxTestDefinitionName(testCase.TestCaseDisplayName));

		// Provide TestMethodIdentifierProperty synthetically per https://github.com/microsoft/testfx/pull/12466#issuecomment-6917139465
		if (testCase.TestMethodName is null)
			result.Properties.Add(
				new TestMethodIdentifierProperty(
					assemblyFullName,
					testCase.TestClassNamespace ?? string.Empty,
					testCase.TestClassSimpleName ?? testCase.UniqueID,
					testCase.UniqueID,
					0,
					[],
					voidVSTestTypeName
				)
			);

		// Always supply a "fully qualified type name" (perhaps synthetic) per https://github.com/microsoft/testfx/issues/12436
		// since xUnit.net does not require tests originate in .NET types
		result.Properties.Add(new TrxFullyQualifiedTypeNameProperty(testCase.TestClassName ?? testCase.UniqueID));

		if (traits.TryGetValue("category", out var values))
			result.Properties.Add(new TrxCategoriesProperty(values.ToArray()));

		List<string> stdOutMessages = [];

		if (testResult is not null && testResult.Output.Length != 0)
			stdOutMessages.Add(testResult.Output);

		if (testResult is ITestSkipped testSkipped)
			stdOutMessages.Add(testSkipped.Reason);

		if (stdOutMessages.Count > 0)
			result.Properties.Add(new TrxMessagesProperty(stdOutMessages.SelectArray(msg => new StandardOutputTrxMessage(msg))));

		if (testResult is ITestFailed testFailed)
			result.Properties.Add(new TrxExceptionProperty(ExceptionUtility.CombineMessages(testFailed), ExceptionUtility.CombineStackTraces(testFailed)));
	}

	public static void SendUpdate(
		this TestNode testNode,
		IDataProducer producer,
		SessionUid sessionUid,
		IMessageBus messageBus) =>
			messageBus.PublishAsync(producer, new TestNodeUpdateMessage(sessionUid, testNode)).SpinWait();
}
