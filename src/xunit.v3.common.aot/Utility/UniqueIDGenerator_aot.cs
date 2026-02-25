using System.ComponentModel;

namespace Xunit.Sdk;

partial class UniqueIDGenerator
{
	/// <summary>
	/// This overload requires access to serialization, which is not supported in Native AOT.
	/// Call <see cref="ForTestCase(string, int)"/> instead.
	/// </summary>
	[Obsolete("This overload requires access to serialization, which is not supported in Native AOT; call the overload with index instead", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static string ForTestCase(
		string parentUniqueID,
		Type[]? testMethodGenericTypes,
		object?[]? testMethodArguments) =>
			throw new PlatformNotSupportedException("This overload requires access to serialization, which is not supported in Native AOT; call the overload with index instead");
}
