using System.Collections.Concurrent;
using System.Reflection;

namespace Xunit.Sdk;

/// <summary>
/// Extension methods for reflection types in .NET.
/// </summary>
public static partial class ReflectionExtensions
{
	static readonly ConcurrentDictionary<Type, bool> isFromLocalAssemblyCache = new();
	static readonly ConcurrentDictionary<Type, bool> isNullableCache = new();
	static readonly ConcurrentDictionary<Type, bool> isNullableEnumCache = new();

	/// <summary>
	/// Gets the arity (number of generic types) of the method.
	/// </summary>
	public static int GetArity(this MethodInfo method) =>
		Guard.ArgumentNotNull(method).IsGenericMethod
			? method.GetGenericArguments().Length
			: 0;

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

#if !XUNIT_AOT
		baseDisplayName += ResolveGenericDisplay(genericTypes);
#endif

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

	static string GetParameterName(
		ParameterInfo[] parameters,
		int index) =>
			index < parameters.Length
				? parameters[index].Name ?? "???"
				: "???";

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
		isNullableEnumCache.GetOrAdd(
			Guard.ArgumentNotNull(type),
			t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>) && t.GetGenericArguments()[0].IsEnum
		);

	/// <summary>
	/// Determines whether <paramref name="type"/> is a static class.
	/// </summary>
	public static bool IsStatic(this Type type) =>
		Guard.ArgumentNotNull(type).IsAbstract && type.IsSealed;

	static string ParameterToDisplayValue(
		string parameterName,
		object? parameterValue) =>
			string.Format(CultureInfo.CurrentCulture, "{0}: {1}", parameterName, ArgumentFormatter.Format(parameterValue));

	/// <summary>
	/// Gets a fully qualified type name (i.e., <see cref="Type.FullName"/>), falling back to a simple
	/// type name (i.e., <see cref="MemberInfo.Name"/>) when a fully qualified name is not available. Typically
	/// used when presenting type names to the user, or to guarantee the type name is never <see langword="null"/>.
	/// </summary>
	public static string SafeName(this Type type) =>
		Guard.ArgumentNotNull(type).FullName ?? type.Name;

	/// <summary>
	/// Convert a collection of <see cref="Type"/> objects into a comma-separated list
	/// for display purposes.
	/// </summary>
	public static string ToCommaSeparatedList(
		this IEnumerable<Type?> types,
		string nullDisplay = "(null)") =>
			string.Join(", ", types.Select(type => "'" + (type?.SafeName() ?? nullDisplay) + "'"));

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

	/// <summary>
	/// Attempts to strip <see cref="Nullable{T}"/> from a type value and just return T.
	/// For non-nullable types, will return the type that was passed in.
	/// </summary>
	public static Type UnwrapNullable(this Type type) =>
		Guard.ArgumentNotNull(type).IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
			? type.GetGenericArguments()[0]
			: type;
}
