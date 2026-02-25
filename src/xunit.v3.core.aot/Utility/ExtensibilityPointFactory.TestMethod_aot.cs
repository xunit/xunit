using System.ComponentModel;
using System.Reflection;

namespace Xunit.v3;

// Extensibility points related to test methods (AOT)

public static partial class ExtensibilityPointFactory
{
	/// <summary>
	/// BeforeAfterTestAttribute instances are collected by the source generator in Native AOT
	/// </summary>
	[Obsolete("BeforeAfterTestAttribute instances are collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<IBeforeAfterTestAttribute> GetMethodBeforeAfterTestAttributes(
		MethodInfo testMethod,
		IReadOnlyCollection<IBeforeAfterTestAttribute> classBeforeAfterAttributes) =>
			throw new PlatformNotSupportedException("BeforeAfterTestAttribute instances are collected by the source generator in Native AOT");

	/// <summary>
	/// Data attributes are collected by the source generator in Native AOT
	/// </summary>
	[Obsolete("Data attributes are collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<IDataAttribute> GetMethodDataAttributes(MethodInfo testMethod) =>
		throw new PlatformNotSupportedException("Data attributes are collected by the source generator in Native AOT");

	/// <summary>
	/// Test methods are collected by the source generator in Native AOT
	/// </summary>
	[Obsolete("Test methods are collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<IFactAttribute> GetMethodFactAttributes(MethodInfo testMethod) =>
		throw new PlatformNotSupportedException("Test methods are collected by the source generator in Native AOT");

	/// <summary>
	/// Test method traits are collected by the source generator in Native AOT
	/// </summary>
	[Obsolete("Test method traits are collected by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetMethodTraits(
		MethodInfo testMethod,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? testClassTraits) =>
			throw new PlatformNotSupportedException("Test method traits are collected by the source generator in Native AOT");
}
