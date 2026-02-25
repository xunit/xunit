namespace Xunit.v3;

/// <summary>
/// Represents a test case from xUnit.net v3 based on code generation.
/// </summary>
/// <param name="explicit">A flag to indicate whether the test case is marked as explicit</param>
/// <param name="skipExceptions">The exception types that, when thrown, will cause the test to be skipped rather than failed</param>
/// <param name="skipReason">The display text for the reason the test case might be skipped</param>
/// <param name="skipUnless">A function which, if it returns <see langword="false" />, will skip the test</param>
/// <param name="skipWhen">A function which, if it returns <see langword="true" />, will skip the test</param>
/// <param name="sourceFilePath">The source file name where the test case originated</param>
/// <param name="sourceLineNumber">The source line number where the test case originated</param>
/// <param name="testCaseDisplayName">The test case display name</param>
/// <param name="testFactories">The factories that create <see cref="ICodeGenTest"/> objects for the test case</param>
/// <param name="testMethod">The test method this test case belongs to</param>
/// <param name="timeout">The timeout, in seconds, for this test (set to <c>0</c> to disable timeout)</param>
/// <param name="traits">The traits attached to the test case</param>
/// <param name="uniqueID">The test case unique ID</param>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public sealed class CodeGenTestCase(
	bool @explicit,
	Type[]? skipExceptions,
	string? skipReason,
	Func<bool>? skipUnless,
	Func<bool>? skipWhen,
	string? sourceFilePath,
	int? sourceLineNumber,
	string testCaseDisplayName,
	IReadOnlyCollection<Func<ICodeGenTestCase, ValueTask<IReadOnlyCollection<ICodeGenTest>>>> testFactories,
	ICodeGenTestMethod testMethod,
	int timeout,
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
	string uniqueID) :
		CodeGenTestCaseBase(
			@explicit,
			skipExceptions,
			skipReason,
			skipUnless,
			skipWhen,
			sourceFilePath,
			sourceLineNumber,
			testCaseDisplayName,
			testMethod,
			timeout,
			traits,
			uniqueID
		)
{
	/// <inheritdoc/>
	public async override ValueTask<IReadOnlyCollection<ICodeGenTest>> CreateTests()
	{
		var result = new List<ICodeGenTest>();

		foreach (var testFactory in testFactories)
			result.AddRange(await testFactory(this));

		return result;
	}
}
