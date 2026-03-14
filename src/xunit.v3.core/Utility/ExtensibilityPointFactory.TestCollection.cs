using System.ComponentModel;

namespace Xunit.v3;

partial class ExtensibilityPointFactory
{
	/// <summary>
	/// Please call <see cref="RegisteredEngineConfig.GetCollectionTestCaseOrderer"/>.
	/// This method will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call RegisteredEngineConfig.GetCollectionTestCaseOrderer. This method will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static ITestCaseOrderer? GetCollectionTestCaseOrderer(Type? collectionDefinition) =>
		RegisteredEngineConfig.GetCollectionTestCaseOrderer(collectionDefinition);
}
