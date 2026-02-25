using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="ICodeGenTestMethod"/> for xUnit.net v3 tests.
/// </summary>
/// <param name="beforeAfterTestAttributes">The <see cref="BeforeAfterTestAttribute"/>s attached to the test method</param>
/// <param name="declaredTypeIndex">The type index for the declared type if it differs from the test class type</param>
/// <param name="isStatic">Indicates if the test method is static</param>
/// <param name="methodArity">The test method's arity</param>
/// <param name="methodName">The test method's name</param>
/// <param name="sourceFilePath">The source file where the method lives, if known</param>
/// <param name="sourceLineNumber">The line number where the method lives, if known</param>
/// <param name="testClass">The test class this test method belongs to</param>
/// <param name="traits">The traits attached to the test method</param>
/// <param name="uniqueID">The optional unique ID for the test method</param>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestMethod(
	IReadOnlyCollection<BeforeAfterTestAttribute>? beforeAfterTestAttributes,
	string? declaredTypeIndex,
	bool isStatic,
	int methodArity,
	string methodName,
	string? sourceFilePath,
	int? sourceLineNumber,
	ICodeGenTestClass testClass,
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits,
	string? uniqueID = null) :
		ICodeGenTestMethod
{
	readonly Lazy<ITestCaseOrderer?> testCaseOrderer = new(() => RegisteredEngineConfig.GetMethodTestCaseOrderer(testClass.Class, methodName));

	/// <inheritdoc/>
	public IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; } =
		beforeAfterTestAttributes ?? [];

	/// <inheritdoc/>
	public string? DeclaredTypeIndex { get; } =
		declaredTypeIndex;

	/// <inheritdoc/>
	public bool IsStatic =>
		isStatic;

	/// <inheritdoc/>
	public int MethodArity =>
		methodArity;

	int? ITestMethodMetadata.MethodArity => MethodArity;

	/// <inheritdoc/>
	public string MethodName { get; } =
		Guard.ArgumentNotNull(methodName);

	/// <inheritdoc/>
	public string? SourceFilePath =>
		sourceFilePath;

	/// <inheritdoc/>
	public int? SourceLineNumber =>
		sourceLineNumber;

	/// <inheritdoc/>
	public ITestCaseOrderer? TestCaseOrderer =>
		testCaseOrderer.Value;

	/// <inheritdoc/>
	public ICodeGenTestClass TestClass { get; } =
		Guard.ArgumentNotNull(testClass);

	ICoreTestClass ICoreTestMethod.TestClass => TestClass;

	ITestClass ITestMethod.TestClass => TestClass;

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; } =
		traits ?? testClass.Traits;

	/// <inheritdoc/>
	public string UniqueID { get; } =
		uniqueID ?? UniqueIDGenerator.ForTestMethod(testClass.UniqueID, methodName);
}
