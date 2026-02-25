using System.ComponentModel;

namespace Xunit.v3;

partial class ExtensibilityPointFactory
{
	/// <summary>
	/// Arbitrary object creation is not supported in Native AOT
	/// </summary>
	[Obsolete("Arbitrary object creation is not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static TInterface? Get<TInterface>(
		Type? type,
		object?[]? ctorArgs = null)
			where TInterface : class =>
				throw new PlatformNotSupportedException("Arbitrary object creation is not supported in Native AOT");

	/// <summary>
	/// Test case discoverers are not used in Native AOT
	/// </summary>
	[Obsolete("Test case discoverers are not used in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IXunitTestCaseDiscoverer? GetXunitTestCaseDiscoverer(Type testCaseDiscovererType) =>
		throw new PlatformNotSupportedException("Test case discoverers are not used in Native AOT");

	// This should be removed in the next major version, since the reflection counterpart is also going away.
	/// <summary>
	/// Please call <see cref="RegisteredEngineConfig.GetTestCollectionFactory"/> in Native AOT
	/// </summary>
	[Obsolete("Please call RegisteredEngineConfig.GetTestCollectionFactory in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IXunitTestCollectionFactory? GetXunitTestCollectionFactory(
		Type? testCollectionFactoryType,
		IXunitTestAssembly testAssembly) =>
			throw new PlatformNotSupportedException("Please call RegisteredEngineConfig.GetTestCollectionFactory in Native AOT");
}
