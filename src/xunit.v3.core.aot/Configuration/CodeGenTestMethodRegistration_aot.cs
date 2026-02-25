#if XUNIT_GENERATOR
namespace Xunit.Generators;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Contains information about a test method, as discovered by code generation.
/// </summary>
public class CodeGenTestMethodRegistration
{
	/// <summary>
	/// Gets the method's arity (the number of generic types).
	/// </summary>
#if XUNIT_GENERATOR
	public required int Arity { get; set; }
#else
	public int Arity { get; init; }
#endif

	/// <summary>
	/// Gets the before/after attributes attached to the test method
	/// </summary>
#if XUNIT_GENERATOR
	public required IReadOnlyCollection<string>? BeforeAfterAttributeTypes { get; set; }
#else
	public Func<IReadOnlyCollection<BeforeAfterTestAttribute>>? BeforeAfterAttributesFactory { get; init; }
#endif

#if !XUNIT_GENERATOR

	/// <summary>
	/// Gets the empty test method registration.
	/// </summary>
	public static CodeGenTestMethodRegistration Empty { get; } = new();

#endif  // !XUNIT_GENERATOR

	/// <summary>
	/// Gets the declared type index of the test method if it differs from the test class
	/// </summary>
#if XUNIT_GENERATOR
	public required string? DeclaredTypeIndex { get; set; }
#else
	public string? DeclaredTypeIndex { get; init; }
#endif

	/// <summary>
	/// Gets a flag which indicates if the method is static
	/// </summary>
#if XUNIT_GENERATOR
	public required bool IsStatic { get; set; }
#else
	public bool IsStatic { get; init; }
#endif

	/// <summary>
	/// Gets the source file path for the test, if known.
	/// </summary>
#if XUNIT_GENERATOR
	public required string? SourceFilePath { get; set; }
#else
	public string? SourceFilePath { get; init; }
#endif

	/// <summary>
	/// Gets the source line number for the test, if known.
	/// </summary>
#if XUNIT_GENERATOR
	public required int? SourceLineNumber { get; set; }
#else
	public int? SourceLineNumber { get; init; }
#endif

	/// <summary>
	/// Gets the factory for the method-level test case orderer.
	/// </summary>
#if XUNIT_GENERATOR
	public required string? TestCaseOrdererType { get; set; }
#else
	public Func<ITestCaseOrderer>? TestCaseOrdererFactory { get; init; }
#endif

	/// <summary>
	/// The traits attached to the test method
	/// </summary>
#if XUNIT_GENERATOR
	public required IReadOnlyDictionary<string, List<string>>? Traits { get; set; }
#else
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>>? Traits { get; init; }
#endif

#if !XUNIT_GENERATOR

	readonly object factoryLock = new();
	ICodeGenTestMethod? testMethod;

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

				if (Traits is null || Traits.Count == 0)
					traits = testClass.Traits;
				else
				{
					var newTraits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
					foreach (var kvp in testClass.Traits)
						foreach (var value in kvp.Value)
							newTraits.AddOrGet(kvp.Key).Add(value);

					foreach (var kvp in Traits)
						foreach (var value in kvp.Value)
							newTraits.AddOrGet(kvp.Key).Add(value);

					traits = newTraits.ToReadOnly();
				}

				IEnumerable<BeforeAfterTestAttribute> beforeAfterTestAttributes = testClass.BeforeAfterTestAttributes;
				if (BeforeAfterAttributesFactory is not null)
					beforeAfterTestAttributes = beforeAfterTestAttributes.Concat(BeforeAfterAttributesFactory());

				testMethod = new CodeGenTestMethod(
					beforeAfterTestAttributes.CastOrToReadOnlyCollection(),
					DeclaredTypeIndex,
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

#endif  // !XUNIT_GENERATOR

#if XUNIT_GENERATOR

	/// <summary>
	/// Gets init values used by the source generator.
	/// </summary>
	public string ToGeneratedInit()
	{
		var initValues = new List<string>();

		if (Arity != 0)
			initValues.Add($"Arity = {Arity}");
		if (BeforeAfterAttributeTypes is not null && BeforeAfterAttributeTypes.Count != 0)
			initValues.Add($"BeforeAfterAttributesFactory = () => new global::Xunit.v3.BeforeAfterTestAttribute[] {{ {string.Join(", ", BeforeAfterAttributeTypes.Select(t => $"new {t}()"))} }}");
		if (DeclaredTypeIndex is not null)
			initValues.Add($"DeclaredTypeIndex = {DeclaredTypeIndex.Quoted()}");
		if (IsStatic)
			initValues.Add("IsStatic = true");
		if (SourceFilePath is not null)
			initValues.Add($"SourceFilePath = {SourceFilePath.Quoted()}");
		if (SourceLineNumber is not null)
			initValues.Add($"SourceLineNumber = {SourceLineNumber}");
		if (TestCaseOrdererType is not null)
			initValues.Add($"TestCaseOrdererFactory = () => new {TestCaseOrdererType}()");
		if (Traits is not null && Traits.Count != 0)
			initValues.Add($"Traits = {CodeGenRegistration.ToTraits(Traits)}");

		if (initValues.Count == 0)
			return "global::Xunit.v3.CodeGenTestMethodRegistration.Empty";

		return $"new global::Xunit.v3.CodeGenTestMethodRegistration() {{ {string.Join(", ", initValues)} }}";
	}

#endif  // XUNIT_GENERATOR
}
