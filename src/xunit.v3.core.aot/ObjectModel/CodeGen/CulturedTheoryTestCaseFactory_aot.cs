using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="ICodeGenTestCaseFactory"/> for use by tests which
/// are decorated by <see cref="CulturedTheoryAttribute"/>.
/// </summary>
public class CulturedTheoryTestCaseFactory : TheoryTestCaseFactory
{
	/// <summary>
	/// Gets the cultures to be run.
	/// </summary>
	public required IReadOnlyCollection<string> Cultures { get; init; }

	/// <summary>
	/// Creates one test case per culture from <see cref="Cultures"/>.
	/// </summary>
	/// <remarks>
	/// The logic here follows much the same as <see cref="TheoryTestCaseFactory.GenerateDelayEnumerated"/> on a
	/// per culture basis.
	/// </remarks>
	/// <inheritdoc/>
	protected override async ValueTask<IReadOnlyCollection<ICodeGenTestCase>> GenerateDelayEnumerated(
		ICodeGenTestMethod testMethod,
		string displayName,
		DisposalTracker disposalTracker,
		IReadOnlyCollection<Func<DisposalTracker, ValueTask<IReadOnlyCollection<ITheoryDataRow>>>> dataRowFactories)
	{
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(displayName);
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
									dataRow,
									async obj => await CultureOverride.Call(culture, obj, await MethodInvokerFactory(dataRow)),
									++idx,
									displayNameSuffix
								)
							);

					return result;
				}
			};

			return CreateDelayEnumeratedTestCase(testMethod, displayName, testFactories, displayNameSuffix);
		}).CastOrToReadOnlyCollection();
	}

	/// <summary>
	/// Creates one test case per the cross product of cultures from <see cref="Cultures"/> and the data rows.
	/// </summary>
	/// <remarks>
	/// The logic here follows much the same as <see cref="TheoryTestCaseFactory.GeneratePreEnumerated"/> on a
	/// per culture basis.
	/// </remarks>
	/// <inheritdoc/>
	protected override async ValueTask<IReadOnlyCollection<ICodeGenTestCase>> GeneratePreEnumerated(
		ICodeGenTestMethod testMethod,
		string displayName,
		DisposalTracker disposalTracker,
		IReadOnlyCollection<Func<DisposalTracker, ValueTask<IReadOnlyCollection<ITheoryDataRow>>>> dataRowFactories)
	{
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(displayName);
		Guard.ArgumentNotNull(dataRowFactories);

		var result = new List<ICodeGenTestCase>();

		foreach (var culture in Cultures)
		{
			var displayNameSuffix = $"[{culture}]";
			var idx = 0;

			foreach (var dataRowFactory in dataRowFactories)
				foreach (var dataRow in await dataRowFactory(disposalTracker))
				{
					idx++;

					var displayNameIndex =
						IncludeTestCaseIndex
							? StringExtensions.FormatTestCaseIndex(idx)
							: null;

					result.Add(
						CreatePreEnumeratedTestCase(
							testMethod,
							displayName,
							dataRow,
							async obj => await CultureOverride.Call(culture, obj, await MethodInvokerFactory(dataRow)),
							idx,
							displayNameSuffix,
							displayNameIndex
						)
					);
				}
		}

		return result;
	}
}
