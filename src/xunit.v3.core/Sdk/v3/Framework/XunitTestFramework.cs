namespace Xunit.v3;

/// <summary>
/// The implementation of <see cref="_ITestFramework"/> that supports discovery and
/// execution of unit tests linked against xunit.v3.core.dll.
/// </summary>
public class XunitTestFramework : TestFramework
{
	readonly string? configFileName;

	// Two constructors are required here, because ExtensibilityPointFactory demands a
	// parameterless constructor (Activator.CreateInstance does not match constructors
	// with default values).

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestFramework"/> class.
	/// </summary>
	public XunitTestFramework()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestFramework"/> class.
	/// </summary>
	/// <param name="configFileName">The optional test configuration file.</param>
	public XunitTestFramework(string? configFileName)
	{
		this.configFileName = configFileName;
	}

	/// <inheritdoc/>
	protected override _ITestFrameworkDiscoverer CreateDiscoverer(_IAssemblyInfo assembly) =>
		new XunitTestFrameworkDiscoverer(assembly, configFileName);

	/// <inheritdoc/>
	protected override _ITestFrameworkExecutor CreateExecutor(_IReflectionAssemblyInfo assembly) =>
		new XunitTestFrameworkExecutor(assembly, configFileName);
}
