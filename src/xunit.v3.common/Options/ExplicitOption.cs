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

/// <summary>
/// Extension methods for <see cref="ExplicitOption"/>
/// </summary>
public static class ExplicitOptionExtensions
{
	static readonly HashSet<ExplicitOption> validValues =
	[
		ExplicitOption.Off,
		ExplicitOption.On,
		ExplicitOption.Only,
	];

	/// <summary>
	/// Determines if the value is a valid enum value.
	/// </summary>
	public static bool IsValid(this ExplicitOption value) =>
		validValues.Contains(value);
}
