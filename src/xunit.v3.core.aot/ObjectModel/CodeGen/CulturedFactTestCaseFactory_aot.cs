using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="ICodeGenTestCaseFactory"/> for use by tests which
/// are decorated by <see cref="CulturedFactAttribute"/>.
/// </summary>
public class CulturedFactTestCaseFactory : FactTestCaseFactoryBase
{
	/// <summary>
	/// Gets the cultures to be run.
	/// </summary>
	/// <remarks>
	/// Each culture will result in a test case.
	/// </remarks>
	public required IReadOnlyCollection<string> Cultures { get; init; }

	/// <inheritdoc/>
	protected override async ValueTask<IReadOnlyCollection<ICodeGenTestCase>> GenerateTestCases(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		ICodeGenTestMethod testMethod,
		DisposalTracker disposalTracker,
		string displayName,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits)
	{
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(displayName);

		return Cultures.Select(culture =>
			new CodeGenTestCase(
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
				traits,
				$"{UniqueIDGenerator.ForTestCase(testMethod.UniqueID, index: 0)}[{culture}]"
			)
		).CastOrToReadOnlyCollection();
	}
}
