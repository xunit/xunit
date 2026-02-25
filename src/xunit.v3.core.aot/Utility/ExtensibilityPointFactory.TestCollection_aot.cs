using System.ComponentModel;
using System.Reflection;

namespace Xunit.v3;

// Extensibility point factories related to test collections (AOT)

partial class ExtensibilityPointFactory
{
	/// <summary>
	/// BeforeAfterTestAttribute instances are collected by the source generator in Native AOT
	/// </summary>
	[Obsolete("BeforeAfterTestAttribute instances are collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<IBeforeAfterTestAttribute> GetCollectionBeforeAfterTestAttributes(
		Type? collectionDefinition,
		IReadOnlyCollection<IBeforeAfterTestAttribute> assemblyBeforeAfterTestAttributes) =>
			throw new PlatformNotSupportedException("BeforeAfterTestAttribute instances are collected by the source generator in Native AOT");

	/// <summary>
	/// The CollectionBehaviorAttribute is collected by the source generator in Native AOT
	/// </summary>
	[Obsolete("The CollectionBehaviorAttribute is collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static ICollectionBehaviorAttribute? GetCollectionBehavior(Assembly testAssembly) =>
		throw new PlatformNotSupportedException("The CollectionBehaviorAttribute is collected by the source generator in Native AOT");

	/// <summary>
	/// Class fixtures are collected by the source generator in Native AOT
	/// </summary>
	[Obsolete("Class fixtures are collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Type> GetCollectionClassFixtureTypes(Type? collectionDefinition) =>
		throw new PlatformNotSupportedException("Class fixtures are collected by the source generator in Native AOT");

	/// <summary>
	/// Collection fixtures are collected by the source generator in Native AOT
	/// </summary>
	[Obsolete("Collection fixtures are collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Type> GetCollectionCollectionFixtureTypes(Type? collectionDefinition) =>
		throw new PlatformNotSupportedException("Collection fixtures are collected by the source generator in Native AOT");

	/// <summary>
	/// Collection definitions are collected by the source generator in Native AOT
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	[Obsolete("Collection definitions are collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyDictionary<string, (Type Type, CollectionDefinitionAttribute Attribute)> GetCollectionDefinitions(Assembly testAssembly) =>
		throw new PlatformNotSupportedException("Collection definitions are collected by the source generator in Native AOT");

	/// <summary>
	/// Collection traits are collected by the source generator in Native AOT
	/// </summary>
	/// <param name="testCollectionDefinition">The test collection</param>
	/// <param name="testAssemblyTraits">The traits inherited from the test assembly</param>
	[Obsolete("Collection traits are collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetCollectionTraits(
		Type? testCollectionDefinition,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? testAssemblyTraits) =>
			throw new PlatformNotSupportedException("Collection traits are collected by the source generator in Native AOT");
}
