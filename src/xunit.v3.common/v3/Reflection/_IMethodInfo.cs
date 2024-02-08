using System.Collections.Generic;

namespace Xunit.v3;

/// <summary>
/// Represents information about a method. The primary implementation is based on runtime
/// reflection, but may also be implemented by runner authors to provide non-reflection-based
/// test discovery (for example, AST-based runners like CodeRush or Resharper).
/// </summary>
public interface _IMethodInfo
{
	/// <summary>
	/// Gets a value indicating whether the method is abstract.
	/// </summary>
	bool IsAbstract { get; }

	/// <summary>
	/// Gets a value indicating whether the method is a generic definition (i.e., an open generic).
	/// </summary>
	bool IsGenericMethodDefinition { get; }

	/// <summary>
	/// Gets a value indicating whether the method is public.
	/// </summary>
	bool IsPublic { get; }

	/// <summary>
	/// Gets a value indicating whether the method is static.
	/// </summary>
	bool IsStatic { get; }

	/// <summary>
	/// Gets the name of the method.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the fully qualified type name of the return type.
	/// </summary>
	_ITypeInfo ReturnType { get; }

	/// <summary>
	/// Gets a value which represents the class that this method was
	/// reflected from (i.e., equivalent to MethodInfo.ReflectedType)
	/// </summary>
	_ITypeInfo Type { get; }

	/// <summary>
	/// Gets all the custom attributes for the method that are of the given type.
	/// </summary>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the method</returns>
	IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(_ITypeInfo attributeType);

	/// <summary>
	/// Gets the types of the generic arguments for generic methods.
	/// </summary>
	/// <returns>The argument types.</returns>
	IReadOnlyCollection<_ITypeInfo> GetGenericArguments();

	/// <summary>
	/// Gets information about the parameters to the method.
	/// </summary>
	/// <returns>The method's parameters.</returns>
	IReadOnlyCollection<_IParameterInfo> GetParameters();

	/// <summary>
	/// Converts an open generic method into a closed generic method, using the provided type arguments.
	/// </summary>
	/// <param name="typeArguments">The type arguments to be used in the generic definition.</param>
	/// <returns>A new <see cref="_IMethodInfo"/> that represents the closed generic method.</returns>
	_IMethodInfo MakeGenericMethod(params _ITypeInfo[] typeArguments);
}
