using System;

namespace Xunit.v3;

/// <summary>
/// Represents a reflection-backed implementation of <see cref="_ITypeInfo"/>.
/// </summary>
public interface _IReflectionTypeInfo : _ITypeInfo
{
	/// <summary>
	/// Gets the underlying <see cref="Type"/> object.
	/// </summary>
	Type Type { get; }
}
