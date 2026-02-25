using System.ComponentModel;

namespace Xunit.v3;

partial class ExtensibilityPointFactory
{
	/// <summary>
	/// Please call <see cref="RegisteredEngineConfig.GetClassTestCaseOrderer"/>.
	/// This method will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call RegisteredEngineConfig.GetClassTestCaseOrderer. This method will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static ITestCaseOrderer? GetClassTestCaseOrderer(Type testClass) =>
		RegisteredEngineConfig.GetClassTestCaseOrderer(testClass);
}
