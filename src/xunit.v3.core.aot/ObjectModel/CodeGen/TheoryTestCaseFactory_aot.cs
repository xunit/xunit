using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="ICodeGenTestCaseFactory"/> for use by tests which
/// are decorated by <see cref="TheoryAttribute"/>.
/// </summary>
public class TheoryTestCaseFactory : TheoryTestCaseFactoryBase
{
	/// <inheritdoc/>
	protected override async ValueTask<IReadOnlyCollection<ICodeGenTestCase>> GenerateDelayEnumerated(
		ICodeGenTestMethod testMethod,
		string displayName,
		DisposalTracker disposalTracker,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
		IReadOnlyCollection<Func<DisposalTracker, ValueTask<IReadOnlyCollection<ITheoryDataRow>>>> dataRowFactories)
	{
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(displayName);
		Guard.ArgumentNotNull(traits);
		Guard.ArgumentNotNull(dataRowFactories);

		var testFactories = new List<Func<ICodeGenTestCase, ValueTask<IReadOnlyCollection<ICodeGenTest>>>>
		{
			async testCase =>
			{
				var result = new List<ICodeGenTest>();
				var idx = 0;

				foreach (var dataRowFactory in dataRowFactories)
					foreach (var dataRow in await dataRowFactory(disposalTracker))
						result.Add(CreateDelayEnumeratedTest(testCase, displayName, traits, dataRow, await MethodInvokerFactory(dataRow), ++idx));

				return result;
			}
		};

		return [CreateDelayEnumeratedTestCase(testMethod, displayName, traits, testFactories)];
	}

	/// <inheritdoc/>
	protected override async ValueTask<IReadOnlyCollection<ICodeGenTestCase>> GeneratePreEnumerated(
		ICodeGenTestMethod testMethod,
		string displayName,
		DisposalTracker disposalTracker,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
		IReadOnlyCollection<Func<DisposalTracker, ValueTask<IReadOnlyCollection<ITheoryDataRow>>>> dataRowFactories)
	{
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(displayName);
		Guard.ArgumentNotNull(traits);
		Guard.ArgumentNotNull(dataRowFactories);

		var result = new List<ICodeGenTestCase>();
		var idx = 0;

		foreach (var dataRowFactory in dataRowFactories)
			foreach (var dataRow in await dataRowFactory(disposalTracker))
				result.Add(CreatePreEnumeratedTestCase(testMethod, displayName, traits, dataRow, await MethodInvokerFactory(dataRow), ++idx));

		return result;
	}
}
