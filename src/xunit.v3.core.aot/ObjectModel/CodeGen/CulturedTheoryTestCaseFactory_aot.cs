using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="ICodeGenTestCaseFactory"/> for use by tests which
/// are decorated by <see cref="CulturedTheoryAttribute"/>.
/// </summary>
public class CulturedTheoryTestCaseFactory : TheoryTestCaseFactoryBase
{
	/// <summary>
	/// Gets the cultures to be run.
	/// </summary>
	/// <remarks>
	/// Each culture will result in a test case, regardless of p
	/// </remarks>
	public required IReadOnlyCollection<string> Cultures { get; init; }

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

		return Cultures.Select(culture =>
		{
			var displayNameSuffix = $"[{culture}]";
			var testFactories = new List<Func<ICodeGenTestCase, ValueTask<IReadOnlyCollection<ICodeGenTest>>>>
			{
				async testCase =>
				{
					var result = new List<ICodeGenTest>();
					var idx = 0;

					foreach (var dataRowFactory in dataRowFactories)
						foreach (var dataRow in await dataRowFactory(disposalTracker))
							result.Add(
								CreateDelayEnumeratedTest(
									testCase,
									displayName,
									traits,
									dataRow,
									async obj => await CultureOverride.Call(culture, obj, await MethodInvokerFactory(dataRow)),
									++idx,
									displayNameSuffix
								)
							);

					return result;
				}
			};

			return CreateDelayEnumeratedTestCase(testMethod, displayName, traits, testFactories, displayNameSuffix);
		}).CastOrToReadOnlyCollection();
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

		foreach (var culture in Cultures)
		{
			var displayNameSuffix = $"[{culture}]";
			var idx = 0;

			foreach (var dataRowFactory in dataRowFactories)
				foreach (var dataRow in await dataRowFactory(disposalTracker))
					result.Add(
						CreatePreEnumeratedTestCase(
							testMethod,
							displayName,
							traits,
							dataRow,
							async obj => await CultureOverride.Call(culture, obj, await MethodInvokerFactory(dataRow)),
							++idx,
							displayNameSuffix
						)
					);
		}

		return result;
	}
}
