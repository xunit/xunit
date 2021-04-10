using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

/// <summary>
/// This class represents utility methods needed to supplement the
/// reflection capabilities provided by the CLR
/// </summary>
public static class ReflectionAbstractionExtensions
{
	static MethodInfo? GetMethodInfoFromIMethodInfo(this Type type, _IMethodInfo methodInfo)
	{
		var methods = methodInfo.IsStatic ? type.GetRuntimeMethods() : type.GetMethods();

		return
			methods
				.Where(method => method.IsPublic == methodInfo.IsPublic && method.IsStatic == methodInfo.IsStatic && method.Name == methodInfo.Name)
				.FirstOrDefault();
	}

	/// <summary>
	/// Gets methods in the target type that match the protection level of the supplied method.
	/// </summary>
	/// <param name="type">The type</param>
	/// <param name="methodInfo">The method</param>
	/// <returns>The methods that have the same visibility as the supplied method.</returns>
	public static IReadOnlyCollection<MethodInfo> GetMatchingMethods(this Type type, MethodInfo methodInfo)
	{
		Guard.ArgumentNotNull(nameof(type), type);
		Guard.ArgumentNotNull(nameof(methodInfo), methodInfo);

		var methods = methodInfo.IsStatic ? type.GetRuntimeMethods() : type.GetMethods();

		return methods
			.Where(method => method.IsPublic == methodInfo.IsPublic && method.IsStatic == methodInfo.IsStatic)
			.CastOrToReadOnlyCollection();
	}

	/// <summary>
	/// Gets all the custom attributes for the given assembly.
	/// </summary>
	/// <param name="assemblyInfo">The assembly</param>
	/// <param name="attributeType">The type of the attribute</param>
	/// <returns>The matching attributes that decorate the assembly</returns>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(this _IAssemblyInfo assemblyInfo, Type attributeType)
	{
		Guard.ArgumentNotNull(nameof(assemblyInfo), assemblyInfo);
		Guard.ArgumentNotNull(nameof(attributeType), attributeType);
		Guard.NotNull("Attribute type cannot be a generic type parameter", attributeType.AssemblyQualifiedName);

		return assemblyInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
	}

	/// <summary>
	/// Gets all the custom attributes for the given attribute.
	/// </summary>
	/// <param name="attributeInfo">The attribute</param>
	/// <param name="attributeType">The type of the attribute to find</param>
	/// <returns>The matching attributes that decorate the attribute</returns>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(this _IAttributeInfo attributeInfo, Type attributeType)
	{
		Guard.ArgumentNotNull(nameof(attributeInfo), attributeInfo);
		Guard.ArgumentNotNull(nameof(attributeType), attributeType);
		Guard.NotNull("Attribute type cannot be a generic type parameter", attributeType.AssemblyQualifiedName);

		return attributeInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
	}

	/// <summary>
	/// Gets all the custom attributes for the method that are of the given type.
	/// </summary>
	/// <param name="methodInfo">The method</param>
	/// <param name="attributeType">The type of the attribute</param>
	/// <returns>The matching attributes that decorate the method</returns>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(this _IMethodInfo methodInfo, Type attributeType)
	{
		Guard.ArgumentNotNull(nameof(methodInfo), methodInfo);
		Guard.ArgumentNotNull(nameof(attributeType), attributeType);
		Guard.NotNull("Attribute type cannot be a generic type parameter", attributeType.AssemblyQualifiedName);

		return methodInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
	}

	/// <summary>
	/// Gets all the custom attributes for the given type.
	/// </summary>
	/// <param name="typeInfo">The type</param>
	/// <param name="attributeType">The type of the attribute</param>
	/// <returns>The matching attributes that decorate the type</returns>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(this _ITypeInfo typeInfo, Type attributeType)
	{
		Guard.ArgumentNotNull(nameof(typeInfo), typeInfo);
		Guard.ArgumentNotNull(nameof(attributeType), attributeType);
		Guard.NotNull("Attribute type cannot be a generic type parameter", attributeType.AssemblyQualifiedName);

		return typeInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
	}

	/// <summary>
	/// Converts an <see cref="_IMethodInfo"/> into a <see cref="MethodInfo"/>, if possible (for example, this
	/// will not work when the test method is based on source code rather than binaries).
	/// </summary>
	/// <param name="methodInfo">The method to convert</param>
	/// <returns>The runtime method, if available; <c>null</c>, otherwise</returns>
	public static MethodInfo? ToRuntimeMethod(this _IMethodInfo methodInfo)
	{
		Guard.ArgumentNotNull(nameof(methodInfo), methodInfo);

		if (methodInfo is _IReflectionMethodInfo reflectionMethodInfo)
			return reflectionMethodInfo.MethodInfo;

		return methodInfo.Type.ToRuntimeType()?.GetMethodInfoFromIMethodInfo(methodInfo);
	}

	/// <summary>
	/// Converts an <see cref="_ITypeInfo"/> into a <see cref="Type"/>, if possible (for example, this
	/// will not work when the test class is based on source code rather than binaries).
	/// </summary>
	/// <param name="typeInfo">The type to convert</param>
	/// <returns>The runtime type, if available, <c>null</c>, otherwise</returns>
	public static Type? ToRuntimeType(this _ITypeInfo typeInfo)
	{
		if (typeInfo is _IReflectionTypeInfo reflectionTypeInfo)
			return reflectionTypeInfo.Type;

		return SerializationHelper.GetType(typeInfo.Assembly.Name, typeInfo.Name);
	}
}
