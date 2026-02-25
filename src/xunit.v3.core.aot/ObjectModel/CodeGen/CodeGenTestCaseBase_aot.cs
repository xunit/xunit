using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base class implementation of <see cref="ICodeGenTestCase"/>.
/// </summary>
/// <param name="explicit">A flag to indicate whether the test case is marked as explicit</param>
/// <param name="skipExceptions">The exception types that, when thrown, will cause the test to be skipped rather than failed</param>
/// <param name="skipReason">The display text for the reason the test case might be skipped</param>
/// <param name="skipUnless">A function which, if it returns <see langword="false" />, will skip the test</param>
/// <param name="skipWhen">A function which, if it returns <see langword="true" />, will skip the test</param>
/// <param name="sourceFilePath">The source file name where the test case originated</param>
/// <param name="sourceLineNumber">The source line number where the test case originated</param>
/// <param name="testCaseDisplayName">The test case display name</param>
/// <param name="testMethod">The test method this test case belongs to</param>
/// <param name="timeout">The timeout, in seconds, for this test (set to <c>0</c> to disable timeout)</param>
/// <param name="traits">The traits attached to the test case</param>
/// <param name="uniqueID">The test case unique ID</param>
public abstract class CodeGenTestCaseBase(
	bool @explicit,
	Type[]? skipExceptions,
	string? skipReason,
	Func<bool>? skipUnless,
	Func<bool>? skipWhen,
	string? sourceFilePath,
	int? sourceLineNumber,
	string testCaseDisplayName,
	ICodeGenTestMethod testMethod,
	int timeout,
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
	string uniqueID) :
		ICodeGenTestCase
{
	/// <inheritdoc/>
	public bool Explicit =>
		@explicit;

	/// <inheritdoc/>
	public Type[]? SkipExceptions =>
		skipExceptions;

	/// <inheritdoc/>
	public string? SkipReason =>
		skipReason;

	// Contractually, we don't want to return a non-null SkipReason when there is
	// a setting for SkipUnless or SkipWhen, since the contract is to get the reason
	// for a statically skipped test.
	string? ITestCaseMetadata.SkipReason =>
		SkipUnless is null && SkipWhen is null ? SkipReason : null;

	/// <inheritdoc/>
	public Func<bool>? SkipUnless =>
		skipUnless;

	/// <inheritdoc/>
	public Func<bool>? SkipWhen =>
		skipWhen;

	/// <inheritdoc/>
	public string? SourceFilePath =>
		sourceFilePath;

	/// <inheritdoc/>
	public int? SourceLineNumber =>
		sourceLineNumber;

	/// <inheritdoc/>
	public string TestCaseDisplayName { get; } =
		Guard.ArgumentNotNull(testCaseDisplayName);

	/// <inheritdoc/>
	public ICodeGenTestClass TestClass =>
		TestMethod.TestClass;

	ICoreTestClass ICoreTestCase.TestClass => TestClass;

	ITestClass? ITestCase.TestClass => TestClass;

	/// <remarks>
	/// Test class metadata tokens are not available for code generation-based tests, so this will always
	/// return <see langword="null"/>.
	/// </remarks>
	/// <inheritdoc/>
	public int? TestClassMetadataToken =>
		null;

	/// <inheritdoc/>
	public string TestClassName =>
		TestClass.TestClassName;

	/// <inheritdoc/>
	public string? TestClassNamespace =>
		TestClass.TestClassNamespace;

	/// <inheritdoc/>
	public string TestClassSimpleName =>
		TestClass.TestClassSimpleName;

	/// <inheritdoc/>
	public ICodeGenTestCollection TestCollection =>
		TestClass.TestCollection;

	ICoreTestCollection ICoreTestCase.TestCollection => TestCollection;

	ITestCollection ITestCase.TestCollection => TestCollection;

	/// <inheritdoc/>
	public ICodeGenTestMethod TestMethod { get; } =
		Guard.ArgumentNotNull(testMethod);

	ICoreTestMethod ICoreTestCase.TestMethod => TestMethod;

	ITestMethod? ITestCase.TestMethod => TestMethod;

	/// <inheritdoc/>
	public int TestMethodArity =>
		TestMethod.MethodArity;

	int? ITestCaseMetadata.TestMethodArity => TestMethod.MethodArity;

	/// <remarks>
	/// Test method metadata tokens are not available for code generation-based tests, so this will always
	/// return <see langword="null"/>.
	/// </remarks>
	/// <inheritdoc/>
	public int? TestMethodMetadataToken =>
		null;

	/// <inheritdoc/>
	public string TestMethodName =>
		TestMethod.MethodName;

	/// <remarks>
	/// This information is not provided for code generation-based tests, and will always return <see langword="null"/>.
	/// </remarks>
	/// <inheritdoc/>
	public string[]? TestMethodParameterTypesVSTest =>
		null;

	/// <remarks>
	/// This information is not provided for code generation-based tests, and will always return <see langword="null"/>.
	/// </remarks>
	/// <inheritdoc/>
	public string? TestMethodReturnTypeVSTest =>
		null;

	/// <inheritdoc/>
	public int Timeout =>
		timeout;

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; } =
		Guard.ArgumentNotNull(traits);

	/// <inheritdoc/>
	public string UniqueID { get; } =
		Guard.ArgumentNotNull(uniqueID);

	/// <inheritdoc/>
	public abstract ValueTask<IReadOnlyCollection<ICodeGenTest>> CreateTests();

	/// <inheritdoc/>
	public virtual void PostInvoke()
	{ }

	/// <inheritdoc/>
	public virtual void PreInvoke()
	{ }
}
