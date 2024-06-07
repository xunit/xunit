namespace Xunit.Runner.Common;

/// <summary>
/// Represents the target framework identifier that an assembly is targeting
/// </summary>
public enum TargetFrameworkIdentifier
{
	/// <summary>
	/// The target framework is unknown.
	/// </summary>
	UnknownTargetFramework,

	/// <summary>
	/// The target framework is .NET Framework.
	/// </summary>
	DotNetFramework,

	/// <summary>
	/// The target framework is .NET or .NET Core.
	/// </summary>
	DotNetCore,
}
