using System.ComponentModel;

namespace Xunit.v3;

// Extensibility point factories related to test classes (AOT)

partial class ExtensibilityPointFactory
{
	/// <summary>
	/// BeforeAfterTestAttribute instances are collected by the source generator in Native AOT
	/// </summary>
	[Obsolete("BeforeAfterTestAttribute instances are collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<IBeforeAfterTestAttribute> GetClassBeforeAfterTestAttributes(
		Type testClass,
		IReadOnlyCollection<IBeforeAfterTestAttribute> collectionBeforeAfterAttributes) =>
			throw new PlatformNotSupportedException("BeforeAfterTestAttribute instances are collected by the source generator in Native AOT");

	/// <summary>
	/// Class fixtures are collected by the source generator in Native AOT
	/// </summary>
	[Obsolete("Class fixtures are collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Type> GetClassClassFixtureTypes(
		Type testClass,
		IReadOnlyCollection<Type> collectionClassFixtureTypes) =>
			throw new PlatformNotSupportedException("Class fixtures are collected by the source generator in Native AOT");

	/// <summary>
	/// Test class traits are collected by the source generator in Native AOT
	/// </summary>
	[Obsolete("Test class traits are collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetClassTraits(
		Type? testClass,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? testCollectionTraits) =>
			throw new PlatformNotSupportedException("Test class traits are collected by the source generator in Native AOT");
}
