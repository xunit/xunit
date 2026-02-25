using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Contains the information by <see cref="IFrontController.Run"/>.
/// </summary>
public partial class FrontControllerRunSettings : FrontControllerSettingsBase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="FrontControllerRunSettings"/> class.
	/// </summary>
	[Obsolete("This constructor has been deprecated. Please use one of the static factory functions (e.g., WithTestCaseIDs).", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public FrontControllerRunSettings(
		ITestFrameworkExecutionOptions options,
		IReadOnlyCollection<string> serializedTestCases) =>
			throw new NotSupportedException("This constructor has been deprecated. Please use one of the static factory functions (e.g., WithTestCaseIDs).");

	FrontControllerRunSettings(
		ITestFrameworkExecutionOptions options,
		IReadOnlyCollection<string> serializedTestCases,
		IReadOnlyCollection<string> testCaseIDsToRun)
	{
		Options = Guard.ArgumentNotNull(options);
#if !XUNIT_AOT
		SerializedTestCases = Guard.ArgumentNotNull(serializedTestCases);
#endif
		TestCaseIDsToRun = Guard.ArgumentNotNull(testCaseIDsToRun);
	}

	/// <summary>
	/// The options used during execution.
	/// </summary>
	public ITestFrameworkExecutionOptions Options { get; }

	/// <summary>
	/// Get the list of test case IDs to be run.
	/// </summary>
	public IReadOnlyCollection<string> TestCaseIDsToRun { get; }

	/// <summary>
	/// Creates a new instance of <see cref="FrontControllerRunSettings"/> to run a set of test cases
	/// by test case ID.
	/// </summary>
	/// <param name="options">The options to use during execution</param>
	/// <param name="testCaseIDsToRun">The test case IDs to run</param>
	public static FrontControllerRunSettings WithTestCaseIDs(
		ITestFrameworkExecutionOptions options,
		IReadOnlyCollection<string> testCaseIDsToRun) =>
			new(options, [], testCaseIDsToRun);
}
