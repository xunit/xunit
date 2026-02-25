using Xunit.Sdk;

public partial class AcceptanceTestV3
{
	static ITestResultWithMetadata? TestResultFactory(
		ITestResultMessage result,
		List<IMessageSinkMessage> messages) =>
			result switch
			{
				ITestFailed failed => TestResultFactory(failed, messages, TestFailedWithMetadata.FromTestFailed),
				ITestNotRun notRun => TestResultFactory(notRun, messages, TestNotRunWithMetadata.FromTestNotRun),
				ITestPassed passed => TestResultFactory(passed, messages, TestPassedWithMetadata.FromTestPassed),
				ITestSkipped skipped => TestResultFactory(skipped, messages, TestSkippedWithMetadata.FromTestSkipped),
				_ => null,
			};

	// TODO: This should use a message metadata cache to speed up the lookups
	static ITestResultWithMetadata? TestResultFactory<TResult>(
		TResult result,
		List<IMessageSinkMessage> messages,
		Func<TResult, IAssemblyMetadata, ITestCollectionMetadata, ITestClassMetadata?, ITestMethodMetadata?, ITestCaseMetadata, ITestMetadata, ITestResultWithMetadata> factory)
			where TResult : ITestResultMessage
	{
		if (messages.OfType<IAssemblyMetadata>().FirstOrDefault(a => a.UniqueID == result.AssemblyUniqueID) is not IAssemblyMetadata testAssembly)
			return null;
		if (messages.OfType<ITestCollectionMetadata>().FirstOrDefault(tc => tc.UniqueID == result.TestCollectionUniqueID) is not ITestCollectionMetadata testCollection)
			return null;
		if (messages.OfType<ITestCaseMetadata>().FirstOrDefault(tc => tc.UniqueID == result.TestCaseUniqueID) is not ITestCaseMetadata testCase)
			return null;
		if (messages.OfType<ITestMetadata>().FirstOrDefault(t => t.UniqueID == result.TestUniqueID) is not ITestMetadata test)
			return null;

		var testClass = result.TestClassUniqueID is null ? null : messages.OfType<ITestClassMetadata>().FirstOrDefault(tc => tc.UniqueID == result.TestClassUniqueID);
		var testMethod = result.TestMethodUniqueID is null ? null : messages.OfType<ITestMethodMetadata>().FirstOrDefault(tm => tm.UniqueID == result.TestMethodUniqueID);

		return factory(result, testAssembly, testCollection, testClass, testMethod, testCase, test);
	}
}
