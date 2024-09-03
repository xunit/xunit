using System;
using Xunit.Internal;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents metadata about an assembly.
/// </summary>
public class AssemblyMetadata
{
	static readonly Version versionZero = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="AssemblyMetadata"/> class.
	/// </summary>
	/// <param name="xunitVersion">The xUnit.net version (0 = unknown, or 1/2/3)</param>
	/// <param name="targetFramework">The target framework</param>
	public AssemblyMetadata(
		int xunitVersion,
		string? targetFramework)
	{
		Guard.ArgumentValid("xunitVersion must be between 0 and 3", xunitVersion is >= 0 and <= 3, nameof(xunitVersion));

		XunitVersion = xunitVersion;
		TargetFrameworkIdentifier = TargetFrameworkIdentifier.UnknownTargetFramework;
		TargetFrameworkVersion = versionZero;

		if (targetFramework is not null)
		{
			var pieces = targetFramework.Split(',');
			TargetFrameworkIdentifier = pieces[0].ToUpperInvariant() switch
			{
				".NETFRAMEWORK" => TargetFrameworkIdentifier.DotNetFramework,
				".NETCOREAPP" => TargetFrameworkIdentifier.DotNetCore,
				_ => TargetFrameworkIdentifier.UnknownTargetFramework,
			};

			if (pieces.Length > 1 &&
					pieces[1].StartsWith("Version=v", StringComparison.OrdinalIgnoreCase) &&
					Version.TryParse(pieces[1].Substring(9), out var targetFrameworkVersion))
				TargetFrameworkVersion = targetFrameworkVersion;
		}
	}

	/// <summary>
	/// Gets the target framework identifier the assembly was built against.
	/// </summary>
	public TargetFrameworkIdentifier TargetFrameworkIdentifier { get; }

	/// <summary>
	/// Gets the version of the target framework identifier that the assembly was built against.
	/// </summary>
	public Version TargetFrameworkVersion { get; }

	/// <summary>
	/// Gets the major version of xUnit.net this assembly targets (<c>1</c>, <c>2</c>, or <c>3</c>); may return a value
	/// of <c>0</c> if the version is unknown.
	/// </summary>
	public int XunitVersion { get; }
}
