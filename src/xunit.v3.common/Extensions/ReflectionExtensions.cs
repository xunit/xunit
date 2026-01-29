using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace Xunit.Sdk;

/// <summary>
/// Extension methods for reflection types in .NET.
/// </summary>
public static class ReflectionExtensions
{
	readonly static AttributeUsageAttribute DefaultAttributeUsageAttribute = new(AttributeTargets.All);
	readonly static MethodInfo EnumerableCast = typeof(Enumerable).GetRuntimeMethods().First(m => m.Name == "Cast");
	readonly static MethodInfo EnumerableToArray = typeof(Enumerable).GetRuntimeMethods().First(m => m.Name == "ToArray");

	static readonly ConcurrentDictionary<Type, bool> isFromLocalAssemblyCache = new();
	static readonly ConcurrentDictionary<Type, bool> isNullableCache = new();
	static readonly ConcurrentDictionary<Type, bool> isNullableEnumCache = new();

	static IEnumerable<Type> EnumerateTypeHierarchy(this Type type)
	{
		for (var current = type; current is not null; current = current.BaseType)
			yield return current;
	}

	static IReadOnlyCollection<Attribute> FindCustomAttributes(
		this IList<CustomAttributeData> customAttributeDatas,
		Type attributeType,
		List<string>? warnings = null)
	{
		Guard.ArgumentNotNull(customAttributeDatas);
		Guard.ArgumentNotNull(attributeType);

		List<Attribute>? result = null;

		foreach (var customAttributeData in customAttributeDatas)
			try
			{
				if (attributeType.IsAssignableFrom(customAttributeData.AttributeType) || attributeType.IsOpenGenericOf(customAttributeData.AttributeType))
					if (ToAttribute(customAttributeData) is Attribute attribute)
					{
						result ??= [];
						result.Add(attribute);
					}
			}
			catch (Exception ex)
			{
				if (warnings is not null)
				{
					var innerEx = ex.Unwrap();

					warnings.Add(
						string.Format(
							CultureInfo.CurrentCulture,
							"Custom attribute type '{0}' threw '{1}' during construction: {2}{3}{4}",
							customAttributeData.AttributeType.SafeName(),
							innerEx.GetType().SafeName(),
							innerEx.Message ?? "(null message)",
							Environment.NewLine,
							innerEx.StackTrace
						)
					);
				}
			}

		result?.Sort((left, right) => string.Compare(left.GetType().SafeName(), right.GetType().SafeName(), StringComparison.Ordinal));

		return result ?? [];
	}

	static IReadOnlyCollection<Attribute> FindCustomAttributes(
		this IList<CustomAttributeData> customAttributeDatas,
		string assemblyQualifiedTypeName,
		List<string>? warnings = null) =>
			FindCustomAttributes(customAttributeDatas, TypeHelper.GetTypeStrict(assemblyQualifiedTypeName), warnings);

	static IReadOnlyCollection<T> FindCustomAttributes<T>(
		this IList<CustomAttributeData> customAttributeDatas,
		List<string>? warnings = null)
			where T : notnull
	{
		Guard.ArgumentNotNull(customAttributeDatas);

		var typeName = typeof(T).FullName;

		List<T>? result = null;

		foreach (var customAttributeData in customAttributeDatas.Where(cad => typeof(T).IsAssignableFrom(cad.AttributeType)))
			try
			{
				if (ToAttribute(customAttributeData) is T attribute)
				{
					result ??= [];
					result.Add(attribute);
				}
			}
			catch (Exception ex)
			{
				if (warnings is not null)
				{
					var innerEx = ex.Unwrap();

					warnings.Add(
						string.Format(
							CultureInfo.CurrentCulture,
							"Custom attribute type '{0}' threw '{1}' during construction: {2}{3}{4}",
							customAttributeData.AttributeType.SafeName(),
							innerEx.GetType().SafeName(),
							innerEx.Message ?? "(null message)",
							Environment.NewLine,
							innerEx.StackTrace
						)
					);
				}
			}

		result?.Sort((left, right) => string.Compare(left.GetType().SafeName(), right.GetType().SafeName(), StringComparison.Ordinal));

		return result ?? [];
	}

	/// <summary>
	/// Gets the arity (number of generic types) of the method.
	/// </summary>
	public static int GetArity(this MethodInfo method) =>
		Guard.ArgumentNotNull(method).IsGenericMethod
			? method.GetGenericArguments().Length
			: 0;

