using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class ReflectionExtensions
{
	static readonly ConcurrentDictionary<Type, bool> isFromLocalAssemblyCache = new();
	static readonly ConcurrentDictionary<Type, bool> isNullableCache = new();
	static readonly ConcurrentDictionary<Type, bool> isNullableEnumCache = new();

	/// <summary/>
	public static bool Equal(
		this _ITypeInfo? objA,
		_ITypeInfo? objB)
	{
		if (objA == null)
			return objB == null;
		if (objB == null)
			return false;

		return objA.Name == objB.Name && objA.Assembly.Name == objB.Assembly.Name;
	}

	/// <summary/>
	public static bool Equal(
		this _ITypeInfo? objA,
		Type? objB)
	{
		if (objA == null)
			return objB == null;
		if (objB == null)
			return false;

		return objA.Name == objB.FullName && (objA.Assembly.Name == objB.Assembly.FullName || objA.Assembly.Name == objB.Assembly.GetName().Name);
	}

	/// <summary/>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		this _IAssemblyInfo assemblyInfo,
		Type attributeType)
	{
		Guard.ArgumentNotNull(assemblyInfo);
		Guard.ArgumentNotNull(attributeType);
		Guard.NotNull("Attribute type cannot be a generic type parameter", attributeType.AssemblyQualifiedName);

		return assemblyInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
	}

	/// <summary/>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		this _IAttributeInfo attributeInfo,
		Type attributeType)
	{
		Guard.ArgumentNotNull(attributeInfo);
		Guard.ArgumentNotNull(attributeType);
		Guard.NotNull("Attribute type cannot be a generic type parameter", attributeType.AssemblyQualifiedName);

		return attributeInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
	}

	/// <summary/>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		this _IMethodInfo methodInfo,
		Type attributeType)
	{
		Guard.ArgumentNotNull(methodInfo);
		Guard.ArgumentNotNull(attributeType);
		Guard.NotNull("Attribute type cannot be a generic type parameter", attributeType.AssemblyQualifiedName);

		return methodInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
	}

	/// <summary/>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		this _ITypeInfo typeInfo,
		Type attributeType)
	{
		Guard.ArgumentNotNull(typeInfo);
		Guard.ArgumentNotNull(attributeType);
		Guard.NotNull("Attribute type cannot be a generic type parameter", attributeType.AssemblyQualifiedName);

		return typeInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
	}

	/// <summary/>
	public static IReadOnlyCollection<MethodInfo> GetMatchingMethods(
		this Type type,
		MethodInfo methodInfo)
	{
		Guard.ArgumentNotNull(type);
		Guard.ArgumentNotNull(methodInfo);

		var methods = methodInfo.IsStatic ? type.GetRuntimeMethods() : type.GetMethods();

		return methods
			.Where(method => method.IsPublic == methodInfo.IsPublic && method.IsStatic == methodInfo.IsStatic)
			.CastOrToReadOnlyCollection();
	}

	static MethodInfo? GetMethodInfoFromIMethodInfo(
		this Type type,
		_IMethodInfo methodInfo)
	{
		var methods = methodInfo.IsStatic ? type.GetRuntimeMethods() : type.GetMethods();

		return
			methods
				.Where(method => method.IsPublic == methodInfo.IsPublic && method.IsStatic == methodInfo.IsStatic && method.Name == methodInfo.Name)
				.FirstOrDefault();
	}

	/// <summary/>
	public static bool Implements(
		this Type type,
		Type interfaceType) =>
			type.GetInterfaces().Contains(interfaceType);

	/// <summary/>
	public static bool Implements(
		this _ITypeInfo typeInfo,
		Type interfaceType) =>
			typeInfo.Interfaces.Any(i => i.Equal(interfaceType));

	/// <summary/>
	public static bool Implements(
		this _ITypeInfo typeInfo,
		_ITypeInfo interfaceTypeInfo) =>
			typeInfo.Interfaces.Any(i => i.Equal(interfaceTypeInfo));

	/// <summary/>
	public static bool IsAssignableFrom(
		this Type type,
		_ITypeInfo otherTypeInfo) =>
			IsAssignableFrom(Reflector.Wrap(type), otherTypeInfo);

	/// <summary/>
	public static bool IsAssignableFrom(
		this _ITypeInfo typeInfo,
		Type otherType) =>
			IsAssignableFrom(typeInfo, Reflector.Wrap(otherType));

	/// <summary/>
	public static bool IsAssignableFrom(
		this _ITypeInfo typeInfo,
		_ITypeInfo otherTypeInfo)
	{
		if (typeInfo.IsInterface)
			return otherTypeInfo.Interfaces.Any(i => i.Equal(typeInfo));

		var nonNullableTypeInfo = typeInfo.UnwrapNullable();

		for (var current = otherTypeInfo; current != null; current = current.BaseType)
			if (typeInfo.Equal(current) || (nonNullableTypeInfo != typeInfo && nonNullableTypeInfo.Equal(current)))
				return true;

		return false;
	}

	/// <summary/>
	public static bool IsFromLocalAssembly(this Type type)
	{
		Guard.ArgumentNotNull(type);

		return isFromLocalAssemblyCache.GetOrAdd(type, t =>
		{
			var assemblyName = t.Assembly.GetName().Name;

			try
			{
				Assembly.Load(new AssemblyName { Name = assemblyName });
				return true;
			}
			catch
			{
				return false;
			}
		});
	}

	/// <summary/>
	public static bool IsFromLocalAssembly(this _ITypeInfo typeInfo)
	{
		Guard.ArgumentNotNull(typeInfo);

		var assemblyName = typeInfo.Assembly.Name.Split(',').First();

		try
		{
			Assembly.Load(new AssemblyName { Name = assemblyName });
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary/>
	public static bool IsNullable(this Type type)
	{
		Guard.ArgumentNotNull(type);

		return isNullableCache.GetOrAdd(type, t =>
		{
			if (!t.IsValueType)
				return true;

			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		});
	}

	/// <summary/>
	public static bool IsNullable(this _ITypeInfo typeInfo)
	{
		Guard.ArgumentNotNull(typeInfo);

		if (!typeInfo.IsValueType)
			return true;

		return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition().Equal(typeof(Nullable<>));
	}

	/// <summary/>
	public static bool IsNullableEnum(this Type type) =>
		isNullableEnumCache.GetOrAdd(type, t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>) && t.GetGenericArguments()[0].IsEnum);

	/// <summary/>
	public static bool IsNullableEnum(this _ITypeInfo typeInfo) =>
		typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition().Equal(typeof(Nullable<>)) && typeInfo.GetGenericArguments()[0].IsEnum;

	/// <summary/>
	public static string SafeName(this Type type)
	{
		Guard.ArgumentNotNull(type);

		return type.FullName ?? type.Name;
	}

	/// <summary/>
	public static MethodInfo? ToRuntimeMethod(this _IMethodInfo methodInfo)
	{
		Guard.ArgumentNotNull(methodInfo);

		if (methodInfo is _IReflectionMethodInfo reflectionMethodInfo)
			return reflectionMethodInfo.MethodInfo;

		return methodInfo.Type.ToRuntimeType()?.GetMethodInfoFromIMethodInfo(methodInfo);
	}

	/// <summary/>
	public static Type? ToRuntimeType(this _ITypeInfo typeInfo)
	{
		Guard.ArgumentNotNull(typeInfo);

		if (typeInfo is _IReflectionTypeInfo reflectionTypeInfo)
			return reflectionTypeInfo.Type;

		return TypeHelper.GetType(typeInfo.Assembly.Name, typeInfo.Name);
	}

	/// <summary/>
	public static Type UnwrapNullable(this Type type)
	{
		Guard.ArgumentNotNull(type);

		if (!type.IsGenericType)
			return type;
		if (type.GetGenericTypeDefinition() != typeof(Nullable<>))
			return type;
		return type.GetGenericArguments()[0];
	}

	/// <summary/>
	public static _ITypeInfo UnwrapNullable(this _ITypeInfo typeInfo)
	{
		Guard.ArgumentNotNull(typeInfo);

		if (!typeInfo.IsGenericType)
			return typeInfo;
		if (!Equal(typeInfo.GetGenericTypeDefinition(), typeof(Nullable<>)))
			return typeInfo;
		return typeInfo.GetGenericArguments()[0];
	}
}
