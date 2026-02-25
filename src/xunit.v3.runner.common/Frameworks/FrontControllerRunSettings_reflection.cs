using Xunit.Sdk;

namespace Xunit;

partial class FrontControllerRunSettings
{
	/// <summary>
	/// Get the list of test cases to be run.
	/// </summary>
	public IReadOnlyCollection<string> SerializedTestCases { get; }

	/// <summary>
	/// Creates a new instance of <see cref="FrontControllerRunSettings"/> to run a set of test cases
	/// by serialization.
	/// </summary>
	/// <param name="options">The options to use during execution</param>
	/// <param name="serializedTestCases">The test cases to run</param>
	public static FrontControllerRunSettings WithSerializedTestCases(
		ITestFrameworkExecutionOptions options,
		IReadOnlyCollection<string> serializedTestCases) =>
			new(options, serializedTestCases, []);

	/// <summary>
	/// Creates a new instance of <see cref="FrontControllerRunSettings"/> to run a set of test cases
	/// by serialization and a set of test cases by test case ID.
	/// </summary>
	/// <param name="options">The options to use during execution</param>
	/// <param name="serializedTestCases">The test cases to run</param>
	/// <param name="testCaseIDsToRun">The test case IDs to run</param>
	public static FrontControllerRunSettings WithSerializedTestCasesAndTestCaseIDs(
		ITestFrameworkExecutionOptions options,
		IReadOnlyCollection<string> serializedTestCases,
		IReadOnlyCollection<string> testCaseIDsToRun) =>
			new(options, serializedTestCases, testCaseIDsToRun);
}
