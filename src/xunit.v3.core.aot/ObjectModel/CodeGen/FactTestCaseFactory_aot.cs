using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="ICodeGenTestCaseFactory"/> for use by tests which
/// are decorated by <see cref="FactAttribute"/>.
/// </summary>
public class FactTestCaseFactory : TestCaseFactoryBase
{
	/// <summary>
	/// Gets the function which invokes the test method when the test runs.
	/// </summary>
	public required Func<object?, ValueTask> MethodInvoker { get; init; }

	/// <summary>
	/// This creates a single instance of <see cref="CodeGenTestCase"/>.
	/// </summary>
	/// <inheritdoc/>
	protected override async ValueTask<IReadOnlyCollection<ICodeGenTestCase>> GenerateTestCases(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		ICodeGenTestMethod testMethod,
		DisposalTracker disposalTracker,
		string displayName) =>
			[new CodeGenTestCase(
				disableParallelization: false,  // [Fact.DisableParallelization] is fed into the test method registration
				Explicit,
				SkipExceptions,
				SkipReason,
				SkipUnless,
				SkipWhen,
				Guard.ArgumentNotNull(testMethod).SourceFilePath,
				testMethod.SourceLineNumber,
				displayName,
				[async testCase => [GenerateTest(testCase, MethodInvoker)]],
				testMethod,
				Timeout,
				testMethod.Traits,
				UniqueIDGenerator.ForTestCase(testMethod.UniqueID, 0)
			)];
}
