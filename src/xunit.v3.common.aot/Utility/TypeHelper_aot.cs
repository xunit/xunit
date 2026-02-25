using System.ComponentModel;
using System.Reflection;

namespace Xunit.Sdk;

partial class TypeHelper
{
	/// <summary>
	/// Please use <see cref="TryConvert{T}"/> or <see cref="TryConvertNullable{T}"/> in Native AOT
	/// </summary>
	[Obsolete("Please use TryConvert<T> or TryConvertNullable<T> in Native AOT")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static object? ConvertArgument(
		object? arg,
		Type type) =>
			throw new PlatformNotSupportedException("Please use TryConvert<T> or TryConvertNullable<T> in Native AOT");

	/// <summary>
	/// Please use <see cref="TryConvert{T}"/> or <see cref="TryConvertNullable{T}"/> in Native AOT
	/// </summary>
	[Obsolete("Please use TryConvert<T> or TryConvertNullable<T> in Native AOT")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static object?[] ConvertArguments(
		object?[] args,
		Type[] types) =>
			throw new PlatformNotSupportedException("Please use TryConvert<T> or TryConvertNullable<T> in Native AOT");

	/// <summary>
	/// Runtime attribute type conversion is not supported in Native AOT
	/// </summary>
	[Obsolete("Runtime attribute type conversion is not supported in Native AOT")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static Array ConvertAttributeArgumentCollection(
		IReadOnlyCollection<CustomAttributeTypedArgument> collection,
		Type elementType) =>
			throw new PlatformNotSupportedException("Runtime attribute type conversion is not supported in Native AOT");

	/// <summary>
	/// Runtime type lookup is not supported in Native AOT
	/// </summary>
	[Obsolete("Runtime type lookup is not supported in Native AOT")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static Type? GetType(string assemblyQualifiedTypeName) =>
		throw new PlatformNotSupportedException("Runtime type lookup is not supported in Native AOT");

	/// <summary>
	/// Runtime type lookup is not supported in Native AOT
	/// </summary>
	[Obsolete("Runtime type lookup is not supported in Native AOT")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static Type? GetType(
		string assemblyName,
		string typeName) =>
			throw new PlatformNotSupportedException("Runtime type lookup is not supported in Native AOT");

	/// <summary>
	/// Getting a type name for serialization is not supported in Native AOT
	/// </summary>
	[Obsolete("Getting a type name for serialization is not supported in Native AOT")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static string GetTypeName(Type value) =>
		throw new PlatformNotSupportedException("Getting a type name for serialization is not supported in Native AOT");

	/// <summary>
	/// Runtime type lookup is not supported in Native AOT
	/// </summary>
	[Obsolete("Runtime type lookup is not supported in Native AOT")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static Type GetTypeStrict(string assemblyQualifiedTypeName) =>
			throw new PlatformNotSupportedException("Runtime type lookup is not supported in Native AOT");
}
