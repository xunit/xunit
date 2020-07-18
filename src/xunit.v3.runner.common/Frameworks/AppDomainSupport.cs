namespace Xunit.Runner.Common
{
	/// <summary>
	/// Indicates the level of app domain support that the runner is requesting.
	/// </summary>
	public enum AppDomainSupport
	{
		/// <summary>
		/// Requests that app domains be used for v1 and v2 tests, if available; if app domains cannot
		/// be used, then the tests will be discovered and run in the runner's app domain.
		/// </summary>
		IfAvailable = 1,

#if NETFRAMEWORK
		/// <summary>
		/// Requires that v1 and v2 tests run in a separate app domain. Can only be requested by runners
		/// written in .NET Framework.
		/// </summary>
		Required = 2,
#endif

		/// <summary>
		/// Requires that v1 and v2 tests be run in the runner's app domain.
		/// </summary>
		Denied = 3
	}
}
