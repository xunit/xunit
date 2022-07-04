using System;

namespace Xunit.v3;

/// <summary>
/// Represents a reflection-backed implementation of <see cref="_IAttributeInfo"/>.
/// </summary>
public interface _IReflectionAttributeInfo : _IAttributeInfo
{
	/// <summary>
	/// Gets the instance of the attribute, if available.
	/// </summary>
	Attribute Attribute { get; }
}
