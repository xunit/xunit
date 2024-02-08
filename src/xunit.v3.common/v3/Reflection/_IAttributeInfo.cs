using System.Collections.Generic;

namespace Xunit.v3;

/// <summary>
/// Represents information about an attribute. The primary implementation is based on runtime
/// reflection, but may also be implemented by runner authors to provide non-reflection-based
/// test discovery (for example, AST-based runners like CodeRush or Resharper).
/// </summary>
public interface _IAttributeInfo
{
	/// <summary>
	/// Gets the type of the attribute.
	/// </summary>
	_ITypeInfo AttributeType { get; }

	/// <summary>
	/// Gets the arguments passed to the constructor.
	/// </summary>
	/// <returns>The constructor arguments, in order</returns>
	IReadOnlyCollection<object?> GetConstructorArguments();

	/// <summary>
	/// Gets all the custom attributes for the attribute that are of the given attribute type.
	/// </summary>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the attribute</returns>
	IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(_ITypeInfo attributeType);

	/// <summary>
	/// Gets a named-argument initialized value of the attribute. If there is no named argument for the given name
	/// on this attribute, then returns <c>default(TValue)</c>.
	/// </summary>
	/// <typeparam name="TValue">The type of the argument</typeparam>
	/// <param name="argumentName">The name of the argument</param>
	/// <returns>The argument value (or <c>default(TValue)</c>)</returns>
	TValue? GetNamedArgument<TValue>(string argumentName);
}
