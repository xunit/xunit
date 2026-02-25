#pragma warning disable IDE0060 // Method contracts here must match the non-AOT version

using System.ComponentModel;
using System.Reflection;

namespace Xunit.Sdk;

partial class ReflectionExtensions
{
	static readonly Dictionary<Type, object?> defaultValues = new()
	{
		[typeof(char)] = (char)0,
		[typeof(byte)] = (byte)0,
		[typeof(sbyte)] = (sbyte)0,
		[typeof(short)] = (short)0,
		[typeof(ushort)] = (ushort)0,
		[typeof(int)] = 0,
		[typeof(uint)] = 0U,
		[typeof(long)] = 0L,
		[typeof(ulong)] = 0UL,
		[typeof(float)] = 0F,
		[typeof(double)] = 0D,
		[typeof(decimal)] = 0M,
		[typeof(bool)] = false,
		[typeof(DateTime)] = DateTime.MinValue,
		[typeof(DateTimeOffset)] = DateTimeOffset.MinValue,
		[typeof(TimeSpan)] = TimeSpan.Zero,
		[typeof(Guid)] = Guid.Empty,

		[typeof(char?)] = null,
		[typeof(byte?)] = null,
		[typeof(sbyte?)] = null,
		[typeof(short?)] = null,
		[typeof(ushort?)] = null,
		[typeof(int?)] = null,
		[typeof(uint?)] = null,
		[typeof(long?)] = null,
		[typeof(ulong?)] = null,
		[typeof(float?)] = null,
		[typeof(double?)] = null,
		[typeof(decimal?)] = null,
		[typeof(bool?)] = null,
		[typeof(DateTime?)] = null,
		[typeof(DateTimeOffset?)] = null,
		[typeof(TimeSpan?)] = null,
		[typeof(Guid?)] = null,
	};

	/// <summary>
	/// Returns the default value for the given type. For value types, this means a 0-initialized
	/// instance of the type; for reference types, this means <see langword="null"/>.
	/// </summary>
	/// <param name="type">The type to get the default value of.</param>
	/// <returns>The default value for the given type.</returns>
	/// <remarks>
	/// In Native AOT, this only supports reference types (which return <see langword="null"/>)
	/// and known value types (which return default values). The known data types are:
	/// <list type="bullet">
	/// <item><see langword="bool" /></item>
	/// <item><see langword="byte" /></item>
	/// <item><see langword="char" /></item>
	/// <item><see cref="DateTime"/></item>
	/// <item><see cref="DateTimeOffset"/></item>
	/// <item><see langword="double" /></item>
	/// <item><see langword="float" /></item>
	/// <item><see cref="Guid"/></item>
	/// <item><see langword="int" /></item>
	/// <item><see langword="long" /></item>
	/// <item><see langword="sbyte" /></item>
	/// <item><see langword="short" /></item>
	/// <item><see cref="TimeSpan"/></item>
	/// <item><see langword="uint" /></item>
	/// <item><see langword="ulong" /></item>
	/// <item><see langword="ushort" /></item>
	/// </list>
	/// </remarks>
	public static object? GetDefaultValue(this Type type)
	{
		Guard.ArgumentNotNull(type);

		if (defaultValues.TryGetValue(type, out var result))
			return result;

		if (!type.IsValueType)
			return null;

		throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Type '{0}' is not one of the known types", type.SafeName()));
	}

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Assembly assembly,
		Type attributeType) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Assembly assembly,
		Type attributeType,
		List<string> warnings) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Assembly assembly,
		string assemblyQualifiedTypeName) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Assembly assembly,
		string assemblyQualifiedTypeName,
		List<string> warnings) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(this Assembly assembly)
		where T : notnull =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(
		this Assembly assembly,
		List<string> warnings)
			where T : notnull =>
				throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Attribute attribute,
		Type attributeType) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Attribute attribute,
		Type attributeType,
		List<string> warnings) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Attribute attribute,
		string assemblyQualifiedTypeName) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Attribute attribute,
		string assemblyQualifiedTypeName,
		List<string> warnings) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(this Attribute attribute)
		where T : notnull =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(
		this Attribute attribute,
		List<string> warnings)
			where T : notnull =>
				throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this ParameterInfo parameter,
		Type attributeType) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this ParameterInfo parameter,
		Type attributeType,
		List<string> warnings) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this ParameterInfo parameter,
		string assemblyQualifiedTypeName) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this ParameterInfo parameter,
		string assemblyQualifiedTypeName,
		List<string> warnings) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(this ParameterInfo parameter)
		where T : notnull =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(
		this ParameterInfo parameter,
		List<string> warnings)
			where T : notnull =>
				throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Type type,
		Type attributeType) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Type type,
		Type attributeType,
		List<string> warnings) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Type type,
		string assemblyQualifiedTypeName) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this Type type,
		string assemblyQualifiedTypeName,
		List<string> warnings) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(this Type type)
		where T : notnull =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(
		this Type type,
		List<string> warnings)
			where T : notnull =>
				throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this MethodInfo method,
		Type attributeType) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this MethodInfo method,
		Type attributeType,
		List<string> warnings) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this MethodInfo method,
		string assemblyQualifiedTypeName) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<Attribute> GetMatchingCustomAttributes(
		this MethodInfo method,
		string assemblyQualifiedTypeName,
		List<string> warnings) =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(this MethodInfo method)
		where T : notnull =>
			throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface-based attributes are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyCollection<T> GetMatchingCustomAttributes<T>(
		this MethodInfo method,
		List<string> warnings)
			where T : notnull =>
				throw new PlatformNotSupportedException("Interface-based attributes are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Interface implementation is determined by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static bool Implements(
		this Type type,
		Type interfaceType) =>
			throw new PlatformNotSupportedException("Interface implementation is determined by the source generator in Native AOT");

	/// <summary/>
	[Obsolete("Generic test methods are not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static Type[] ResolveGenericTypes(
		this MethodInfo method,
		object?[] arguments) =>
			throw new PlatformNotSupportedException("Generic test methods are not supported in Native AOT");

	/// <summary/>
	[Obsolete("Argument resolution is performed by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static object?[] ResolveMethodArguments(
		this MethodBase testMethod,
		object?[] arguments) =>
			throw new PlatformNotSupportedException("Argument resolution is performed by the source generator in Native AOT");

	/// <summary/>
	[Obsolete("Display name composition is performed by the source generator in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static string ToDisplayName(this Type type) =>
		throw new PlatformNotSupportedException("Display name composition is performed by the source generator in Native AOT");
}
