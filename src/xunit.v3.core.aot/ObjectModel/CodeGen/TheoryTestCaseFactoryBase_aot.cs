using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base implementation of <see cref="ICodeGenTestCaseFactory"/> designed to support
/// <see cref="TheoryAttribute"/> and <see cref="CulturedTheoryAttribute"/>.
/// </summary>
public abstract class TheoryTestCaseFactoryBase : TestCaseFactoryBase
{
	/// <summary>
	/// Gets a flag which indicates whether the test method wants to skip enumerating data during
	/// discovery. This will cause the theory to yield a single test case for all data, and the
	/// data discovery will be performed during test execution instead of discovery.
	/// </summary>
	public bool? DisableDiscoveryEnumeration { get; set; }

	/// <summary>
	/// Gets the factory method that converts a data row into a method invoker.
	/// </summary>
	public required Func<ITheoryDataRow, ValueTask<Func<object?, ValueTask>>> MethodInvokerFactory { get; set; }

	/// <summary>
	/// Gets the parameter default values for the test method.
	/// </summary>
	public string?[]? ParameterDefaultValues { get; set; }

	/// <summary>
	/// Gets the parameter names for the test method.
	/// </summary>
	public required string?[] ParameterNames { get; set; }

	/// <summary>
	/// Gets a flag which indicates whether the test should be skipped (rather than failed) for
	/// a lack of data.
	/// </summary>
	public bool SkipTestWithoutData { get; set; }

	/// <summary>
	/// Creates a <see cref="CodeGenTest"/> attached to a <see cref="ICodeGenTestCase"/> used for delay enumeration.
	/// </summary>
	/// <param name="testCase">The test case</param>
	/// <param name="displayName">The display name (to be used if <c><paramref name="dataRow"/>.TestDisplayName</c> is <see langword="null"/>)</param>
	/// <param name="traits"></param>
	/// <param name="dataRow">The data row</param>
	/// <param name="methodInvoker">The method invoker</param>
	/// <param name="testIndex">The test index</param>
	/// <param name="displayNameSuffix">The optional display name suffix</param>
	protected CodeGenTest CreateDelayEnumeratedTest(
		ICodeGenTestCase testCase,
		string displayName,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
		ITheoryDataRow dataRow,
		Func<object?, ValueTask> methodInvoker,
		int testIndex,
		string? displayNameSuffix = null) =>
			new(
				Guard.ArgumentNotNull(dataRow).Explicit ?? Explicit,
				methodInvoker,
				dataRow.Skip ?? SkipReason,
				dataRow.SkipUnless ?? SkipUnless,
				dataRow.SkipWhen ?? SkipWhen,
				testCase,
				GetTestDisplayName(dataRow, displayName, displayNameSuffix),
				dataRow.Label,
				dataRow.Timeout ?? Timeout,
				MergeTraits(traits, dataRow.Traits),
				UniqueIDGenerator.ForTest(Guard.ArgumentNotNull(testCase).UniqueID, testIndex)
			);

	/// <summary>
	/// Creates a <see cref="CodeGenTestCase"/> intended to be used for delay enumeration.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	/// <param name="displayName">The test case display name</param>
	/// <param name="traits">The test case traits</param>
	/// <param name="testFactories">The factories for the tests</param>
	/// <param name="displayNameSuffix">The optional display name suffix (will also be appended to the end of the unique ID)</param>
	protected CodeGenTestCase CreateDelayEnumeratedTestCase(
		ICodeGenTestMethod testMethod,
		string displayName,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
		IReadOnlyCollection<Func<ICodeGenTestCase, ValueTask<IReadOnlyCollection<ICodeGenTest>>>> testFactories,
		string? displayNameSuffix = null) =>
			new(
				Explicit,
				SkipExceptions,
				SkipReason,
				SkipUnless,
				SkipWhen,
				Guard.ArgumentNotNull(testMethod).SourceFilePath,
				testMethod.SourceLineNumber,
				$"{displayName}{displayNameSuffix}",
				testFactories,
				testMethod,
				Timeout,
				traits,
				$"{UniqueIDGenerator.ForTestCase(testMethod.UniqueID, index: 0)}{displayNameSuffix}"
			);

