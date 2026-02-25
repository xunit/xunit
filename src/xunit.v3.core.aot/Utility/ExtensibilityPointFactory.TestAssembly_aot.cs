using System.ComponentModel;
using System.Reflection;

namespace Xunit.v3;

// Extensibility point factories related to test assemblies (AOT)

partial class ExtensibilityPointFactory
{
	/// <summary>
	/// BeforeAfterTestAttribute instances are collected by the source generator in Native AOT
	/// </summary>
	[Obsolete("BeforeAfterTestAttribute instances are collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<IBeforeAfterTestAttribute> GetAssemblyBeforeAfterTestAttributes(Assembly testAssembly) =>
		throw new PlatformNotSupportedException("BeforeAfterTestAttribute instances are collected by the source generator in Native AOT");

	/// <summary>
	/// AssemblyFixtureAttribute instances are collected by the source generator in Native AOT
	/// </summary>
	[Obsolete("AssemblyFixtureAttribute instances are collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Type> GetAssemblyFixtureTypes(Assembly testAssembly) =>
		throw new PlatformNotSupportedException("AssemblyFixtureAttribute instances are collected by the source generator in Native AOT");

	/// <summary>
	/// Assembly-level traits are collected during test assembly registration in Native AOT
	/// </summary>
	[Obsolete("Assembly-level traits are collected during test assembly registration in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetAssemblyTraits(Assembly testAssembly) =>
		throw new PlatformNotSupportedException("Assembly-level traits are collected during test assembly registration in Native AOT");
}
