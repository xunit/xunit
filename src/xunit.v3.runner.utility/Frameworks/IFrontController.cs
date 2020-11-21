using Xunit.v3;

namespace Xunit
{
	/// <summary>
	/// Represents a class which acts as a front controller for unit testing frameworks.
	/// This allows runners to run tests from multiple unit testing frameworks (in particular,
	/// hiding the differences between xUnit.net v1, v2, and v3 tests).
	/// </summary>
	public interface IFrontController : _ITestFrameworkDiscoverer, _ITestFrameworkExecutor
	{
		/// <summary>
		/// Gets a flag indicating whether this discovery/execution can use app domains.
		/// </summary>
		bool CanUseAppDomains { get; }
	}
}
