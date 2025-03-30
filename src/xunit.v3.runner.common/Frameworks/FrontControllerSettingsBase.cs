namespace Xunit;

/// <summary>
/// Base class for all invocations of <see cref="IFrontController"/> and <see cref="IFrontControllerDiscoverer"/>.
/// </summary>
public class FrontControllerSettingsBase
{
	/// <summary>
	/// Launch options. Currently only applicable to v3 test projects.
	/// </summary>
	public FrontControllerLaunchOptions LaunchOptions { get; } = new();
}
