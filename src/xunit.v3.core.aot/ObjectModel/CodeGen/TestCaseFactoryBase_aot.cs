using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base implementation of <see cref="ICodeGenTestCaseFactory"/>.
/// </summary>
/// <remarks>
/// The intention is that this class supports <see cref="FactAttribute"/> and similar
/// attributes.
/// </remarks>
public abstract class TestCaseFactoryBase : ICodeGenTestCaseFactory
{
	/// <summary>
	/// Gets the display name, if it's been overridden.
	/// </summary>
	public string? DisplayName { get; init; }

	/// <summary>
	/// Gets a flag indicating whether a test is marked as explicit.
	/// </summary>
	public bool Explicit { get; init; }

	/// <summary>
	/// Gets the exception types that, when thrown, indicate the test should be skipped rather
	/// than failed.
	/// </summary>
	public Type[]? SkipExceptions { get; init; }

	/// <summary>
	/// Gets the message used to skip a test.
	/// </summary>
	public string? SkipReason { get; init; }

	/// <summary>
	/// Gets the values used to conditionally skip a test (when false).
	/// </summary>
	public Func<bool>? SkipUnless { get; init; }

	/// <summary>
	/// Gets the values used to conditionally skip a test (when false).
	/// </summary>
	public Func<bool>? SkipWhen { get; init; }

	/// <summary>
	/// The timeout for the test (if <c>0</c>, the test has no timeout)
	/// </summary>
	public int Timeout { get; init; }

	/// <summary>
	/// The traits attached to the test case
	/// </summary>
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>>? Traits { get; init; }

	/// <inheritdoc/>
	public ValueTask<IReadOnlyCollection<ICodeGenTestCase>> Generate(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		ICodeGenTestMethod testMethod,
		DisposalTracker disposalTracker)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);

		var defaultMethodDisplay = discoveryOptions.MethodDisplayOrDefault();
		var defaultMethodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();
		var formatter = new DisplayNameFormatter(defaultMethodDisplay, defaultMethodDisplayOptions);
		var displayName =
			DisplayName ??
				(defaultMethodDisplay == TestMethodDisplay.ClassAndMethod
					? formatter.Format(string.Format(CultureInfo.CurrentCulture, "{0}.{1}", testMethod.TestClass.Class.SafeName(), testMethod.MethodName))
					: formatter.Format(testMethod.MethodName));

		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;

		if (Traits is null || Traits.Count == 0)
			traits = testMethod.Traits;
		else
		{
			var newTraits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
			foreach (var kvp in testMethod.Traits)
				foreach (var value in kvp.Value)
					newTraits.AddOrGet(kvp.Key).Add(value);

			foreach (var kvp in Traits)
				foreach (var value in kvp.Value)
					newTraits.AddOrGet(kvp.Key).Add(value);

			traits = newTraits.ToReadOnly();
		}

		return GenerateTestCases(discoveryOptions, testMethod, disposalTracker, displayName, traits);
	}

	/// <summary>
	/// Generates a test for a test case.
	/// </summary>
	/// <param name="testCase">The test case the test belongs to</param>
	/// <param name="methodInvoker">The code that will invoke the test method when the test is run</param>
	/// <param name="testIndex">The test index (defaults to <c>0</c>)</param>
	protected static ICodeGenTest GenerateTest(
		ICodeGenTestCase testCase,
		Func<object?, ValueTask> methodInvoker,
		int testIndex = 0) =>
			new CodeGenTest(
				Guard.ArgumentNotNull(testCase).Explicit,
				methodInvoker,
				testCase.SkipReason,
				testCase.SkipUnless,
				testCase.SkipWhen,
				testCase,
				testCase.TestCaseDisplayName,
				testLabel: null,
				testCase.Timeout,
				testCase.Traits,
				UniqueIDGenerator.ForTest(testCase.UniqueID, testIndex)
			);

	/// <summary>
	/// Generates test cases.
	/// </summary>
	/// <param name="discoveryOptions">The requested discovery options</param>
	/// <param name="testMethod">The test method the test cases are generated from</param>
	/// <param name="disposalTracker">The disposal tracker (for class data instances and objects in data rows)</param>
	/// <param name="displayName">The base display name of the test case</param>
	/// <param name="traits">The traits for the test case</param>
	protected abstract ValueTask<IReadOnlyCollection<ICodeGenTestCase>> GenerateTestCases(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		ICodeGenTestMethod testMethod,
		DisposalTracker disposalTracker,
		string displayName,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits);
}