	static IList<CustomAttributeData> GetCustomAttributesDataWithInheritance<T>(
		T current,
		Func<T, IList<CustomAttributeData>> attributeAccessor,
		Func<T, T?> parentAccessor)
	{
		return getAttributes(current, attributeAccessor, parentAccessor, [], [], topLevel: true);

		static List<CustomAttributeData> getAttributes(
			T? current,
			Func<T, IList<CustomAttributeData>> attributeAccessor,
			Func<T, T?> parentAccessor,
			Dictionary<Type, AttributeUsageAttribute> usages,
			List<CustomAttributeData> results,
			bool topLevel)
		{
			if (current is null)
				return results;

			foreach (var attributeData in attributeAccessor(current))
			{
				if (!usages.TryGetValue(attributeData.AttributeType, out var usage))
				{
					// First time we've seen this attribute
					usage = attributeData.AttributeType.GetCustomAttribute<AttributeUsageAttribute>() ?? DefaultAttributeUsageAttribute;
					usages[attributeData.AttributeType] = usage;
				}
				else
				{
					// Not the first time we've seen this attribute
					if (!usage.AllowMultiple)
						continue;
				}

				if (!topLevel && !usage.Inherited)
					continue;

				results.Add(attributeData);
			}

			return getAttributes(parentAccessor(current), attributeAccessor, parentAccessor, usages, results, topLevel: false);
		}
	}

	static IList<CustomAttributeData> GetCustomAttributesDataWithInheritance(this MethodInfo method)
	{
		return GetCustomAttributesDataWithInheritance(method, m => m.GetCustomAttributesData(), getParent);

		static IEnumerable<MethodInfo> getMatchingMethods(
			Type type,
			MethodInfo methodInfo) =>
				from method in methodInfo.IsStatic ? type.GetRuntimeMethods() : type.GetTypeInfo().DeclaredMethods
				where method.IsPublic == methodInfo.IsPublic && method.IsStatic == methodInfo.IsStatic
				select method;

		static MethodInfo? getParent(MethodInfo method)
		{
			if (!method.IsVirtual)
				return null;

			var methodParameters = method.GetParameters();
			var methodGenericArgCount = method.GetGenericArguments().Length;

			var currentType = method.DeclaringType;

			while (currentType is not null && currentType != typeof(object))
			{
				currentType = currentType.GetTypeInfo().BaseType;
				if (currentType == null)
					return null;

				foreach (MethodInfo m in getMatchingMethods(currentType, method))
				{
					if (m.Name == method.Name &&
						m.GetGenericArguments().Length == methodGenericArgCount &&
						parametersHaveSameTypes(methodParameters, m.GetParameters()))
						return m;
				}
			}

			return null;
		}

		static bool parametersHaveSameTypes(
			ParameterInfo[] left,
			ParameterInfo[] right)
		{
			if (left.Length != right.Length)
				return false;

			for (int i = 0; i < left.Length; i++)
				if (!GenericTypeComparer.Instance.Equals(left[i].ParameterType, right[i].ParameterType))
					return false;

			return true;
		}
	}

	static IList<CustomAttributeData> GetCustomAttributesDataWithInheritance(this Type type) =>
		GetCustomAttributesDataWithInheritance(type, t => t.GetCustomAttributesData(), t => t.BaseType);

