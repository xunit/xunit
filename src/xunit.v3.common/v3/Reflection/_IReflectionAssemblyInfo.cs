using System.Reflection;

namespace Xunit.v3;

/// <summary>
/// Represents a reflection-backed implementation of <see cref="_IAssemblyInfo"/>.
/// </summary>
public interface _IReflectionAssemblyInfo : _IAssemblyInfo
{
	/// <summary>
	/// Gets the underlying <see cref="Assembly"/> for the assembly.
	/// </summary>
	Assembly Assembly { get; }
}
