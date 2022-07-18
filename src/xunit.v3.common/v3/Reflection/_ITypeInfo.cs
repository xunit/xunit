using System;
using System.Collections.Generic;
using System.Reflection;

namespace Xunit.v3;

/// <summary>
/// Represents information about a type. The primary implementation is based on runtime
/// reflection, but may also be implemented by runner authors to provide non-reflection-based
/// test discovery (for example, AST-based runners like CodeRush or Resharper).
/// </summary>
public interface _ITypeInfo
{
	/// <summary>
	/// Gets the assembly this type is located in.
	/// </summary>
	_IAssemblyInfo Assembly { get; }

	/// <summary>
	/// Gets the base type of the given type. Will be <c>null</c> if this type represents
	/// <see cref="object"/>; otherwise, will not be <c>null</c>.
	/// </summary>
	_ITypeInfo? BaseType { get; }

	/// <summary>
	/// Gets the interfaces implemented by the given type.
	/// </summary>
	IReadOnlyCollection<_ITypeInfo> Interfaces { get; }

	/// <summary>
	/// Gets a value indicating whether the type is abstract.
	/// </summary>
	bool IsAbstract { get; }

	/// <summary>
	/// Gets a value indicating whether the type is an array.
	/// </summary>
	bool IsArray { get; }

	/// <summary>
	/// Gets a value indicating whether the type is an enum.
	/// </summary>
	bool IsEnum { get; }

	/// <summary>
	/// Gets a value indicating whether the type represents a generic parameter.
	/// </summary>
	bool IsGenericParameter { get; }

	/// <summary>
	/// Gets a value indicating whether the type is a generic type.
	/// </summary>
	bool IsGenericType { get; }

	/// <summary>
	/// Gets a value indicating whether the type is a generic type definition (aka an "open generic"
	/// like IEnumerable&lt;&gt;).
	/// </summary>
	bool IsGenericTypeDefinition { get; }

	/// <summary>
	/// Gets a value indicating whether the type is an interface.
	/// </summary>
	bool IsInterface { get; }

	/// <summary>
	/// Gets a value indicating whether the type is sealed.
	/// </summary>
	bool IsSealed { get; }

	/// <summary>
	/// Gets a value indicating whether the type is a value type.
	/// </summary>
	bool IsValueType { get; }

	/// <summary>
	/// Gets the fully qualified type name (for non-generic parameters), or the
	/// simple type name (for generic parameters). This maps to <see cref="Type.FullName"/>,
	/// except that it will	return <see cref="MemberInfo.Name"/> rather <c>null</c>.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the namepace of the type; will return <c>null</c> if the type does
	/// not have a namespace.
	/// </summary>
	string? Namespace { get; }

	/// <summary>
	/// Gets the simple type name. This maps to <see cref="Type"/>.<see cref="MemberInfo.Name"/>.
	/// </summary>
	string SimpleName { get; }

	/// <summary>
	/// Gets the number of dimensions in an array.
	/// </summary>
	/// <exception cref="ArgumentException">Thrown if the type is not an array.</exception>
	int GetArrayRank();

	/// <summary>
	/// Gets all the custom attributes for the given type.
	/// </summary>
	/// <param name="assemblyQualifiedAttributeTypeName">The type of the attribute, in assembly qualified form</param>
	/// <returns>The matching attributes that decorate the type</returns>
	IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName);

	/// <summary>
	/// Returns the type of the object encompassed or referred to by the current array, pointer or reference type.
	/// </summary>
	/// <returns>The typeof the object encompassed or referred to by the current array, pointer, or reference type, or
	/// null if the current System.Type is not an array or a pointer, or is not passed by reference, or represents a
	/// generic type or a type parameter in the definition of a generic type or generic method.</returns>
	_ITypeInfo? GetElementType();

	/// <summary>
	/// Gets the generic type arguments for a generic type.
	/// </summary>
	/// <returns>The list of generic types.</returns>
	_ITypeInfo[] GetGenericArguments();

	/// <summary>
	/// Returns a type that represents a generic type definition from which the current generic type can be constructed.
	/// </summary>
	/// <exception cref="InvalidOperationException">The current type is not a generic type.</exception>
	_ITypeInfo GetGenericTypeDefinition();

	/// <summary>
	/// Gets a specific method.
	/// </summary>
	/// <param name="methodName">The name of the method.</param>
	/// <param name="includePrivateMethod">Set to <c>true</c> to look for the method in both public and private.</param>
	/// <returns>The method.</returns>
	_IMethodInfo? GetMethod(string methodName, bool includePrivateMethod);

	/// <summary>
	/// Gets all the methods in this type.
	/// </summary>
	/// <param name="includePrivateMethods">Set to <c>true</c> to return all methods in the type,
	/// or <c>false</c> to return only public methods.</param>
	IReadOnlyCollection<_IMethodInfo> GetMethods(bool includePrivateMethods);
}
