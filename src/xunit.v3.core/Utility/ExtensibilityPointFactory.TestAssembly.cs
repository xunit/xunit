using System.ComponentModel;
using System.Reflection;

namespace Xunit.v3;

partial class ExtensibilityPointFactory
{
	/// <summary>
	/// Please call <see cref="RegisteredEngineConfig.GetAssemblyTestCaseOrderer"/>.
	/// This method will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call RegisteredEngineConfig.GetAssemblyTestCaseOrderer. This method will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static ITestCaseOrderer? GetAssemblyTestCaseOrderer(Assembly testAssembly) =>
		RegisteredEngineConfig.GetAssemblyTestCaseOrderer(testAssembly);

	/// <summary>
	/// Please call <see cref="RegisteredEngineConfig.GetAssemblyTestCollectionOrderer"/>.
	/// This method will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call RegisteredEngineConfig.GetAssemblyTestCollectionOrderer. This method will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static ITestCollectionOrderer? GetAssemblyTestCollectionOrderer(Assembly testAssembly) =>
		RegisteredEngineConfig.GetAssemblyTestCollectionOrderer(testAssembly);
}
