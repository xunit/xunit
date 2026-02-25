using System.Runtime.InteropServices;

namespace Xunit.v3;

/// <summary>
/// Helpers for code generation-based testing.
/// </summary>
public static class CodeGenHelper
{
	/// <summary>
	/// Gets an empty collection definition.
	/// </summary>
	public static IReadOnlyDictionary<string, (Type? Type, bool DisableParallelization)> EmptyCollectionDefinitions { get; } =
		new Dictionary<string, (Type? Type, bool DisableParallelization)>();

	/// <summary>
	/// Gets an empty fixture factory list.
	/// </summary>
	public static IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> EmptyFixtureFactories { get; } =
		new Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>();

	/// <summary>
	/// Gets an empty traits dictionary.
	/// </summary>
	public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> EmptyTraits { get; } =
		new Dictionary<string, IReadOnlyCollection<string>>();

	/// <summary>
	/// Gets the executable extension expected for a Native AOT test application.
	/// </summary>
	/// <remarks>
	/// This will return <c>".exe"</c> when running on Windows, and <see cref="string.Empty"/> for all other OSes.
	/// </remarks>
	public static string ExecutableExtension { get; } =
		RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
			? ".exe"
			: string.Empty;
}