	/// <summary>
	/// Creates a <see cref="CodeGenTestCase"/> intended to be used for pre-enumeration.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	/// <param name="displayName">The display name (to be used if <c><paramref name="dataRow"/>.TestDisplayName</c> is <see langword="null"/>)</param>
	/// <param name="traits">The test case traits</param>
	/// <param name="dataRow">The data row</param>
	/// <param name="methodInvoker">The method invoker</param>
	/// <param name="testCaseIndex">The index to be used for unique ID generation</param>
	/// <param name="displayNameSuffix">The optional display name suffix (will also be appended to the end of the unique ID)</param>
	protected CodeGenTestCase CreatePreEnumeratedTestCase(
		ICodeGenTestMethod testMethod,
		string displayName,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
		ITheoryDataRow dataRow,
		Func<object?, ValueTask> methodInvoker,
		int testCaseIndex,
		string? displayNameSuffix = null)
	{
		Guard.ArgumentNotNull(dataRow);
		Guard.ArgumentNotNull(testMethod);

		var testDisplayName = GetTestDisplayName(dataRow, displayName, displayNameSuffix);
		var mergedTraits = MergeTraits(traits, dataRow.Traits);

		var skipReason = SkipReason;
		var skipUnless = SkipUnless;
		var skipWhen = SkipWhen;

		if (dataRow.Skip is not null)
		{
			skipReason = dataRow.Skip;
			skipUnless = dataRow.SkipUnless;
			skipWhen = dataRow.SkipWhen;
		}

		return new CodeGenTestCase(
			dataRow.Explicit ?? Explicit,
			SkipExceptions,
			skipReason,
			skipUnless,
			skipWhen,
			testMethod.SourceFilePath,
			testMethod.SourceLineNumber,
			testDisplayName,
			[
				async testCase => [new CodeGenTest(
					dataRow.Explicit ?? Explicit,
					methodInvoker,
					skipReason,
					skipUnless,
					skipWhen,
					testCase,
					testDisplayName,
					dataRow.Label,
					dataRow.Timeout ?? Timeout,
					mergedTraits,
					UniqueIDGenerator.ForTest(testCase.UniqueID, testIndex: 0)
				)]
			],
			testMethod,
			dataRow.Timeout ?? Timeout,
			mergedTraits,
			$"{UniqueIDGenerator.ForTestCase(testMethod.UniqueID, testCaseIndex)}{displayNameSuffix}"
		);
	}

	/// <summary>
	/// Formats the parameters of a data row, for use in the display name suffix.
	/// </summary>
	/// <param name="dataRow">The data row</param>
	protected string FormatParameters(ITheoryDataRow dataRow)
	{
		var parameters = new List<string>();
		var data = Guard.ArgumentNotNull(dataRow).GetData();
		var maxIndex = Math.Max(ParameterNames.Length, data.Length);
		for (var idx = 0; idx < maxIndex; ++idx)
			parameters.Add($"{TryGetParameterName(idx)}: {TryGetValue(data, idx)}");

		return string.Join(", ", parameters);
	}

	/// <summary>
	/// Generate test cases for delay enumeration.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	/// <param name="displayName">The test case display name</param>
	/// <param name="disposalTracker">The disposal tracker (used for class data instances and objects in data rows)</param>
	/// <param name="traits">The test case traits</param>
	/// <param name="dataRowFactories">The data row factories</param>
	protected abstract ValueTask<IReadOnlyCollection<ICodeGenTestCase>> GenerateDelayEnumerated(
		ICodeGenTestMethod testMethod,
		string displayName,
		DisposalTracker disposalTracker,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
		IReadOnlyCollection<Func<DisposalTracker, ValueTask<IReadOnlyCollection<ITheoryDataRow>>>> dataRowFactories);

	/// <summary>
	/// Generate test cases for pre-enumeration.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	/// <param name="displayName">The test case display name</param>
	/// <param name="disposalTracker">The disposal tracker (used for class data instances and objects in data rows)</param>
	/// <param name="traits">The test case traits</param>
	/// <param name="dataRowFactories">The data row factories</param>
	protected abstract ValueTask<IReadOnlyCollection<ICodeGenTestCase>> GeneratePreEnumerated(
		ICodeGenTestMethod testMethod,
		string displayName,
		DisposalTracker disposalTracker,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
		IReadOnlyCollection<Func<DisposalTracker, ValueTask<IReadOnlyCollection<ITheoryDataRow>>>> dataRowFactories);

