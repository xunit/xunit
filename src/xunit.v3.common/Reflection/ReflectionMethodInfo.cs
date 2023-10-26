using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Reflection-based implementation of <see cref="_IReflectionMethodInfo"/>.
/// </summary>
public class ReflectionMethodInfo : _IReflectionMethodInfo
{
	static readonly IEqualityComparer TypeComparer = new GenericTypeComparer();

	IReadOnlyCollection<_IParameterInfo>? cachedParameters;
	readonly Lazy<_ITypeInfo> returnType;
	readonly Lazy<_ITypeInfo> type;

	/// <summary>
	/// Initializes a new instance of the <see cref="ReflectionMethodInfo"/> class.
	/// </summary>
	/// <param name="method">The method to be wrapped.</param>
	public ReflectionMethodInfo(MethodInfo method)
	{
		MethodInfo = Guard.ArgumentNotNull(method);

		returnType = new(() => Reflector.Wrap(MethodInfo.ReturnType));
		type = new(() => Reflector.Wrap(MethodInfo.ReflectedType!));
	}

	/// <inheritdoc/>
	public bool IsAbstract => MethodInfo.IsAbstract;

	/// <inheritdoc/>
	public bool IsGenericMethodDefinition => MethodInfo.IsGenericMethodDefinition;

	/// <inheritdoc/>
	public bool IsPublic => MethodInfo.IsPublic;

	/// <inheritdoc/>
	public bool IsStatic => MethodInfo.IsStatic;

	/// <inheritdoc/>
	public MethodInfo MethodInfo { get; }

	/// <inheritdoc/>
	public string Name => MethodInfo.Name;

	/// <inheritdoc/>
	public _ITypeInfo ReturnType => returnType.Value;

	/// <inheritdoc/>
	public _ITypeInfo Type => type.Value;

	/// <inheritdoc/>
	public IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
	{
		Guard.ArgumentNotNull(assemblyQualifiedAttributeTypeName);

		return GetCustomAttributes(MethodInfo, assemblyQualifiedAttributeTypeName).CastOrToList();
	}

	static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		MethodInfo method,
		string assemblyQualifiedAttributeTypeName)
	{
		Guard.ArgumentNotNull(method);
		Guard.ArgumentNotNull(assemblyQualifiedAttributeTypeName);

		var attributeType = ReflectionAttributeNameCache.GetType(assemblyQualifiedAttributeTypeName);

		Guard.ArgumentNotNull(() => string.Format(CultureInfo.CurrentCulture, "Could not load type: '{0}'", assemblyQualifiedAttributeTypeName), attributeType, nameof(assemblyQualifiedAttributeTypeName));

		return GetCustomAttributes(method, attributeType, ReflectionAttributeInfo.GetAttributeUsage(attributeType));
	}

	static IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(
		MethodInfo method,
		Type attributeType,
		AttributeUsageAttribute attributeUsage)
	{
		List<ReflectionAttributeInfo>? list = null;

		foreach (var attr in method.CustomAttributes)
		{
			if (attributeType.IsAssignableFrom(attr.AttributeType))
			{
				if (list is null)
					list = new List<ReflectionAttributeInfo>();

				list.Add(new ReflectionAttributeInfo(attr));
			}
		}

		if (list is not null)
			list.Sort((left, right) => string.Compare(left.AttributeData.AttributeType.Name, right.AttributeData.AttributeType.Name, StringComparison.Ordinal));

		var results = list ?? Enumerable.Empty<_IAttributeInfo>();

		if (attributeUsage.Inherited && (attributeUsage.AllowMultiple || list is null))
		{
			// Need to find the parent method, which may not necessarily be on the parent type
			var baseMethod = GetParent(method);
			if (baseMethod is not null)
				results = results.Concat(GetCustomAttributes(baseMethod, attributeType, attributeUsage));
		}

		return results.CastOrToReadOnlyCollection();
	}

	/// <inheritdoc/>
	public IReadOnlyCollection<_ITypeInfo> GetGenericArguments() =>
		MethodInfo.GetGenericArguments().Select(t => Reflector.Wrap(t)).ToArray();

	static MethodInfo? GetParent(MethodInfo method)
	{
		if (!method.IsVirtual)
			return null;

		var methodParameters = method.GetParameters();
		var methodGenericArgCount = method.GetGenericArguments().Length;

		var currentType = method.DeclaringType;

		while (currentType != typeof(object) && currentType is not null)
		{
			currentType = currentType.BaseType;
			if (currentType is null)
				return null;

			foreach (var m in currentType.GetMatchingMethods(method))
			{
				if (m.Name == method.Name &&
					m.GetGenericArguments().Length == methodGenericArgCount &&
					ParametersHaveSameTypes(methodParameters, m.GetParameters()))
					return m;
			}
		}

		return null;
	}

	static bool ParametersHaveSameTypes(
		ParameterInfo[] left,
		ParameterInfo[] right)
	{
		if (left.Length != right.Length)
			return false;

		for (var i = 0; i < left.Length; i++)
			if (!TypeComparer.Equals(left[i].ParameterType, right[i].ParameterType))
				return false;

		return true;
	}

	/// <inheritdoc/>
	public _IMethodInfo MakeGenericMethod(params _ITypeInfo[] typeArguments)
	{
		Guard.ArgumentNotNull(typeArguments);

		var unwrapedTypeArguments = typeArguments.Select(t => ((_IReflectionTypeInfo)t).Type).ToArray();

		return Reflector.Wrap(MethodInfo.MakeGenericMethod(unwrapedTypeArguments));
	}

	/// <inheritdoc/>
	public override string? ToString() => MethodInfo.ToString();

	/// <inheritdoc/>
	public IReadOnlyCollection<_IParameterInfo> GetParameters()
	{
		if (cachedParameters is null)
		{
			var parameters = MethodInfo.GetParameters();
			var parameterInfos = new _IParameterInfo[parameters.Length];

			for (var i = 0; i < parameterInfos.Length; i++)
				parameterInfos[i] = Reflector.Wrap(parameters[i]);

			cachedParameters = parameterInfos;
		}

		return cachedParameters;
	}

	sealed class GenericTypeComparer : IEqualityComparer
	{
		bool IEqualityComparer.Equals(object? x, object? y)
		{
			if (x is null && y is null)
				return true;
			if (x is null || y is null)
				return false;

			var typeX = (Type)x;
			var typeY = (Type)y;

			if (typeX.IsGenericParameter && typeY.IsGenericParameter)
				return typeX.GenericParameterPosition == typeY.GenericParameterPosition;

			return typeX == typeY;
		}

		int IEqualityComparer.GetHashCode(object obj) =>
			Guard.ArgumentNotNull(obj).GetHashCode();
	}
}
