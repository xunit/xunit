using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Contains the information by <see cref="IFrontController.Run"/>.
/// </summary>
/// <param name="options">The options used during execution</param>
/// <param name="serializedTestCases">The test cases to be run</param>
public class FrontControllerRunSettings(
	ITestFrameworkExecutionOptions options,
	IReadOnlyCollection<string> serializedTestCases) :
		FrontControllerSettingsBase
{
	/// <summary>
	/// The options used during execution.
	/// </summary>
	public ITestFrameworkExecutionOptions Options { get; } = Guard.ArgumentNotNull(options);

	/// <summary>
	/// Get the list of test cases to be run.
	/// </summary>
	public IReadOnlyCollection<string> SerializedTestCases { get; } = Guard.ArgumentNotNull(serializedTestCases);
}
