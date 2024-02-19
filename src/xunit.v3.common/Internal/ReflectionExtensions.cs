using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Extension methods for xUnit.net's reflection abstractions.
/// </summary>
public static class ReflectionExtensions
{
	static readonly ConcurrentDictionary<Type, bool> isFromLocalAssemblyCache = new();
	static readonly ConcurrentDictionary<Type, bool> isNullableCache = new();
	static readonly ConcurrentDictionary<Type, bool> isNullableEnumCache = new();

	/// <summary>
	/// Enumerates the type and all its base types.
	/// </summary>
	/// <param name="type">The type to start enumerating from.</param>
	/// <returns>A sequence of types.</returns>
	static IEnumerable<_ITypeInfo> EnumerateTypeHierarchy(this _ITypeInfo type)
	{
		for (var current = type; current is not null; current = current.BaseType)
			yield return current;
	}

	/// <summary>
	/// Determine if two <see cref="_ITypeInfo"/> objects point to the same underlying type.
	/// </summary>
	public static bool Equal(
		this _ITypeInfo? objA,
		_ITypeInfo? objB)
	{
		if (objA is null)
			return objB is null;
		if (objB is null)
			return false;

		return objA.Name == objB.Name && objA.Assembly.Name == objB.Assembly.Name;
	}

	/// <summary>
	/// Determines if an <see cref="_ITypeInfo"/> object and a <see cref="Type"/> object point to the
	/// same underlying type.
	/// </summary>
	public static bool Equal(
		this _ITypeInfo? objA,
		Type? objB)
	{
		if (objA is null)
			return objB is null;
		if (objB is null)
			return false;

		return objA.Name == objB.FullName && (objA.Assembly.Name == objB.Assembly.FullName || objA.Assembly.Name == objB.Assembly.GetName().Name);
	}

	/// <summary>
	/// Finds instance of an attribute from a collection of <see cref="_IAttributeInfo"/>.
	/// </summary>
	/// <param name="attributes">The attributes to search</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the type</returns>
	public static IReadOnlyCollection<_IAttributeInfo> FindCustomAttributes(
		this IEnumerable<_IAttributeInfo> attributes,
		_ITypeInfo attributeType)
	{
		Guard.ArgumentNotNull(attributes);
		Guard.ArgumentNotNull(attributeType);

		List<_IAttributeInfo>? result = null;

		foreach (var attr in attributes)
		{
			if (attributeType.IsAssignableFrom(attr.AttributeType))
			{
				result ??= new List<_IAttributeInfo>();
				result.Add(attr);
			}
			else if (attributeType.IsGenericTypeDefinition
				&& attr.AttributeType.IsConstructedGenericType
				&& attr.AttributeType.GetGenericTypeDefinition().Equal(attributeType))
			{
				result ??= new List<_IAttributeInfo>();
				result.Add(attr);
			}
		}

		result?.Sort((left, right) => string.Compare(left.AttributeType.Name, right.AttributeType.Name, StringComparison.Ordinal));

		return result ?? (IReadOnlyCollection<_IAttributeInfo>)Array.Empty<_IAttributeInfo>();
	}

	/// <summary>
	/// Finds instance of an attribute from a collection of <see cref="_IAttributeInfo"/>.
	/// </summary>
	/// <param name="attributes">The attributes to search</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the type</returns>
	internal static IReadOnlyCollection<_IAttributeInfo> FindCustomAttributes(
		this IEnumerable<_IAttributeInfo> attributes,
		string assemblyQualifiedTypeName) =>
			FindCustomAttributes(attributes, Reflector.FindTypeAndWrap(assemblyQualifiedTypeName));

	/// <summary>
	/// Finds instance of an attribute from a collection of <see cref="CustomAttributeData"/>.
	/// </summary>
	/// <param name="attributes">The attributes to search</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the type</returns>
	public static IReadOnlyCollection<_IAttributeInfo> FindCustomAttributes(
		this IEnumerable<CustomAttributeData> attributes,
		_ITypeInfo attributeType) =>
			FindCustomAttributes(attributes.Select(Reflector.Wrap).WhereNotNull(), attributeType);

	/// <summary>
	/// Finds instance of an attribute from a collection of <see cref="CustomAttributeData"/>.
	/// </summary>
	/// <param name="attributes">The attributes to search</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the type</returns>
	public static IReadOnlyCollection<_IAttributeInfo> FindCustomAttributes(
		this IEnumerable<CustomAttributeData> attributes,
		string assemblyQualifiedTypeName) =>
			FindCustomAttributes(attributes.Select(Reflector.Wrap).WhereNotNull(), Reflector.FindTypeAndWrap(assemblyQualifiedTypeName));

