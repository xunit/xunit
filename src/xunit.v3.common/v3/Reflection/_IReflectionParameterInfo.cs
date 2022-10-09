using System.Reflection;

namespace Xunit.v3;

/// <summary>
/// Represents a reflection-backed implementation of <see cref="_IParameterInfo"/>.
/// </summary>
public interface _IReflectionParameterInfo : _IParameterInfo
{
	/// <summary>
	/// Gets the underlying <see cref="ParameterInfo"/> for the parameter.
	/// </summary>
	ParameterInfo ParameterInfo { get; }
}
