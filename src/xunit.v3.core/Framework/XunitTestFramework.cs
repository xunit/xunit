using System.Globalization;
using System.Reflection;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// The implementation of <see cref="ITestFramework"/> that supports discovery and
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
	public XunitTestFramework(string? configFileName) =>
		this.configFileName = configFileName;

	internal static string DisplayName { get; } =
		string.Format(CultureInfo.InvariantCulture, "xUnit.net v3 {0}", ThisAssembly.AssemblyInformationalVersion);

	/// <inheritdoc/>
	public override string TestFrameworkDisplayName =>
		DisplayName;

	/// <inheritdoc/>
	protected override ITestFrameworkDiscoverer CreateDiscoverer(Assembly assembly) =>
		new XunitTestFrameworkDiscoverer(new XunitTestAssembly(Guard.ArgumentNotNull(assembly), configFileName, assembly.GetName().Version));

	/// <inheritdoc/>
	protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly) =>
		new XunitTestFrameworkExecutor(new XunitTestAssembly(Guard.ArgumentNotNull(assembly), configFileName, assembly.GetName().Version));
}
