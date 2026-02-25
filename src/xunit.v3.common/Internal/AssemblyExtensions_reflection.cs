#nullable enable  // This file is temporarily shared with xunit.v1.tests and xunit.v2.tests, which are not nullable-enabled

using System.Reflection;

namespace Xunit.Internal;

partial class AssemblyExtensions
{
	/// <summary/>
	[return: NotNullIfNotNull(nameof(assembly))]
	public static string? GetLocalCodeBase(this Assembly? assembly) =>
		GetLocalCodeBase(assembly?.GetSafeCodeBase(), Path.DirectorySeparatorChar);

	/// <summary>
	/// Safely gets the code base of an assembly.
	/// </summary>
	/// <param name="assembly">The assembly.</param>
	/// <returns>If the assembly is null, or is dynamic, then it returns <see langword="null"/>; otherwise, it returns the value
	/// from <see cref="Assembly.CodeBase"/>.</returns>
	[return: NotNullIfNotNull(nameof(assembly))]
	public static string? GetSafeCodeBase(this Assembly? assembly) =>
		assembly?.CodeBase;

	/// <summary>
	/// Safely gets the location of an assembly.
	/// </summary>
	/// <param name="assembly">The assembly.</param>
	/// <returns>If the assembly is null, or is dynamic, then it returns <see langword="null"/>; otherwise, it returns the value
	/// from <see cref="Assembly.Location"/>.</returns>
	[return: NotNullIfNotNull(nameof(assembly))]
	public static string? GetSafeLocation(this Assembly? assembly) =>
		assembly?.Location;
}
