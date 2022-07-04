using System.Reflection;

namespace Xunit.v3;

/// <summary>
/// Represents a reflection-backed implementation of <see cref="_IMethodInfo"/>.
/// </summary>
public interface _IReflectionMethodInfo : _IMethodInfo
{
	/// <summary>
	/// Gets the underlying <see cref="MethodInfo"/> for the method.
	/// </summary>
	MethodInfo MethodInfo { get; }
}
