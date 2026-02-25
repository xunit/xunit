using System.Reflection;

namespace Xunit.v3;

/// <summary>
/// The implementation of <see cref="ITestFramework"/> that supports tests registered via
/// code generation.
/// </summary>
/// <param name="configFileName">The optional test configuration file.</param>
public class CodeGenTestFramework(string? configFileName = null) :
	TestFramework
{
	internal static string DisplayName { get; } =
		string.Format(CultureInfo.InvariantCulture, "xUnit.net v3 {0} (Native AOT)", ThisAssembly.AssemblyInformationalVersion);

	/// <inheritdoc/>
	public override string TestFrameworkDisplayName =>
		DisplayName;

	/// <inheritdoc/>
	protected override ITestFrameworkDiscoverer CreateDiscoverer(Assembly assembly)
	{
		var testAssembly =
			RegisteredEngineConfig.GetTestAssembly(assembly, configFileName)
				?? throw new InvalidOperationException("No test assemblies were registered via code generation");

		return new CodeGenTestFrameworkDiscoverer(testAssembly);
	}

	/// <inheritdoc/>
	protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly)
	{
		var testAssembly =
			RegisteredEngineConfig.GetTestAssembly(assembly, configFileName)
				?? throw new InvalidOperationException("No test assemblies were registered via code generation");

		return new CodeGenTestFrameworkExecutor(testAssembly);
	}
}
