using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A simple implementation of <see cref="ICodeGenTestCase"/> which generates a single skipped test.
/// </summary>
/// <param name="explicit">A flag which indicates whether the test case is marked as explicit</param>
/// <param name="skipReason">The skip reason</param>
/// <param name="sourceFilePath">The optional source file pathi</param>
/// <param name="sourceLineNumber">The optional source line number</param>
/// <param name="testCaseDisplayName">The display name for the test case (and test)</param>
/// <param name="testMethod">The test method the test case belongs to</param>
/// <param name="traits">The test case's traits</param>
/// <param name="uniqueID">The unique ID of the test case</param>
public class SkipTestCase(
	bool @explicit,
	string skipReason,
	string? sourceFilePath,
	int? sourceLineNumber,
	string testCaseDisplayName,
	ICodeGenTestMethod testMethod,
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
	string uniqueID) :
		CodeGenTestCaseBase(
			@explicit,
			skipExceptions: null,
			skipReason,
			skipUnless: null,
			skipWhen: null,
			sourceFilePath,
			sourceLineNumber,
			testCaseDisplayName,
			testMethod,
			timeout: 0,
			traits,
			uniqueID
		)
{
	/// <summary>
	/// Generates a single skipped test.
	/// </summary>
	public override async ValueTask<IReadOnlyCollection<ICodeGenTest>> CreateTests() =>
		[new CodeGenTest(
			Explicit,
			obj => default,
			SkipReason,
			SkipUnless,
			SkipWhen,
			this,
			TestCaseDisplayName,
			testLabel: null,
			Timeout,
			Traits,
			UniqueIDGenerator.ForTest(UniqueID, testIndex: -1)
		)];
}
