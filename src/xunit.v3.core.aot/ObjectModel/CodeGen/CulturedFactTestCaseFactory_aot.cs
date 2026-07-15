using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="ICodeGenTestCaseFactory"/> for use by tests which
/// are decorated by <see cref="CulturedFactAttribute"/>.
/// </summary>
public class CulturedFactTestCaseFactory : FactTestCaseFactory
{
	/// <summary>
	/// Gets the cultures to be run.
	/// </summary>
	public required IReadOnlyCollection<string> Cultures { get; init; }

	/// <summary>
	/// Creates one test case per culture from <see cref="Cultures"/>.
	/// </summary>
	/// <remarks>
	/// The logic here follows much the same as <see cref="FactTestCaseFactory.GenerateTestCases"/> on a
	/// per culture basis.
	/// </remarks>
	/// <inheritdoc/>
	protected override async ValueTask<IReadOnlyCollection<ICodeGenTestCase>> GenerateTestCases(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		ICodeGenTestMethod testMethod,
		DisposalTracker disposalTracker,
		string displayName)
	{
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(displayName);

		return Cultures.Select(culture =>
			new CodeGenTestCase(
				disableParallelization: false,  // [CulturedFact.DisableParallelization] is fed into the test method registration
				Explicit,
				SkipExceptions,
				SkipReason,
				SkipUnless,
				SkipWhen,
				Guard.ArgumentNotNull(testMethod).SourceFilePath,
				testMethod.SourceLineNumber,
				$"{displayName}[{culture}]",
				[async testCase => [GenerateTest(testCase, obj => CultureOverride.Call(culture, obj, MethodInvoker))]],
				testMethod,
				Timeout,
				testMethod.Traits,
				$"{UniqueIDGenerator.ForTestCase(testMethod.UniqueID, index: 0)}[{culture}]"
			)
		).CastOrToReadOnlyCollection();
	}
}