	/// <summary>
	/// Returns the default value for the given type. For value types, this means a 0-initialized
	/// instance of the type; for reference types, this means <see langword="null"/>.
	/// </summary>
	/// <param name="type">The type to get the default value of.</param>
	/// <returns>The default value for the given type.</returns>
	public static object? GetDefaultValue(this Type type) =>
		Guard.ArgumentNotNull(type).IsValueType
			? Activator.CreateInstance(type)
			: null;

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
		this MethodInfo method,
		string baseDisplayName,
		object?[]? arguments,
		Type[]? genericTypes)
	{
		Guard.ArgumentNotNull(method);
		Guard.ArgumentNotNull(baseDisplayName);

		baseDisplayName += ResolveGenericDisplay(genericTypes);

		if (arguments is null)
			return baseDisplayName;

		var parameterInfos = method.GetParameters();
		var displayValues = new string[Math.Max(arguments.Length, parameterInfos.Length)];
		int idx;

		for (idx = 0; idx < arguments.Length; idx++)
			displayValues[idx] = ParameterToDisplayValue(GetParameterName(parameterInfos, idx), arguments[idx]);

		for (; idx < parameterInfos.Length; idx++)
		{
			var parameterInfo = parameterInfos[idx];
			var parameterName = GetParameterName(parameterInfos, idx);
			displayValues[idx] =
				parameterInfo.IsOptional
					? ParameterToDisplayValue(parameterName, parameterInfo.DefaultValue)
					: parameterName + ": ???";
		}

		return string.Format(CultureInfo.CurrentCulture, "{0}({1})", baseDisplayName, string.Join(", ", displayValues));
	}

	/// <summary>
	/// Gets all the custom attributes for the assembly that are of the given attribute type.
	/// </summary>
	/// <param name="assembly">The assembly to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the assembly</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Assembly assembly,
		Type attributeType) =>
			FindCustomAttributes(Guard.ArgumentNotNull(assembly).GetCustomAttributesData(), attributeType);

	/// <summary>
	/// Gets all the custom attributes for the assembly that are of the given attribute type.
	/// </summary>
	/// <param name="assembly">The assembly to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the assembly</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Assembly assembly,
		Type attributeType,
		List<string> warnings) =>
			FindCustomAttributes(Guard.ArgumentNotNull(assembly).GetCustomAttributesData(), attributeType, warnings);

	/// <summary>
	/// Gets all the custom attributes for the assembly that are of the given attribute type.
	/// </summary>
	/// <param name="assembly">The assembly to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the assembly</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Assembly assembly,
		string assemblyQualifiedTypeName) =>
			FindCustomAttributes(Guard.ArgumentNotNull(assembly).GetCustomAttributesData(), assemblyQualifiedTypeName);

	/// <summary>
	/// Gets all the custom attributes for the assembly that are of the given attribute type.
	/// </summary>
	/// <param name="assembly">The assembly to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the assembly</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Assembly assembly,
		string assemblyQualifiedTypeName,
		List<string> warnings) =>
			FindCustomAttributes(Guard.ArgumentNotNull(assembly).GetCustomAttributesData(), assemblyQualifiedTypeName, warnings);

	/// <summary>
	/// Gets all the custom attributes for the assembly that are of the given attribute type.
	/// </summary>
	/// <param name="assembly">The assembly to get custom attributes for.</param>
	/// <returns>The matching attributes that decorate the assembly</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(this Assembly assembly)
		where T : notnull =>
			FindCustomAttributes<T>(Guard.ArgumentNotNull(assembly).GetCustomAttributesData());

	/// <summary>
	/// Gets all the custom attributes for the assembly that are of the given attribute type.
	/// </summary>
	/// <param name="assembly">The assembly to get custom attributes for.</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the assembly</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(
		this Assembly assembly,
		List<string> warnings)
			where T : notnull =>
				FindCustomAttributes<T>(Guard.ArgumentNotNull(assembly).GetCustomAttributesData(), warnings);

	/// <summary>
	/// Gets all the custom attributes for the attribute that are of the given attribute type.
	/// </summary>
	/// <param name="attribute">The attribute to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the attribute</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Attribute attribute,
		Type attributeType) =>
			FindCustomAttributes(Guard.ArgumentNotNull(attribute).GetType().GetCustomAttributesDataWithInheritance(), attributeType);

	/// <summary>
	/// Gets all the custom attributes for the attribute that are of the given attribute type.
	/// </summary>
	/// <param name="attribute">The attribute to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the attribute</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Attribute attribute,
		Type attributeType,
		List<string> warnings) =>
			FindCustomAttributes(Guard.ArgumentNotNull(attribute).GetType().GetCustomAttributesDataWithInheritance(), attributeType, warnings);

	/// <summary>
	/// Gets all the custom attributes for the attribute that are of the given attribute type.
	/// </summary>
	/// <param name="attribute">The attribute to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the attribute</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Attribute attribute,
		string assemblyQualifiedTypeName) =>
			FindCustomAttributes(Guard.ArgumentNotNull(attribute).GetType().GetCustomAttributesDataWithInheritance(), assemblyQualifiedTypeName);

	/// <summary>
	/// Gets all the custom attributes for the attribute that are of the given attribute type.
	/// </summary>
	/// <param name="attribute">The attribute to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the attribute</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Attribute attribute,
		string assemblyQualifiedTypeName,
		List<string> warnings) =>
			FindCustomAttributes(Guard.ArgumentNotNull(attribute).GetType().GetCustomAttributesDataWithInheritance(), assemblyQualifiedTypeName, warnings);

	/// <summary>
	/// Gets all the custom attributes for the attribute that are of the given attribute type.
	/// </summary>
	/// <param name="attribute">The attribute to get custom attributes for.</param>
	/// <returns>The matching attributes that decorate the attribute</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(this Attribute attribute)
		where T : notnull =>
			FindCustomAttributes<T>(Guard.ArgumentNotNull(attribute).GetType().GetCustomAttributesDataWithInheritance());

	/// <summary>
	/// Gets all the custom attributes for the attribute that are of the given attribute type.
	/// </summary>
	/// <param name="attribute">The attribute to get custom attributes for.</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the attribute</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(
		this Attribute attribute,
		List<string> warnings)
			where T : notnull =>
				FindCustomAttributes<T>(Guard.ArgumentNotNull(attribute).GetType().GetCustomAttributesDataWithInheritance(), warnings);

	/// <summary>
	/// Gets all the custom attributes for the method that are of the given attribute type.
	/// </summary>
	/// <param name="method">The method to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the method</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this MethodInfo method,
		Type attributeType) =>
			FindCustomAttributes(Guard.ArgumentNotNull(method).GetCustomAttributesDataWithInheritance(), attributeType);

	/// <summary>
	/// Gets all the custom attributes for the method that are of the given attribute type.
	/// </summary>
	/// <param name="method">The method to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the method</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this MethodInfo method,
		Type attributeType,
		List<string> warnings) =>
			FindCustomAttributes(Guard.ArgumentNotNull(method).GetCustomAttributesDataWithInheritance(), attributeType, warnings);

	/// <summary>
	/// Gets all the custom attributes for the method that are of the given attribute type.
	/// </summary>
	/// <param name="method">The method to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the method</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this MethodInfo method,
		string assemblyQualifiedTypeName) =>
			FindCustomAttributes(Guard.ArgumentNotNull(method).GetCustomAttributesDataWithInheritance(), assemblyQualifiedTypeName);

	/// <summary>
	/// Gets all the custom attributes for the method that are of the given attribute type.
	/// </summary>
	/// <param name="method">The method to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the method</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this MethodInfo method,
		string assemblyQualifiedTypeName,
		List<string> warnings) =>
			FindCustomAttributes(Guard.ArgumentNotNull(method).GetCustomAttributesDataWithInheritance(), assemblyQualifiedTypeName, warnings);

	/// <summary>
	/// Gets all the custom attributes for the method that are of the given attribute type.
	/// </summary>
	/// <param name="method">The method to get custom attributes for.</param>
	/// <returns>The matching attributes that decorate the method</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(this MethodInfo method)
		where T : notnull =>
			FindCustomAttributes<T>(Guard.ArgumentNotNull(method).GetCustomAttributesDataWithInheritance());

	/// <summary>
	/// Gets all the custom attributes for the method that are of the given attribute type.
	/// </summary>
	/// <param name="method">The method to get custom attributes for.</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the method</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(
		this MethodInfo method,
		List<string> warnings)
			where T : notnull =>
				FindCustomAttributes<T>(Guard.ArgumentNotNull(method).GetCustomAttributesDataWithInheritance(), warnings);

	/// <summary>
	/// Gets all the custom attributes for the parameter that are of the given attribute type.
	/// </summary>
	/// <param name="parameter">The parameter to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the parameter</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this ParameterInfo parameter,
		Type attributeType) =>
			FindCustomAttributes(Guard.ArgumentNotNull(parameter).GetCustomAttributesData(), attributeType);

	/// <summary>
	/// Gets all the custom attributes for the parameter that are of the given attribute type.
	/// </summary>
	/// <param name="parameter">The parameter to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the parameter</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this ParameterInfo parameter,
		Type attributeType,
		List<string> warnings) =>
			FindCustomAttributes(Guard.ArgumentNotNull(parameter).GetCustomAttributesData(), attributeType, warnings);

	/// <summary>
	/// Gets all the custom attributes for the parameter that are of the given attribute type.
	/// </summary>
	/// <param name="parameter">The parameter to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the parameter</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this ParameterInfo parameter,
		string assemblyQualifiedTypeName) =>
			FindCustomAttributes(Guard.ArgumentNotNull(parameter).GetCustomAttributesData(), assemblyQualifiedTypeName);

	/// <summary>
	/// Gets all the custom attributes for the parameter that are of the given attribute type.
	/// </summary>
	/// <param name="parameter">The parameter to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the parameter</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this ParameterInfo parameter,
		string assemblyQualifiedTypeName,
		List<string> warnings) =>
			FindCustomAttributes(Guard.ArgumentNotNull(parameter).GetCustomAttributesData(), assemblyQualifiedTypeName, warnings);

	/// <summary>
	/// Gets all the custom attributes for the parameter that are of the given attribute type.
	/// </summary>
	/// <param name="parameter">The parameter to get custom attributes for.</param>
	/// <returns>The matching attributes that decorate the parameter</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(this ParameterInfo parameter)
		where T : notnull =>
			FindCustomAttributes<T>(Guard.ArgumentNotNull(parameter).GetCustomAttributesData());

	/// <summary>
	/// Gets all the custom attributes for the parameter that are of the given attribute type.
	/// </summary>
	/// <param name="parameter">The parameter to get custom attributes for.</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the parameter</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(
		this ParameterInfo parameter,
		List<string> warnings)
			where T : notnull =>
				FindCustomAttributes<T>(Guard.ArgumentNotNull(parameter).GetCustomAttributesData(), warnings);

	/// <summary>
	/// Gets all the custom attributes for the type that are of the given attribute type.
	/// </summary>
	/// <param name="type">The type to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the type</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Type type,
		Type attributeType) =>
			FindCustomAttributes(Guard.ArgumentNotNull(type).GetCustomAttributesDataWithInheritance(), attributeType);

	/// <summary>
	/// Gets all the custom attributes for the type that are of the given attribute type.
	/// </summary>
	/// <param name="type">The type to get custom attributes for.</param>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the type</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Type type,
		Type attributeType,
		List<string> warnings) =>
			FindCustomAttributes(Guard.ArgumentNotNull(type).GetCustomAttributesDataWithInheritance(), attributeType, warnings);

	/// <summary>
	/// Gets all the custom attributes for the type that are of the given attribute type.
	/// </summary>
	/// <param name="type">The type to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the type</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Type type,
		string assemblyQualifiedTypeName) =>
			FindCustomAttributes(Guard.ArgumentNotNull(type).GetCustomAttributesDataWithInheritance(), assemblyQualifiedTypeName);

	/// <summary>
	/// Gets all the custom attributes for the type that are of the given attribute type.
	/// </summary>
	/// <param name="type">The type to get custom attributes for.</param>
	/// <param name="assemblyQualifiedTypeName">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the type</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Type type,
		string assemblyQualifiedTypeName,
		List<string> warnings) =>
			FindCustomAttributes(Guard.ArgumentNotNull(type).GetCustomAttributesDataWithInheritance(), assemblyQualifiedTypeName, warnings);

	/// <summary>
	/// Gets all the custom attributes for the type that are of the given attribute type.
	/// </summary>
	/// <param name="type">The type to get custom attributes for.</param>
	/// <returns>The matching attributes that decorate the type</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(this Type type)
		where T : notnull =>
			FindCustomAttributes<T>(Guard.ArgumentNotNull(type).GetCustomAttributesDataWithInheritance());

	/// <summary>
	/// Gets all the custom attributes for the type that are of the given attribute type.
	/// </summary>
	/// <param name="type">The type to get custom attributes for.</param>
	/// <param name="warnings">A collection to fill with warning messages about attribute construction failure</param>
	/// <returns>The matching attributes that decorate the type</returns>
	/// <remarks>
	/// This method safely skips attributes that throw in their constructor.
	/// </remarks>
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(
		this Type type,
		List<string> warnings)
			where T : notnull =>
				FindCustomAttributes<T>(Guard.ArgumentNotNull(type).GetCustomAttributesDataWithInheritance(), warnings);

	static string GetParameterName(
		ParameterInfo[] parameters,
		int index) =>
			index < parameters.Length
				? parameters[index].Name ?? "???"
				: "???";

	/// <summary>
	/// Determines if the given type implements the given interface.
	/// </summary>
	/// <param name="type">The type to check</param>
	/// <param name="interfaceType">The interface type to look for</param>
	/// <returns>Returns <see langword="true"/> if the type implements the interface; <see langword="false"/>, otherwise</returns>
	public static bool Implements(
		this Type type,
		Type interfaceType) =>
			Guard.ArgumentNotNull(type).GetInterfaces().Contains(interfaceType);

	static bool IsByRefLikeType(Type type)
	{
		var val = type.GetType().GetRuntimeProperty("IsByRefLike")?.GetValue(type);
		if (val is bool isByRefLike)
			return isByRefLike;

		// The type can't be a byreflike type if the property doesn't exist.
		return false;
	}

	static bool IsOpenGenericOf(
		this Type openGenericType,
		Type concreteType) =>
			openGenericType.IsGenericTypeDefinition &&
			concreteType.IsConstructedGenericType &&
			concreteType.GetGenericTypeDefinition() == openGenericType;

	/// <summary>
	/// Determines if the given type is from a local assembly.
	/// </summary>
	/// <param name="type">The type to verify</param>
	/// <returns>Returns <see langword="true"/> if the type originates in a local assembly; <see langword="false"/> if the type originates in the GAC.</returns>
	public static bool IsFromLocalAssembly(this Type type) =>
		isFromLocalAssemblyCache.GetOrAdd(
			Guard.ArgumentNotNull(type),
			t =>
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
			}
		);

	/// <summary>
	/// Determines whether <paramref name="type"/> is a nullable type; that is, whether it
	/// is a reference type or it represents <see cref="Nullable{T}"/> for a value type.
	/// </summary>
	public static bool IsNullable(this Type type) =>
		isNullableCache.GetOrAdd(
			Guard.ArgumentNotNull(type),
			t => !t.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
		);

	/// <summary>
	/// Determines whether <paramref name="type"/> represents a nullable enum value.
	/// </summary>
	public static bool IsNullableEnum(this Type type) =>
		isNullableEnumCache.GetOrAdd(Guard.ArgumentNotNull(type), t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>) && t.GetGenericArguments()[0].IsEnum);

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

	// Gets a display name for a generic method, given the resolved generic types
	static string ResolveGenericDisplay(Type[]? genericTypes)
	{
		if (genericTypes is null || genericTypes.Length == 0)
			return string.Empty;

		var typeNames = new string[genericTypes.Length];
		for (var idx = 0; idx < genericTypes.Length; idx++)
			typeNames[idx] = ToDisplayName(genericTypes[idx]);

		return string.Format(CultureInfo.CurrentCulture, "<{0}>", string.Join(", ", typeNames));
	}

	// Resolves an individual generic type given an intended generic parameter type and the type of an object
	// passed to that type. If the parameter type and the generic type are the same, then that is the resolved
	// type value; otherwise, it attempts to resolve the type by matching the argument type to the parameter type.
	static bool ResolveGenericParameter(
		this Type genericType,
		Type parameterType,
		Type argumentType,
		[NotNullWhen(true)]
		out Type? result)
	{
		if (genericType == parameterType)
		{
			result = argumentType;
			return true;
		}

		return ResolveMatchingElementType(genericType, parameterType, argumentType, out result)
			|| ResolveMatchingGenericType(genericType, parameterType, argumentType, out result);
	}

	// Resolves a generic type for a method. The arguments and parameter information are used to determine
	// the best matching generic type for the test method that can be satisfied by all the parameters and
	// their argument values. This is done by walking parameter by parameter and seeing whether that
	// parameter can help resolve the type, given the matching argument.
	static Type ResolveGenericType(
		this Type genericType,
		ParameterInfo[] parameters,
		object?[] arguments)
	{
		for (var idx = 0; idx < parameters.Length; ++idx)
		{
			var parameterType = parameters[idx].ParameterType;
			var argumentType = arguments[idx]?.GetType();
			if (argumentType is not null && ResolveGenericParameter(genericType, parameterType, argumentType, out var matchedType))
				return matchedType;
		}

		return typeof(object);
	}

	/// <summary>
	/// Resolves all the generic types for a test method. The arguments are used to determine
	/// the best matching generic types for the method that can be satisfied by all the generic
	/// parameters and their argument values.
	/// </summary>
	/// <param name="method">The method</param>
	/// <param name="arguments">The argument values being passed to the method</param>
	/// <returns>The best matching generic types</returns>
	public static Type[] ResolveGenericTypes(
		this MethodInfo method,
		object?[] arguments)
	{
		Guard.ArgumentNotNull(method);
		Guard.ArgumentNotNull(arguments);

		var genericTypes = method.GetGenericArguments();
		var resolvedTypes = new Type[genericTypes.Length];
		var parameterInfos = method.GetParameters();

		for (var idx = 0; idx < genericTypes.Length; ++idx)
			resolvedTypes[idx] = ResolveGenericType(genericTypes[idx], parameterInfos, arguments);

		return resolvedTypes;
	}

	// Resolves an individual generic type given a type that has an element type, e.g. T[] or ref T.
	static bool ResolveMatchingElementType(
		this Type genericType,
		Type parameterType,
		Type argumentType,
		[NotNullWhen(true)]
		out Type? result)
	{
		var parameterElementType = parameterType.GetElementType();
		if (parameterElementType is not null)
		{
			var argumentElementType = argumentType;
			if (parameterType.IsArray && argumentType.IsArray)
				argumentElementType = argumentType.GetElementType();
			if (argumentElementType is not null)
				return ResolveGenericParameter(genericType, parameterElementType, argumentElementType, out result);
		}

		result = null;
		return false;
	}

	// Resolves an individual generic type given the generic arguments and the type of an object passed to that type.
	static bool ResolveMatchingGenericArguments(
		this Type genericType,
		Type parameterType,
		Type[] parameterGenericArgumentTypes,
		Type argumentType,
		[NotNullWhen(true)]
		out Type? result)
	{
		if (argumentType.IsGenericType)
		{
			var argumentTypeDefinition = argumentType.GetGenericTypeDefinition();

			if (parameterType == argumentTypeDefinition)
			{
				var argumentGenericArgumentTypes = argumentType.GetGenericArguments();

				for (var i = 0; i < parameterGenericArgumentTypes.Length; i++)
					if (ResolveGenericParameter(genericType, parameterGenericArgumentTypes[i], argumentGenericArgumentTypes[i], out result))
						return true;
			}
		}

		result = null;
		return false;
	}

	// Resolves an individual generic type given a possibly nested generic type
	// (e.g. List&lt;T&gt;, Dictionary&lt;T, List&lt;U&gt;&gt;)
	static bool ResolveMatchingGenericType(
		this Type genericType,
		Type parameterType,
		Type argumentType,
		[NotNullWhen(true)]
		out Type? result)
	{
		if (parameterType.IsGenericType)
		{
			var parameterTypeDefinition = parameterType.GetGenericTypeDefinition();
			var parameterGenericArgumentTypes = parameterType.GetGenericArguments();
			var argumentTypeCandidates =
				argumentType
					.EnumerateTypeHierarchy()
					.Concat(argumentType.GetInterfaces());

			foreach (var argumentTypeCandidate in argumentTypeCandidates)
				if (ResolveMatchingGenericArguments(genericType, parameterTypeDefinition, parameterGenericArgumentTypes, argumentTypeCandidate, out result))
					return true;
		}

		result = null;
		return false;
	}

	/// <summary>
	/// Resolves argument values for the test method, ensuring they are the correct type,
	/// including support for optional method arguments.
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
			hasParamsParameter = parameters[parameters.Length - 1].GetCustomAttribute<ParamArrayAttribute>() is not null;

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
			var paramsElementType =
				paramsParameter.ParameterType.GetElementType()
					?? throw new InvalidOperationException("Cannot determine params element type");

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
			newArguments[i] =
				parameter.HasDefaultValue
					? parameter.DefaultValue
					: parameter.ParameterType.GetDefaultValue();
		}

		return newArguments;
	}

	/// <summary>
	/// Gets a fully qualified type name (i.e., <see cref="Type.FullName"/>), falling back to a simple
	/// type name (i.e., <see cref="MemberInfo.Name"/>) when a fully qualified name is not available. Typically
	/// used when presenting type names to the user, or to guarantee the type name is never <see langword="null"/>.
	/// </summary>
	public static string SafeName(this Type type)
	{
		Guard.ArgumentNotNull(type);

		return type.FullName ?? type.Name;
	}

	static object? ToArgument(
		object? arg,
		Type type)
	{
		if (arg is not null && !type.IsAssignableFrom(arg.GetType()))
			try
			{
				if (!type.IsArray)
					return Convert.ChangeType(arg, type, CultureInfo.CurrentCulture);

				var elementType = type.GetElementType()!;
				var collectionValues = arg switch
				{
					IReadOnlyCollection<CustomAttributeTypedArgument> typedArguments => typedArguments.Select(ToTypedArgument),
					IEnumerable<object> enumerable => enumerable,
					_ => throw new InvalidOperationException("Unknown data type"),
				};
				var castMethod = EnumerableCast.MakeGenericMethod(elementType);
				var toArrayMethod = EnumerableToArray.MakeGenericMethod(elementType);

				return toArrayMethod.Invoke(null, [castMethod.Invoke(null, [collectionValues])]);
			}
			catch { }

		return arg;
	}

	static object?[] ToArguments(
		object?[]? args,
		Type[]? types)
	{
		args ??= [];
		types ??= [];

		if (args.Length == types.Length)
			for (var idx = 0; idx < args.Length; idx++)
				args[idx] = ToArgument(args[idx], types[idx]);

		return args;
	}


	static object? ToAttribute(CustomAttributeData attributeData)
	{
		var ctorArgs = ToTypedArguments(attributeData.ConstructorArguments).ToArray();
		Type[] ctorArgTypes = [];

		if (ctorArgs.Length > 0)
		{
			ctorArgTypes = new Type[attributeData.ConstructorArguments.Count];
			for (int i = 0; i < ctorArgTypes.Length; i++)
				ctorArgTypes[i] = attributeData.ConstructorArguments[i].ArgumentType;
		}

		var attribute = attributeData.Constructor.Invoke(ToArguments(ctorArgs, ctorArgTypes));
		var attributeType = attribute.GetType();

		for (int i = 0; i < attributeData.NamedArguments.Count; i++)
		{
			var namedArg = attributeData.NamedArguments[i];
			var typedValue = ToTypedArgument(namedArg.TypedValue);
			var memberName = namedArg.MemberName;

			var propInfo = attributeType.GetRuntimeProperty(memberName);
			if (propInfo is not null)
				try
				{
					propInfo.SetValue(attribute, typedValue);
				}
				catch
				{
					return null;
				}
			else
			{
				var fieldInfo = attributeType.GetRuntimeField(memberName);
				if (fieldInfo is null)
					return null;

				try
				{
					fieldInfo.SetValue(attribute, typedValue);
				}
				catch
				{
					return null;
				}
			}
		}

		return attribute;
	}

	/// <summary>
	/// Convert a collection of <see cref="Type"/> objects into a comma-separated list
	/// for display purposes.
	/// </summary>
	public static string ToCommaSeparatedList(
		this IEnumerable<Type?> types,
		string nullDisplay = "(null)") =>
			string.Join(", ", types.Select(type => "'" + (type?.SafeName() ?? nullDisplay) + "'"));

	/// <summary>
	/// Converts a type into a name string for display purposes. It attempts to make a more user friendly
	/// name than <see cref="Type.FullName"/> would give, especially when the type is generic.
	/// </summary>
	public static string ToDisplayName(this Type type)
	{
		Guard.ArgumentNotNull(type);

		var baseTypeName = type.SafeName();

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
			simpleNames[idx] = ToDisplayName(genericTypes[idx]);

		return string.Format(CultureInfo.CurrentCulture, "{0}<{1}>", baseTypeName, string.Join(", ", simpleNames));
	}

	/// <summary>
	/// Gets the simple name for a type, suitable for use with <see cref="ITestCaseMetadata.TestClassSimpleName"/>.
	/// </summary>
	public static string ToSimpleName(this Type type)
	{
		Guard.ArgumentNotNull(type);

		return
			type.FullName is null
				? type.Name
				: type.Namespace is null
					? type.FullName
					: type.FullName.StartsWith(type.Namespace, StringComparison.Ordinal)
						? type.FullName.Substring(type.Namespace.Length + 1)
						: type.FullName;
	}

	static object? ToTypedArgument(CustomAttributeTypedArgument arg)
	{
		if (arg.Value is not IReadOnlyCollection<CustomAttributeTypedArgument> collect)
			return arg.Value;

		var argType = arg.ArgumentType.GetElementType()!;
		Array destinationArray = Array.CreateInstance(argType, collect.Count);

		if (argType.IsEnum)
			Array.Copy(collect.Select(x => Enum.ToObject(argType, x.Value!)).ToArray(), destinationArray, collect.Count);
		else
			Array.Copy(collect.Select(x => x.Value).ToArray(), destinationArray, collect.Count);

		return destinationArray;
	}

	static IEnumerable<object?> ToTypedArguments(IEnumerable<CustomAttributeTypedArgument> arguments)
	{
		foreach (var argument in arguments)
		{
			var value = argument.Value;

			// Collections are recursively IEnumerable<CustomAttributeTypedArgument> rather than
			// being the exact matching type, so the inner values must be converted.
			if (value is IEnumerable<CustomAttributeTypedArgument> valueAsEnumerable)
				value = ToTypedArguments(valueAsEnumerable).ToArray();
			else if (value != null && value.GetType() != argument.ArgumentType && argument.ArgumentType.GetTypeInfo().IsEnum)
				value = Enum.ToObject(argument.ArgumentType, value);

			if (value != null && value.GetType() != argument.ArgumentType && argument.ArgumentType.GetTypeInfo().IsArray)
				value = ToArgument(value, argument.ArgumentType);

			yield return value;
		}
	}

	/// <summary>
	/// Converts a <see cref="Type"/> name into the correct form for VSTest managed type name for
	/// using in managed TestCase properties and by xunit.runner.visualstudio.
	/// </summary>
	/// <remarks>
	/// See <see href="https://github.com/microsoft/vstest/blob/main/docs/RFCs/0017-Managed-TestCase-Properties.md"/>
	/// </remarks>
	public static string ToVSTestTypeName(
		this Type type,
		MethodInfo? testMethod = null,
		Type? testClass = null)
	{
		Guard.ArgumentNotNull(type);

		if (type.IsGenericParameter)
		{
			if (testMethod is not null)
			{
				var methodGenericArgs = testMethod.GetGenericArguments();
				for (var i = 0; i < methodGenericArgs.Length; ++i)
					if (methodGenericArgs[i] == type)
						return "!!" + i;
			}

			if (testClass is not null)
			{
				var testClassGenericArgs = testClass.GetGenericArguments();
				for (var i = 0; i < testClassGenericArgs.Length; ++i)
					if (testClassGenericArgs[i] == type)
						return "!" + i;
			}
		}

		if (!type.IsGenericType)
			return type.SafeName();

		// We don't use .FullName here because we don't want the generic [[...]] to show up in our name.
		// So we reconstruct starting with the simple name and work backward from the declaring types
		// since there's no built-in way to get "Namespace.ParentType+ChildType`1".
		var baseTypeName = type.Name;
		var currentType = type.DeclaringType;

		while (currentType is not null)
		{
			if (currentType.FullName is not null)
			{
				baseTypeName = currentType.FullName + "+" + baseTypeName;
				break;
			}

			baseTypeName = currentType.Name + "+" + baseTypeName;
			currentType = currentType.DeclaringType;
		}

		if (currentType is null)
			baseTypeName = type.Namespace + "." + baseTypeName;

		var genericTypes =
			type
				.GenericTypeArguments
				.Select(t => ToVSTestTypeName(t, testMethod, testClass));

		return baseTypeName + "<" + string.Join(",", genericTypes) + ">";
	}

	static object? TryConvertObject(
		object? argumentValue,
		Type parameterType)
	{
		if (argumentValue is null)
			return null;

		// No need to perform conversion
		return
			parameterType.IsAssignableFrom(argumentValue.GetType())
				? argumentValue
				: PerformDefinedConversions(argumentValue, parameterType) ?? argumentValue;
	}

	/// <summary>
	/// Attempts to strip <see cref="Nullable{T}"/> from a type value and just return T.
	/// For non-nullable types, will return the type that was passed in.
	/// </summary>
	public static Type UnwrapNullable(this Type type) =>
		Guard.ArgumentNotNull(type).IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
			? type.GetGenericArguments()[0]
			: type;

	sealed class GenericTypeComparer : IEqualityComparer
	{
		public static IEqualityComparer Instance { get; } = new GenericTypeComparer();

		bool IEqualityComparer.Equals(
			object? x,
			object? y)
		{
			if (x is null)
				return y is null;
			if (y is null)
				return false;

			var typeX = (Type)x;
			var typeY = (Type)y;

			if (typeX.IsGenericParameter && typeY.IsGenericParameter)
				return typeX.GenericParameterPosition == typeY.GenericParameterPosition;

			return typeX == typeY;
		}

		int IEqualityComparer.GetHashCode(object obj)
		{
			Guard.ArgumentNotNull(nameof(obj), obj);

			return obj.GetHashCode();
		}
	}
}
