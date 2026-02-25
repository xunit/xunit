using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="ICodeGenTest"/> for xUnit.net v3 tests.
/// </summary>
/// <param name="explicit">A flag to indicate whether the test case is marked as explicit</param>
/// <param name="methodInvoker">The test method invoker</param>
/// <param name="skipReason">The display text for the reason the test case might be skipped</param>
/// <param name="skipUnless">A function which, if it returns <see langword="false" />, will skip the test</param>
/// <param name="skipWhen">A function which, if it returns <see langword="true" />, will skip the test</param>
/// <param name="testCase">The test case this test belongs to</param>
/// <param name="testDisplayName">The display name of the test</param>
/// <param name="testLabel">The label of the test</param>
/// <param name="timeout">The timeout, in seconds, for this test (set to <c>0</c> to disable timeout)</param>
/// <param name="traits">The traits attached to the test case</param>
/// <param name="uniqueID">The test unique ID</param>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTest(
	bool @explicit,
	Func<object?, ValueTask> methodInvoker,
	string? skipReason,
	Func<bool>? skipUnless,
	Func<bool>? skipWhen,
	ICodeGenTestCase testCase,
	string testDisplayName,
	string? testLabel,
	int timeout,
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
	string uniqueID) :
		ICodeGenTest
{
	/// <inheritdoc/>
	public bool Explicit =>
		@explicit;

	/// <inheritdoc/>
	public Func<object?, ValueTask> MethodInvoker { get; } =
		Guard.ArgumentNotNull(methodInvoker);

	/// <inheritdoc/>
	public string? SkipReason =>
		skipReason;

	/// <inheritdoc/>
	public Func<bool>? SkipUnless =>
		skipUnless;

	/// <inheritdoc/>
	public Func<bool>? SkipWhen =>
		skipWhen;

	/// <inheritdoc/>
	public ICodeGenTestCase TestCase { get; } =
		Guard.ArgumentNotNull(testCase);

	ICoreTestCase ICoreTest.TestCase => TestCase;

	ITestCase ITest.TestCase => TestCase;

	/// <inheritdoc/>
	public string TestDisplayName { get; } =
		Guard.ArgumentNotNull(testDisplayName);

	/// <inheritdoc/>
	public string? TestLabel =>
		testLabel;

	/// <inheritdoc/>
	public int Timeout =>
		timeout;

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; } =
		Guard.ArgumentNotNull(traits);

	/// <inheritdoc/>
	public string UniqueID { get; } =
		Guard.ArgumentNotNull(uniqueID);
}
