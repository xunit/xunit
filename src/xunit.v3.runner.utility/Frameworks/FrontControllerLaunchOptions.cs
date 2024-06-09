namespace Xunit;

/// <summary>
/// Launch options when using <see cref="IFrontController"/> and/or <see cref="IFrontControllerDiscoverer"/>.
/// Current only supported by v3 test projects (all options will be ignored for v1/v2 test projects).
/// </summary>
public class FrontControllerLaunchOptions
{
	/// <summary>
	/// Wait for a debugger to be attached before performing any operations.
	/// </summary>
	public bool WaitForDebugger { get; set; }
}
