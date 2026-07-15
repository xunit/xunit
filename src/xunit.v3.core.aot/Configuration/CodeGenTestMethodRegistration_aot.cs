namespace Xunit.v3;

/// <summary>
/// Contains information about a test method, as discovered by code generation.
/// </summary>
public class CodeGenTestMethodRegistration
{
	readonly Lock factoryLock = new();
	ICodeGenTestMethod? testMethod;

	/// <summary>
	/// Gets the method's arity (the number of generic types).
	/// </summary>
	public int Arity { get; init; }

	/// <summary>
	/// Gets the before/after attributes attached to the test method
	/// </summary>
	public Func<IReadOnlyCollection<BeforeAfterTestAttribute>>? BeforeAfterAttributesFactory { get; init; }

	/// <summary>
	/// Gets the empty test method registration.
	/// </summary>
	public static CodeGenTestMethodRegistration Empty { get; } = new();

	/// <summary>
	/// Gets the declared type index of the test method if it differs from the test class
	/// </summary>
	public string? DeclaredTypeIndex { get; init; }

	/// <summary>
	/// Gets a flag which indicates if the tests from this test method wish to opt out of parallelism.
	/// </summary>
	public bool DisableParallelization { get; init; }

	/// <summary>
	/// Gets a flag which indicates if the method is static
	/// </summary>
	public bool IsStatic { get; init; }

	/// <summary>
	/// Gets the source file path for the test, if known.
	/// </summary>
	public string? SourceFilePath { get; init; }

	/// <summary>
	/// Gets the source line number for the test, if known.
	/// </summary>
	public int? SourceLineNumber { get; init; }

	/// <summary>
	/// Gets the factory for the method-level test case orderer.
	/// </summary>
	public Func<ITestCaseOrderer>? TestCaseOrdererFactory { get; init; }

	/// <summary>
	/// Generates the <see cref="ICodeGenTestMethod"/> instance for this registration.
	/// </summary>
	/// <param name="testClass">The test class the test method belongs to</param>
	/// <param name="methodName">The test method name</param>
	public ICodeGenTestMethod GetTestMethod(
		ICodeGenTestClass testClass,
		string methodName)
	{
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentNotNullOrEmpty(methodName);

		lock (factoryLock)
			if (testMethod is null)
			{
				IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;
				var testMethodTraits = RegisteredEngineConfig.GetTestMethodTraits(testClass.Class, methodName);

				if (testMethodTraits is null || testMethodTraits.Count == 0)
					traits = testClass.Traits;
				else
				{
					var newTraits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

					foreach (var kvp in testClass.Traits)
						newTraits.AddOrGet(kvp.Key).AddRange(kvp.Value);

					foreach (var kvp in testMethodTraits)
						newTraits.AddOrGet(kvp.Key).AddRange(kvp.Value);

					traits = newTraits.ToReadOnly();
				}

				IEnumerable<BeforeAfterTestAttribute> beforeAfterTestAttributes = testClass.BeforeAfterTestAttributes;
				if (BeforeAfterAttributesFactory is not null)
					beforeAfterTestAttributes = beforeAfterTestAttributes.Concat(BeforeAfterAttributesFactory());

				testMethod = new CodeGenTestMethod(
					beforeAfterTestAttributes.CastOrToReadOnlyCollection(),
					DeclaredTypeIndex,
					DisableParallelization,
					IsStatic,
					Arity,
					methodName,
					SourceFilePath,
					SourceLineNumber,
					testClass,
					traits
				);
			}

		return testMethod;
	}
}