	/// <inheritdoc/>
	protected override async ValueTask<IReadOnlyCollection<ICodeGenTestCase>> GenerateTestCases(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		ICodeGenTestMethod testMethod,
		DisposalTracker disposalTracker,
		string displayName,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(disposalTracker);
		Guard.ArgumentNotNull(displayName);
		Guard.ArgumentNotNull(traits);

		try
		{
			var dataRowFactories = RegisteredEngineConfig.GetTheoryDataRowFactories(testMethod);

			// Statically skipped theories should always generate a single test case, regardless of pre-enumeration
			if (SkipReason is not null && SkipUnless is null && SkipWhen is null)
				return [new SkipTestCase(
					Explicit,
					SkipReason,
					testMethod.SourceFilePath,
					testMethod.SourceLineNumber,
					displayName,
					testMethod,
					traits,
					UniqueIDGenerator.ForTestCase(testMethod.UniqueID, index: -1)
				)];

			// If there is no data, return either a single failed or skipped test case.
			if (dataRowFactories.Count == 0)
			{
				var message = string.Format(
					CultureInfo.CurrentCulture,
					"No data found for {0}.{1}",
					testMethod.TestClass.TestClassName,
					testMethod.MethodName
				);

				if (SkipTestWithoutData)
					return [new SkipTestCase(
						Explicit,
						message,
						testMethod.SourceFilePath,
						testMethod.SourceLineNumber,
						displayName,
						testMethod,
						traits,
						UniqueIDGenerator.ForTestCase(testMethod.UniqueID, index: -1)
					)];

				return [new ExecutionErrorTestCase(
					message,
					Explicit,
					testMethod.SourceFilePath,
					testMethod.SourceLineNumber,
					displayName,
					testMethod,
					traits,
					UniqueIDGenerator.ForTestCase(testMethod.UniqueID, index: -1)
				)];
			}

			return
				DisableDiscoveryEnumeration ?? !discoveryOptions.PreEnumerateTheoriesOrDefault()
					? await GenerateDelayEnumerated(testMethod, displayName, disposalTracker, traits, dataRowFactories)
					: await GeneratePreEnumerated(testMethod, displayName, disposalTracker, traits, dataRowFactories);
		}
		catch (Exception ex)
		{
			return [new ExecutionErrorTestCase(
				ex,
				Explicit,
				testMethod.SourceFilePath,
				testMethod.SourceLineNumber,
				displayName,
				testMethod,
				traits,
				UniqueIDGenerator.ForTestCase(testMethod.UniqueID, index: -1)
			)];
		}
	}

	/// <summary>
	/// Calculates the final display name of a test
	/// </summary>
	/// <param name="dataRow">The data row</param>
	/// <param name="baseDisplayName">The base display name</param>
	/// <param name="displayNameSuffix">The display name suffix</param>
	protected string GetTestDisplayName(
		ITheoryDataRow dataRow,
		string baseDisplayName,
		string? displayNameSuffix)
	{
		Guard.ArgumentNotNull(dataRow);
		Guard.ArgumentNotNull(baseDisplayName);

		var displayName = dataRow.TestDisplayName ?? baseDisplayName;
		var label = dataRow.Label;

		if (label is null)
			return $"{displayName}({FormatParameters(dataRow)}){displayNameSuffix}";

		if (label.Length == 0)
			return displayName;

		return $"{displayName} [{label}]{displayNameSuffix}";
	}

	/// <summary>
	/// Merges two sets of traits together into a single trait dictionary
	/// </summary>
	/// <param name="testCaseTraits">The traits from the test case</param>
	/// <param name="dataRowTraits">The traits from the data row</param>
	/// <returns>The merged traits</returns>
	protected static IReadOnlyDictionary<string, IReadOnlyCollection<string>> MergeTraits(
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> testCaseTraits,
		Dictionary<string, HashSet<string>>? dataRowTraits)
	{
		Guard.ArgumentNotNull(testCaseTraits);

		if (dataRowTraits is null || dataRowTraits.Count == 0)
			return testCaseTraits;

		var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

		foreach (var kvp in testCaseTraits)
			foreach (var value in kvp.Value)
				result.Add(kvp.Key, value);

		foreach (var kvp in dataRowTraits)
			foreach (var value in kvp.Value)
				result.Add(kvp.Key, value);

		return result.ToReadOnly();
	}

	/// <summary>
	/// Gets a parameter name for generating the argument display.
	/// </summary>
	/// <param name="idx">The parameter index</param>
	/// <returns>The parameter name, if known; <c>"???"</c> if unknown</returns>
	protected string TryGetParameterName(int idx)
	{
		var result = default(string);

		if (ParameterNames.Length > idx)
			result = ParameterNames[idx];

		return result ?? "???";
	}

	/// <summary>
	/// Gets a parameter value from an array.
	/// </summary>
	/// <param name="data">The data array</param>
	/// <param name="idx">The index</param>
	/// <returns>The formatted parameter value, if known, <c>"???"</c> if unknown</returns>
	protected string TryGetValue(
		object?[] data,
		int idx)
	{
		Guard.ArgumentNotNull(data);

		var result = default(string);

		if (data.Length > idx)
			result = ArgumentFormatter.Format(data[idx]);
		else if (ParameterDefaultValues?.Length > idx)
			result = ParameterDefaultValues[idx];

		return result ?? "???";
	}
}