	/// <summary>
	/// Gets all the custom attributes for the assembly that are of the given attribute type.
	/// </summary>
	/// <param name="assemblyInfo">The assembly to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the assembly</returns>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		this _IAssemblyInfo assemblyInfo,
		Type attributeType)
	{
		Guard.ArgumentNotNull(assemblyInfo);
		Guard.ArgumentNotNull(attributeType);

		return assemblyInfo.GetCustomAttributes(Reflector.Wrap(attributeType));
	}

	/// <summary>
	/// Gets all the custom attributes for the assembly that are of the given attribute type.
	/// </summary>
	/// <param name="assemblyInfo">The assembly to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the assembly</returns>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		this _IAssemblyInfo assemblyInfo,
		string assemblyQualifiedTypeName)
	{
		Guard.ArgumentNotNull(assemblyInfo);
		Guard.ArgumentNotNull(assemblyQualifiedTypeName);

		return assemblyInfo.GetCustomAttributes(Reflector.FindTypeAndWrap(assemblyQualifiedTypeName));
	}

	/// <summary>
	/// Gets all the custom attributes for the attribute that are of the given attribute type.
	/// </summary>
	/// <param name="attributeInfo">The attribute to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the attribute</returns>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		this _IAttributeInfo attributeInfo,
		Type attributeType)
	{
		Guard.ArgumentNotNull(attributeInfo);
		Guard.ArgumentNotNull(attributeType);

		return attributeInfo.GetCustomAttributes(Reflector.Wrap(attributeType));
	}

	/// <summary>
	/// Gets all the custom attributes for the attribute that are of the given attribute type.
	/// </summary>
	/// <param name="attributeInfo">The attribute to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the attribute</returns>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		this _IAttributeInfo attributeInfo,
		string assemblyQualifiedTypeName)
	{
		Guard.ArgumentNotNull(attributeInfo);
		Guard.ArgumentNotNull(assemblyQualifiedTypeName);

		return attributeInfo.GetCustomAttributes(Reflector.FindTypeAndWrap(assemblyQualifiedTypeName));
	}

	/// <summary>
	/// Gets all the custom attributes for the method that are of the given attribute type.
	/// </summary>
	/// <param name="methodInfo">The method to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the method</returns>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		this _IMethodInfo methodInfo,
		Type attributeType)
	{
		Guard.ArgumentNotNull(methodInfo);
		Guard.ArgumentNotNull(attributeType);

		return methodInfo.GetCustomAttributes(Reflector.Wrap(attributeType));
	}

	/// <summary>
	/// Gets all the custom attributes for the method that are of the given attribute type.
	/// </summary>
	/// <param name="methodInfo">The method to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the method</returns>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		this _IMethodInfo methodInfo,
		string assemblyQualifiedTypeName)
	{
		Guard.ArgumentNotNull(methodInfo);
		Guard.ArgumentNotNull(assemblyQualifiedTypeName);

		return methodInfo.GetCustomAttributes(Reflector.FindTypeAndWrap(assemblyQualifiedTypeName));
	}

	/// <summary>
	/// Gets all the custom attributes for the parameter that are of the given attribute type.
	/// </summary>
	/// <param name="parameterInfo">The parameter to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the parameter</returns>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		this _IParameterInfo parameterInfo,
		Type attributeType)
	{
		Guard.ArgumentNotNull(parameterInfo);
		Guard.ArgumentNotNull(attributeType);

		return parameterInfo.GetCustomAttributes(Reflector.Wrap(attributeType));
	}

	/// <summary>
	/// Gets all the custom attributes for the parameter that are of the given attribute type.
	/// </summary>
	/// <param name="parameterInfo">The parameter to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the parameter</returns>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		this _IParameterInfo parameterInfo,
		string assemblyQualifiedTypeName)
	{
		Guard.ArgumentNotNull(parameterInfo);
		Guard.ArgumentNotNull(assemblyQualifiedTypeName);

		return parameterInfo.GetCustomAttributes(Reflector.FindTypeAndWrap(assemblyQualifiedTypeName));
	}

	/// <summary>
	/// Gets all the custom attributes for the type that are of the given attribute type.
	/// </summary>
	/// <param name="typeInfo">The type to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the type</returns>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		this _ITypeInfo typeInfo,
		Type attributeType)
	{
		Guard.ArgumentNotNull(typeInfo);
		Guard.ArgumentNotNull(attributeType);

		return typeInfo.GetCustomAttributes(Reflector.Wrap(attributeType));
	}

	/// <summary>
	/// Gets all the custom attributes for the type that are of the given attribute type.
	/// </summary>
	/// <param name="typeInfo">The type to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the type</returns>
	public static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		this _ITypeInfo typeInfo,
		string assemblyQualifiedTypeName)
	{
		Guard.ArgumentNotNull(typeInfo);
		Guard.ArgumentNotNull(assemblyQualifiedTypeName);

		return typeInfo.GetCustomAttributes(Reflector.FindTypeAndWrap(assemblyQualifiedTypeName));
	}

	/// <summary>
	/// Returns the default value for the given type. For value types, this means a 0-initialized
	/// instance of the type; for reference types, this means <c>null</c>.
	/// </summary>
	/// <param name="type">The type to get the default value of.</param>
	/// <returns>The default value for the given type.</returns>
	public static object? GetDefaultValue(this Type type)
	{
		Guard.ArgumentNotNull(type);

		if (type.IsValueType)
			return Activator.CreateInstance(type);

		return null;
	}

	/// <summary>
	/// Formulates the extended portion of the display name for a test method. For tests with no arguments, this will
	/// return just the base name; for tests with arguments, attempts to format the arguments and appends the argument
	/// list to the test name.
	/// </summary>
	/// <param name="method">The test method</param>
	/// <param name="baseDisplayName">The base part of the display name</param>
	/// <param name="arguments">The test method arguments</param>
	/// <param name="genericTypes">The test method's generic types</param>
	/// <returns>The full display name for the test method</returns>
	public static string GetDisplayNameWithArguments(
		this _IMethodInfo method,
		string baseDisplayName,
		object?[]? arguments,
		_ITypeInfo[]? genericTypes)
	{
		Guard.ArgumentNotNull(method);
		Guard.ArgumentNotNull(baseDisplayName);

		baseDisplayName += ResolveGenericDisplay(genericTypes);

		if (arguments is null)
			return baseDisplayName;

		var parameterInfos = method.GetParameters().CastOrToArray();
		var displayValues = new string[Math.Max(arguments.Length, parameterInfos.Length)];
		int idx;

		for (idx = 0; idx < arguments.Length; idx++)
			displayValues[idx] = ParameterToDisplayValue(GetParameterName(parameterInfos, idx), arguments[idx]);

		for (; idx < parameterInfos.Length; idx++)
		{
			var reflectionParameterInfo = parameterInfos[idx] as _IReflectionParameterInfo;
			var parameterName = GetParameterName(parameterInfos, idx);
			if (reflectionParameterInfo?.ParameterInfo.IsOptional ?? false)
				displayValues[idx] = ParameterToDisplayValue(parameterName, reflectionParameterInfo.ParameterInfo.DefaultValue);
			else
				displayValues[idx] = parameterName + ": ???";
		}

		return string.Format(CultureInfo.CurrentCulture, "{0}({1})", baseDisplayName, string.Join(", ", displayValues));
	}

	/// <summary/>
	internal static IReadOnlyCollection<MethodInfo> GetMatchingMethods(
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

		return methods.FirstOrDefault(method => method.IsPublic == methodInfo.IsPublic && method.IsStatic == methodInfo.IsStatic && method.Name == methodInfo.Name);
	}

	static string GetParameterName(
		_IParameterInfo[] parameters,
		int index)
	{
		if (index >= parameters.Length)
			return "???";

		return parameters[index].Name;
	}

	/// <summary>
	/// Determines if the given type implements the given interface.
	/// </summary>
	/// <param name="type">The type to check</param>
	/// <param name="interfaceType">The interface type to look for</param>
	/// <returns>Returns <c>true</c> if the type implements the interface; <c>false</c>, otherwise</returns>
	public static bool Implements(
		this Type type,
		Type interfaceType) =>
			Guard.ArgumentNotNull(type).GetInterfaces().Contains(interfaceType);

	/// <summary>
	/// Determines if the given type implements the given interface.
	/// </summary>
	/// <param name="typeInfo">The type to check</param>
	/// <param name="interfaceType">The interface type to look for</param>
	/// <returns>Returns <c>true</c> if the type implements the interface; <c>false</c>, otherwise</returns>
	public static bool Implements(
		this _ITypeInfo typeInfo,
		Type interfaceType) =>
			Guard.ArgumentNotNull(typeInfo).Interfaces.Any(i => i.Equal(interfaceType));

	/// <summary>
	/// Determines if the given type implements the given interface.
	/// </summary>
	/// <param name="typeInfo">The type to check</param>
	/// <param name="interfaceTypeInfo">The interface type to look for</param>
	/// <returns>Returns <c>true</c> if the type implements the interface; <c>false</c>, otherwise</returns>
	public static bool Implements(
		this _ITypeInfo typeInfo,
		_ITypeInfo interfaceTypeInfo) =>
			Guard.ArgumentNotNull(typeInfo).Interfaces.Any(i => i.Equal(interfaceTypeInfo));

	/// <summary>
	/// Determines whether an instance of <paramref name="otherTypeInfo"/> can be assigned to an
	/// instance of <paramref name="type"/>.
	/// </summary>
	public static bool IsAssignableFrom(
		this Type type,
		_ITypeInfo otherTypeInfo) =>
			IsAssignableFrom(Reflector.Wrap(type), otherTypeInfo);

	/// <summary>
	/// Determines whether an instance of <paramref name="otherType"/> can be assigned to an
	/// instance of <paramref name="typeInfo"/>.
	/// </summary>
	public static bool IsAssignableFrom(
		this _ITypeInfo typeInfo,
		Type otherType) =>
			IsAssignableFrom(typeInfo, Reflector.Wrap(otherType));

	/// <summary>
	/// Determines whether an instance of <paramref name="otherTypeInfo"/> can be assigned to an
	/// instance of <paramref name="typeInfo"/>.
	/// </summary>
	public static bool IsAssignableFrom(
		this _ITypeInfo typeInfo,
		_ITypeInfo otherTypeInfo)
	{
		Guard.ArgumentNotNull(typeInfo);
		Guard.ArgumentNotNull(otherTypeInfo);

		if (typeInfo.IsInterface)
			return otherTypeInfo.Interfaces.Any(i => i.Equal(typeInfo));

		var nonNullableTypeInfo = typeInfo.UnwrapNullable();

		for (var current = otherTypeInfo; current is not null; current = current.BaseType)
			if (typeInfo.Equal(current) || (nonNullableTypeInfo != typeInfo && nonNullableTypeInfo.Equal(current)))
				return true;

		return false;
	}

	static bool IsByRefLikeType(Type type)
	{
		var val = type.GetType().GetRuntimeProperty("IsByRefLike")?.GetValue(type);
		if (val is bool isByRefLike)
			return isByRefLike;

		// The type can't be a byreflike type if the property doesn't exist.
		return false;
	}

	/// <summary>
	/// Determines if the given type is from a local assembly.
	/// </summary>
	/// <param name="type">The type to verify</param>
	/// <returns>Returns <c>true</c> if the type originates in a local assembly; <c>false</c> if the type originates in the GAC.</returns>
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

	/// <summary>
	/// Determines if the given type is from a local assembly.
	/// </summary>
	/// <param name="typeInfo">The type to verify</param>
	/// <returns>Returns <c>true</c> if the type originates in a local assembly; <c>false</c> if the type originates in the GAC.</returns>
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

	/// <summary>
	/// Determines whether <paramref name="type"/> is a nullable type; that is, whether it
	/// represents <see cref="Nullable{T}"/>.
	/// </summary>
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

	/// <summary>
	/// Determines whether <paramref name="typeInfo"/> is a nullable type; that is, whether it
	/// represents <see cref="Nullable{T}"/>.
	/// </summary>
	public static bool IsNullable(this _ITypeInfo typeInfo)
	{
		Guard.ArgumentNotNull(typeInfo);

		if (!typeInfo.IsValueType)
			return true;

		return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition().Equal(typeof(Nullable<>));
	}

	/// <summary>
	/// Determines whether <paramref name="type"/> represents a nullable enum value.
	/// </summary>
	public static bool IsNullableEnum(this Type type) =>
		isNullableEnumCache.GetOrAdd(Guard.ArgumentNotNull(type), t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>) && t.GetGenericArguments()[0].IsEnum);

	/// <summary>
	/// Determines whether <paramref name="typeInfo"/> represents a nullable enum value.
	/// </summary>
	public static bool IsNullableEnum(this _ITypeInfo typeInfo) =>
		Guard.ArgumentNotNull(typeInfo).IsGenericType && typeInfo.GetGenericTypeDefinition().Equal(typeof(Nullable<>)) && typeInfo.GetGenericArguments()[0].IsEnum;

	static string ParameterToDisplayValue(
		string parameterName,
		object? parameterValue) =>
			string.Format(CultureInfo.CurrentCulture, "{0}: {1}", parameterName, ArgumentFormatter.Format(parameterValue));

	static object? PerformDefinedConversions(
		object argumentValue,
		Type parameterType)
	{
		// argumentValue is known to not be null when we're called from TryConvertObject
		var argumentValueType = argumentValue.GetType();
		var methodArguments = new[] { argumentValue };

		bool isMatchingOperator(
			MethodInfo m,
			string name) =>
				m.Name.Equals(name, StringComparison.Ordinal) &&
				m.IsSpecialName &&  // Filter out non-operator methods that might bear this reserved name
				m.IsStatic &&
				!IsByRefLikeType(m.ReturnType) &&
				m.GetParameters().Length == 1 &&
				m.GetParameters()[0].ParameterType == argumentValueType &&
				parameterType.IsAssignableFrom(m.ReturnType);

		// Implicit & explicit conversions to/from a type can be declared on either side of the relationship.
		// We need to check both possibilities.
		foreach (var conversionDeclaringType in new[] { parameterType, argumentValueType })
		{
			var runtimeMethods = conversionDeclaringType.GetRuntimeMethods().ToArray();

			var implicitMethod = runtimeMethods.FirstOrDefault(m => isMatchingOperator(m, "op_Implicit"));
			if (implicitMethod is not null)
				return implicitMethod.Invoke(null, methodArguments);

			var explicitMethod = runtimeMethods.FirstOrDefault(m => isMatchingOperator(m, "op_Explicit"));
			if (explicitMethod is not null)
				return explicitMethod.Invoke(null, methodArguments);
		}

		return null;
	}

	static string ResolveGenericDisplay(_ITypeInfo[]? genericTypes)
	{
		if (genericTypes is null || genericTypes.Length == 0)
			return string.Empty;

		var typeNames = new string[genericTypes.Length];
		for (var idx = 0; idx < genericTypes.Length; idx++)
			typeNames[idx] = ToSimpleTypeName(genericTypes[idx]);

		return string.Format(CultureInfo.CurrentCulture, "<{0}>", string.Join(", ", typeNames));
	}

	/// <summary>
	/// Resolves an individual generic type given an intended generic parameter type and the type of an object passed to that type.
	/// </summary>
	/// <param name="genericType">The generic type, e.g. T, to resolve.</param>
	/// <param name="methodParameterType">The non-generic or open generic type, e.g. T, to try to match with the type of the object passed to that type.</param>
	/// <param name="passedParameterType">The non-generic or closed generic type, e.g. string, used to resolve the method parameter.</param>
	/// <param name="resultType">The resolved type, e.g. the parameters (T, T, string, typeof(object)) -> (T, T, string, typeof(string)).</param>
	/// <returns>True if resolving was successful, else false.</returns>
	public static bool ResolveGenericParameter(
		this _ITypeInfo genericType,
		_ITypeInfo methodParameterType,
		_ITypeInfo passedParameterType,
		out _ITypeInfo? resultType)
	{
		Guard.ArgumentNotNull(methodParameterType);
		Guard.ArgumentNotNull(passedParameterType);

		if (genericType.Equal(methodParameterType))
		{
			resultType = passedParameterType;
			return true;
		}

		return genericType.ResolveMatchingElementType(methodParameterType, passedParameterType, out resultType)
			|| genericType.ResolveMatchingGenericType(methodParameterType, passedParameterType, out resultType);
	}

	/// <summary>
	/// Resolves a generic type for a test method. The test parameters (and associated parameter infos) are
	/// used to determine the best matching generic type for the test method that can be satisfied by all
	/// the generic parameters and their values.
	/// </summary>
	/// <param name="genericType">The generic type to be resolved</param>
	/// <param name="parameters">The parameter values being passed to the test method</param>
	/// <param name="parameterInfos">The parameter infos for the test method</param>
	/// <returns>The best matching generic type</returns>
	public static _ITypeInfo ResolveGenericType(
		this _ITypeInfo genericType,
		object?[] parameters,
		_IParameterInfo[] parameterInfos)
	{
		Guard.ArgumentNotNull(genericType);
		Guard.ArgumentNotNull(parameters);
		Guard.ArgumentNotNull(parameterInfos);

		for (var idx = 0; idx < parameterInfos.Length; ++idx)
		{
			var parameter = parameters[idx];
			if (parameter is not null)
			{
				var methodParameterType = parameterInfos[idx].ParameterType;
				var passedParameterType = Reflector.Wrap(parameter.GetType());

				if (ResolveGenericParameter(genericType, methodParameterType, passedParameterType, out var matchedType))
					return matchedType!;
			}
		}

		return SerializationHelper.TypeInfo_Object;
	}

	/// <summary>
	/// Resolves all the generic types for a test method. The test parameters are used to determine
	/// the best matching generic types for the test method that can be satisfied by all
	/// the generic parameters and their values.
	/// </summary>
	/// <param name="method">The test method</param>
	/// <param name="parameters">The parameter values being passed to the test method</param>
	/// <returns>The best matching generic types</returns>
	public static _ITypeInfo[] ResolveGenericTypes(
		this _IMethodInfo method,
		object?[] parameters)
	{
		Guard.ArgumentNotNull(method);
		Guard.ArgumentNotNull(parameters);

		var genericTypes = method.GetGenericArguments().ToArray();
		var resolvedTypes = new _ITypeInfo[genericTypes.Length];
		var parameterInfos = method.GetParameters().CastOrToArray();

		for (var idx = 0; idx < genericTypes.Length; ++idx)
			resolvedTypes[idx] = ResolveGenericType(genericTypes[idx], parameters, parameterInfos);

		return resolvedTypes;
	}

	/// <summary>
	/// Resolves an individual generic type given a type that has an element type, e.g. T[] or ref T.
	/// </summary>
	/// <param name="genericType">The generic type, e.g. T, to resolve.</param>
	/// <param name="methodParameterType">The non-generic or open generic type, e.g. T, to try to match with the type of the object passed to that type.</param>
	/// <param name="passedParameterType">The non-generic or closed generic type, e.g. string, used to resolve the method parameter.</param>
	/// <param name="resultType">The resolved type.</param>
	/// <returns>True if resolving was successful, else false.</returns>
	public static bool ResolveMatchingElementType(
		this _ITypeInfo genericType,
		_ITypeInfo methodParameterType,
		_ITypeInfo passedParameterType,
		out _ITypeInfo? resultType)
	{
		Guard.ArgumentNotNull(methodParameterType);
		Guard.ArgumentNotNull(passedParameterType);

		var methodElementType = methodParameterType.GetElementType();
		if (methodElementType is not null)
		{
			var passedElementType = passedParameterType;
			if (methodParameterType.IsArray && passedParameterType.IsArray)
				passedElementType = passedParameterType.GetElementType();

			if (passedElementType is not null)
				return genericType.ResolveGenericParameter(methodElementType, passedElementType, out resultType);
		}

		resultType = null;
		return false;
	}

	/// <summary>
	/// Resolves an individual generic type given the generic arguments and the type of an object passed to that type.
	/// </summary>
	/// <param name="genericType">The generic type, e.g. T, to resolve.</param>
	/// <param name="methodParameterType">The open generic type to match with the passed parameter.</param>
	/// <param name="methodParameterArguments">The generic arguments of the open generic type to match with the passed parameter.</param>
	/// <param name="passedParameterType">The non-generic or closed generic type, e.g. string, used to resolve the method parameter.</param>
	/// <param name="resultType">The resolved type.</param>
	/// <returns>True if resolving was successful, else false.</returns>
	public static bool ResolveMatchingGenericArguments(
		this _ITypeInfo genericType,
		_ITypeInfo methodParameterType,
		_ITypeInfo[] methodParameterArguments,
		_ITypeInfo passedParameterType,
		out _ITypeInfo? resultType)
	{
		Guard.ArgumentNotNull(methodParameterType);
		Guard.ArgumentNotNull(methodParameterArguments);
		Guard.ArgumentNotNull(passedParameterType);

		if (passedParameterType.IsGenericType)
		{
			var passedParameterTypeDefinition = passedParameterType.GetGenericTypeDefinition();

			if (methodParameterType.Equal(passedParameterTypeDefinition))
			{
				var passedParameterArguments = passedParameterType.GetGenericArguments();

				for (var i = 0; i < methodParameterArguments.Length; i++)
					if (genericType.ResolveGenericParameter(methodParameterArguments[i], passedParameterArguments[i], out resultType))
						return true;
			}
		}

		resultType = null;
		return false;
	}

	/// <summary>
	/// Resolves an individual generic type given a possibly nested generic type (e.g. List&lt;T&gt;, Dictionary&lt;T, List&lt;Ugt;gt;)
	/// </summary>
	/// <param name="genericType">The generic type, e.g. T, to resolve.</param>
	/// <param name="methodParameterType">The non-generic or open generic type, e.g. T, to try to match with the type of the object passed to that type.</param>
	/// <param name="passedParameterType">The non-generic or closed generic type, e.g. string, used to resolve the method parameter.</param>
	/// <param name="resultType">The resolved type.</param>
	/// <returns>True if resolving was successful, else false.</returns>
	public static bool ResolveMatchingGenericType(
		this _ITypeInfo genericType,
		_ITypeInfo methodParameterType,
		_ITypeInfo passedParameterType,
		out _ITypeInfo? resultType)
	{
		Guard.ArgumentNotNull(methodParameterType);
		Guard.ArgumentNotNull(passedParameterType);

		if (methodParameterType.IsGenericType)
		{
			var methodParameterTypeDefinition = methodParameterType.GetGenericTypeDefinition();
			var methodParameterArguments = methodParameterType.GetGenericArguments();

			var passedParameterTypeCandidates = passedParameterType
				.EnumerateTypeHierarchy()
				.Concat(passedParameterType.Interfaces);

			foreach (var passedParameterTypeCandidate in passedParameterTypeCandidates)
				if (genericType.ResolveMatchingGenericArguments(methodParameterTypeDefinition, methodParameterArguments, passedParameterTypeCandidate, out resultType))
					return true;
		}

		resultType = null;
		return false;
	}

	/// <summary>
	/// Resolves argument values for the test method, including support for optional method
	/// arguments.
	/// </summary>
	/// <param name="testMethod">The test method to resolve.</param>
	/// <param name="arguments">The user-supplied method arguments.</param>
	/// <returns>The argument values</returns>
	public static object?[] ResolveMethodArguments(
		this MethodBase testMethod,
		object?[] arguments)
	{
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(arguments);

		var parameters = testMethod.GetParameters();
		var hasParamsParameter = false;

		// Params can only be added at the end of the parameter list
		if (parameters.Length > 0)
			hasParamsParameter = parameters[parameters.Length - 1].GetCustomAttribute(typeof(ParamArrayAttribute)) is not null;

		var nonOptionalParameterCount = parameters.Count(p => !p.IsOptional);
		if (hasParamsParameter)
			nonOptionalParameterCount--;

		// We can't call a method if we provided fewer parameters than the number of non-optional parameters in the method.
		if (arguments.Length < nonOptionalParameterCount)
			return arguments;

		// We can't call a non-params method if we have provided more parameters than the total number of parameters in the method.
		if (!hasParamsParameter && arguments.Length > parameters.Length)
			return arguments;

		var newArguments = new object?[parameters.Length];
		var resolvedArgumentsCount = 0;
		if (hasParamsParameter)
		{
			var paramsParameter = parameters[parameters.Length - 1];
			var paramsElementType = paramsParameter.ParameterType.GetElementType();
			if (paramsElementType is null)
				throw new InvalidOperationException("Cannot determine params element type");

			if (arguments.Length < parameters.Length)
			{
				// Didn't include the params parameter
				var emptyParamsArray = Array.CreateInstance(paramsElementType, 0);
				newArguments[newArguments.Length - 1] = emptyParamsArray;
			}
			else if (arguments.Length == parameters.Length &&
				(arguments[arguments.Length - 1] is null ||
				(arguments[arguments.Length - 1]!.GetType().IsArray &&
				arguments[arguments.Length - 1]!.GetType().GetElementType() == paramsElementType)))
			{
				// Passing null or the same type array as the params parameter
				newArguments[newArguments.Length - 1] = arguments[arguments.Length - 1];
				resolvedArgumentsCount = 1;
			}
			else
			{
				// Parameters need adjusting into an array
				var paramsArrayLength = arguments.Length - parameters.Length + 1;
				var paramsArray = Array.CreateInstance(paramsElementType, paramsArrayLength);
				try
				{
					Array.Copy(arguments, parameters.Length - 1, paramsArray, 0, paramsArray.Length);
				}
				catch (ArrayTypeMismatchException)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The arguments for this test method did not match the parameters: {0}", ArgumentFormatter.Format(arguments)));
				}
				catch (InvalidCastException)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The arguments for this test method did not match the parameters: {0}", ArgumentFormatter.Format(arguments)));
				}

				newArguments[newArguments.Length - 1] = paramsArray;
				resolvedArgumentsCount = paramsArrayLength;
			}
		}

		// If the argument has been provided, pass the argument value
		for (var i = 0; i < arguments.Length - resolvedArgumentsCount; i++)
			newArguments[i] = TryConvertObject(arguments[i], parameters[i].ParameterType);

		// If the argument has not been provided, pass the default value
		var unresolvedParametersCount = hasParamsParameter ? parameters.Length - 1 : parameters.Length;
		for (var i = arguments.Length; i < unresolvedParametersCount; i++)
		{
			var parameter = parameters[i];
			if (parameter.HasDefaultValue)
				newArguments[i] = parameter.DefaultValue;
			else
				newArguments[i] = parameter.ParameterType.GetDefaultValue();
		}

		return newArguments;
	}

	/// <summary>
	/// Gets a fully qualified type name (i.e., <see cref="Type.FullName"/>), falling back to a simple
	/// type name (i.e., <see cref="MemberInfo.Name"/> when a fully qualified name is not available. Typically
	/// used when presenting type names to the user.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public static string SafeName(this Type type)
	{
		Guard.ArgumentNotNull(type);

		return type.FullName ?? type.Name;
	}

	/// <summary>
	/// Attempts to convert an instance of <see cref="_IMethodInfo"/> to <see cref="MethodInfo"/> when possible.
	/// Will rely on <see cref="_IReflectionMethodInfo.MethodInfo"/> when available, falling back to trying
	/// to locate the method by name and visibility if not.
	/// </summary>
	public static MethodInfo? ToRuntimeMethod(this _IMethodInfo methodInfo)
	{
		Guard.ArgumentNotNull(methodInfo);

		if (methodInfo is _IReflectionMethodInfo reflectionMethodInfo)
			return reflectionMethodInfo.MethodInfo;

		return methodInfo.Type.ToRuntimeType()?.GetMethodInfoFromIMethodInfo(methodInfo);
	}

	/// <summary>
	/// Attemptes to convert an instance of <see cref="_ITypeInfo"/> to <see cref="Type"/> when possible.
	/// Will rely on <see cref="_IReflectionTypeInfo.Type"/> when available, falling back to trying to
	/// load the type by name if not.
	/// </summary>
	public static Type? ToRuntimeType(this _ITypeInfo typeInfo)
	{
		Guard.ArgumentNotNull(typeInfo);

		if (typeInfo is _IReflectionTypeInfo reflectionTypeInfo)
			return reflectionTypeInfo.Type;

		return TypeHelper.GetType(typeInfo.Assembly.Name, typeInfo.Name);
	}

	/// <summary>
	/// Converts a type into a name string for display purposes. It attempts to make a more user friendly
	/// name than <see cref="Type.FullName"/> would give, especially when the type is generic.
	/// </summary>
	public static string ToSimpleTypeName(this _ITypeInfo type)
	{
		Guard.ArgumentNotNull(type);

		var baseTypeName = type.Name;

		var backTickIdx = baseTypeName.IndexOf('`');
		if (backTickIdx >= 0)
			baseTypeName = baseTypeName.Substring(0, backTickIdx);

		var lastIndex = baseTypeName.LastIndexOf('.');
		if (lastIndex >= 0)
			baseTypeName = baseTypeName.Substring(lastIndex + 1);

		if (!type.IsGenericType)
			return baseTypeName;

		var genericTypes = type.GetGenericArguments().ToArray();
		var simpleNames = new string[genericTypes.Length];

		for (var idx = 0; idx < genericTypes.Length; idx++)
			simpleNames[idx] = ToSimpleTypeName(genericTypes[idx]);

		return string.Format(CultureInfo.CurrentCulture, "{0}<{1}>", baseTypeName, string.Join(", ", simpleNames));
	}

	static object? TryConvertObject(
		object? argumentValue,
		Type parameterType)
	{
		if (argumentValue is null)
			return null;

		// No need to perform conversion
		if (parameterType.IsAssignableFrom(argumentValue.GetType()))
			return argumentValue;

		return PerformDefinedConversions(argumentValue, parameterType) ?? argumentValue;
	}

	/// <summary>
	/// Attempts to strip <see cref="Nullable{T}"/> from a type value and just return T.
	/// For non-nullable types, will return the type that was passed in.
	/// </summary>
	public static Type UnwrapNullable(this Type type)
	{
		Guard.ArgumentNotNull(type);

		if (!type.IsGenericType)
			return type;
		if (type.GetGenericTypeDefinition() != typeof(Nullable<>))
			return type;
		return type.GetGenericArguments()[0];
	}

	/// <summary>
	/// Attempts to strip <see cref="Nullable{T}"/> from a type value and just return T.
	/// For non-nullable types, will return the type that was passed in.
	/// </summary>
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
