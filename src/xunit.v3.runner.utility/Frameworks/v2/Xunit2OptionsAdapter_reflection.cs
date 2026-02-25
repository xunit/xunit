namespace Xunit.Runner.v2;

/// <summary>
/// Class used to adapt v3 discovery and execution options into v2 versions.
/// </summary>
public static class Xunit2OptionsAdapter
{
	/// <summary>
	/// Adapts v3 framework discovery options into v2 framework discovery options.
	/// </summary>
	public static Abstractions.ITestFrameworkDiscoveryOptions Adapt(Sdk.ITestFrameworkDiscoveryOptions options) =>
		new Xunit2Options(options);

	/// <summary>
	/// Adapts v3 framework execution options into v2 framework execution options.
	/// </summary>
	public static Abstractions.ITestFrameworkExecutionOptions Adapt(Sdk.ITestFrameworkExecutionOptions options) =>
		new Xunit2Options(options);
}
