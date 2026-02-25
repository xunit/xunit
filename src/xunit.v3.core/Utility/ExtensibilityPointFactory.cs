using System.ComponentModel;
using System.Reflection;

namespace Xunit.v3;

// Extensibility point factory helper methods, and factories that don't fit in any other categorization

/// <summary>
/// Represents a factory for the types used for extensibility throughout the system.
/// </summary>
public static partial class ExtensibilityPointFactory
{
	/// <summary>
	/// Please call <see cref="RegisteredEngineConfig.GetTestFramework"/>.
	/// This method will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call RegisteredEngineConfig.GetTestFramework. This method will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static ITestFramework GetTestFramework(Assembly testAssembly) =>
		RegisteredEngineConfig.GetTestFramework(testAssembly, configFileName: null);
}
