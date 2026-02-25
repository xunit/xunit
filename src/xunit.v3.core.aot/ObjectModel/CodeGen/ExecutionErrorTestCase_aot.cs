using System.Runtime.ExceptionServices;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A simple implementation of <see cref="ICodeGenTestCase"/> that can be used to report an error
/// rather than running a test.
/// </summary>
/// <param name="exception">The exception to fail the test case with</param>
/// <param name="explicit">A flag indicating if this is an explicit test</param>
/// <param name="sourceFilePath">The source file path for the test method (if known)</param>
/// <param name="sourceLineNumber">The line number for the test method (if known)</param>
/// <param name="testCaseDisplayName">The display name for the test case</param>
/// <param name="testMethod">The test method this test case belongs to</param>
/// <param name="traits">The traits of the test case</param>
/// <param name="uniqueID">The test case unique ID</param>
public class ExecutionErrorTestCase(
	Exception exception,
	bool @explicit,
	string? sourceFilePath,
	int? sourceLineNumber,
	string testCaseDisplayName,
	ICodeGenTestMethod testMethod,
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
	string uniqueID) :
		CodeGenTestCaseBase(
			@explicit,
			skipExceptions: null,
			skipReason: null,
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
	/// Initialize a new instance of the <see cref="ExecutionErrorTestCase"/> class, which will throw
	/// a <see cref="TestPipelineException"/> with the given error message.
	/// </summary>
	/// <param name="errorMessage">The error message to fail the test case with</param>
	/// <param name="explicit">A flag indicating if this is an explicit test</param>
	/// <param name="sourceFilePath">The source file path for the test method (if known)</param>
	/// <param name="sourceLineNumber">The line number for the test method (if known)</param>
	/// <param name="testCaseDisplayName">The display name for the test case</param>
	/// <param name="testMethod">The test method this test case belongs to</param>
	/// <param name="traits">The traits of the test case</param>
	/// <param name="uniqueID">The test case unique ID</param>
	public ExecutionErrorTestCase(
		string errorMessage,
		bool @explicit,
		string? sourceFilePath,
		int? sourceLineNumber,
		string testCaseDisplayName,
		ICodeGenTestMethod testMethod,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
		string uniqueID) :
			this(new TestPipelineException(errorMessage), @explicit, sourceFilePath, sourceLineNumber, testCaseDisplayName, testMethod, traits, uniqueID)
	{ }

	/// <inheritdoc/>
	public override ValueTask<IReadOnlyCollection<ICodeGenTest>> CreateTests()
	{
		ExceptionDispatchInfo.Throw(exception);
		return default;  // Unreachable code, but the compiler seems to ignore [DoesNotReturn]
	}
}
