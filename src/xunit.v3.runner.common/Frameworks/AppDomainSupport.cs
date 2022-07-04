namespace Xunit.Runner.Common;

/// <summary>
/// Indicates the level of app domain support that the runner is requesting. Note that these values
/// are only valid for v1 and v2 tests; v3 tests always run in a separate process rather than
/// in the runner process.
/// </summary>
public enum AppDomainSupport
{
	/// <summary>
	/// Requests that app domains be used for v1 and v2 tests, if available; if app domains cannot
	/// be used, then the tests will be discovered and run in the runner's app domain.
	/// </summary>
	IfAvailable = 1,

	/// <summary>
	/// Requires that v1 and v2 tests run in a separate app domain. Can only be requested by runners
	/// written in .NET Framework.
	/// </summary>
	Required = 2,

	/// <summary>
	/// Requires that v1 and v2 tests be run in the runner's app domain.
	/// </summary>
	Denied = 3
}
