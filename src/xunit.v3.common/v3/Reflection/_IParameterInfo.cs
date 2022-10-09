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
	/// Retrieves a custom attribute of a specified type that is applied to a specified parameter.
	/// </summary>
	/// <param name="attributeType">The type of attribute to search for.</param>
	/// <returns>A custom attribute that matches <paramref name="attributeType"/> if found; <c>null</c>, otherwise.</returns>
	_IAttributeInfo? GetCustomAttribute(_ITypeInfo attributeType);
}
