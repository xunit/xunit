namespace Xunit.Sdk;

/// <summary>
/// Indicates how explicit tests should be handled during execution.
/// </summary>
public enum ExplicitOption
{
	/// <summary>
	/// Indicates the non-explicit tests should be run, and explicit tests should not be run. This
	/// should be the default behavior in most runners.
	/// </summary>
	Off,

	/// <summary>
	/// Indicates that both non-explicit and explicit tests should be run.
	/// </summary>
	On,

	/// <summary>
	/// Indicates that non-explicit tests should not be run, and explicit tests should be run.
	/// </summary>
	Only,
}
