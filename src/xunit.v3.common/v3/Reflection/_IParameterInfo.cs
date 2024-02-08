using System.Collections.Generic;

namespace Xunit.v3;

/// <summary>
/// Represents information about a method parameter. The primary implementation is based on runtime
/// reflection, but may also be implemented by runner authors to provide non-reflection-based
/// test discovery (for example, AST-based runners like CodeRush or Resharper).
/// </summary>
public interface _IParameterInfo
{
	/// <summary>
	/// Gets a value indicating whether the parameter is optional.
	/// </summary>
	bool IsOptional { get; }

	/// <summary>
	/// The name of the parameter.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the type of the parameter.
	/// </summary>
	_ITypeInfo ParameterType { get; }

	/// <summary>
	/// Gets all the custom attributes for the parameter that are of the given attribute type.
	/// </summary>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the parameter</returns>
	IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(_ITypeInfo attributeType);
}
