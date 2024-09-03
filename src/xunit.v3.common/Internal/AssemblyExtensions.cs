#nullable enable  // This file is temporarily shared with xunit.v1.tests and xunit.v2.tests, which are not nullable-enabled

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class AssemblyExtensions
{
	/// <summary>
	/// Gets the value for an unknown target framework.
	/// </summary>
	public const string UnknownTargetFramework = "UnknownTargetFramework";

	/// <summary/>
	[return: NotNullIfNotNull(nameof(assembly))]
	public static string? GetLocalCodeBase(this Assembly? assembly) =>
		GetLocalCodeBase(assembly?.GetSafeCodeBase(), Path.DirectorySeparatorChar);

	/// <summary/>
	[return: NotNullIfNotNull(nameof(codeBase))]
	public static string? GetLocalCodeBase(
		string? codeBase,
		char directorySeparator)
	{
		if (codeBase is null)
			return null;

		if (!codeBase.StartsWith("file://", StringComparison.Ordinal))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Codebase '{0}' is unsupported; must start with 'file://'.", codeBase), nameof(codeBase));

		// "file:///path" is a local path; "file://machine/path" is a UNC
		var localFile = codeBase.Length > 7 && codeBase[7] == '/';

		// POSIX-style directories
		if (directorySeparator == '/')
			return localFile
				? codeBase.Substring(7)
				: throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "UNC-style codebase '{0}' is not supported on POSIX-style file systems.", codeBase), nameof(codeBase));

		// Windows-style directories
		if (directorySeparator == '\\')
		{
			codeBase = codeBase.Replace('/', '\\');
			return localFile ? codeBase.Substring(8) : codeBase.Substring(5);
		}

		throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Unknown directory separator '{0}'; must be one of '/' or '\\'.", directorySeparator), nameof(directorySeparator));
	}

	/// <summary>
	/// Safely gets the code base of an assembly.
	/// </summary>
	/// <param name="assembly">The assembly.</param>
	/// <returns>If the assembly is null, or is dynamic, then it returns <c>null</c>; otherwise, it returns the value
	/// from <see cref="Assembly.CodeBase"/>.</returns>
	[return: NotNullIfNotNull(nameof(assembly))]
	public static string? GetSafeCodeBase(this Assembly? assembly) =>
		assembly?.CodeBase;

	/// <summary>
	/// Safely gets the location of an assembly.
	/// </summary>
	/// <param name="assembly">The assembly.</param>
	/// <returns>If the assembly is null, or is dynamic, then it returns <c>null</c>; otherwise, it returns the value
	/// from <see cref="Assembly.Location"/>.</returns>
	[return: NotNullIfNotNull(nameof(assembly))]
	public static string? GetSafeLocation(this Assembly? assembly) =>
		assembly?.Location;

	/// <summary>
	/// Gets the target framework name for the given assembly.
	/// </summary>
	/// <param name="assembly">The assembly.</param>
	/// <returns>The target framework (typically in a format like ".NETFramework,Version=v4.7.2"
	/// or ".NETCoreApp,Version=v6.0"). If the target framework type is unknown (missing file,
	/// missing attribute, etc.) then returns "UnknownTargetFramework".</returns>
	public static string GetTargetFramework(this Assembly assembly)
	{
		Guard.ArgumentNotNull(assembly);

		var targetFrameworkAttribute = assembly.GetCustomAttribute<TargetFrameworkAttribute>();
		return targetFrameworkAttribute is not null ? targetFrameworkAttribute.FrameworkName : UnknownTargetFramework;
	}
}
