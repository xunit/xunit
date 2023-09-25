using System;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Extension methods for <see cref="IXunitSerializationInfo"/>.
/// </summary>
public static class XunitSerializationInfoExtensions
{
	/// <summary>
	/// Adds a value to the serialization. Supported value types include the built-in
	/// intrinsics (string, int, long, float, double, and decimal, including nullable
	/// versions of those), any class which implements <see cref="IXunitSerializable"/>),
	/// or arrays of any supported types.
	/// </summary>
	/// <param name="info"></param>
	/// <param name="key">The key to store the value with</param>
	/// <param name="value">The value to be stored</param>
	public static void AddValue<T>(
		this IXunitSerializationInfo info,
		string key,
		T value) =>
			Guard.ArgumentNotNull(info).AddValue(key, value, value?.GetType() ?? typeof(T));

	/// <summary>
	/// Adds a value to the serialization. Supported value types include the built-in
	/// intrinsics (string, int, long, float, double, and decimal, including nullable
	/// versions of those), any class which implements <see cref="IXunitSerializable"/>),
	/// or arrays of any supported types.
	/// </summary>
	/// <param name="info"></param>
	/// <param name="key">The key to store the value with</param>
	/// <param name="value">The value to be stored</param>
	/// <param name="valueType">The type of the value to be stored; optional, unless the
	/// value of <paramref name="value"/> is <c>null</c>.</param>
	/// <exception cref="ArgumentException">Thrown if the value is not a supported type.</exception>
	public static void AddValue(
		this IXunitSerializationInfo info,
		string key,
		object? value,
		Type? valueType) =>
			Guard.ArgumentNotNull(info).AddValue(key, value, Reflector.Wrap(valueType));

	/// <summary>
	/// Gets a strongly-typed value from the serialization.
	/// </summary>
	/// <typeparam name="T">The type of the value</typeparam>
	/// <param name="info"></param>
	/// <param name="key">The key of the value to retrieve</param>
	/// <returns>The value, if present; default(T), otherwise</returns>
	public static T? GetValue<T>(
		this IXunitSerializationInfo info,
		string key) =>
			(T?)Guard.ArgumentNotNull(info).GetValue(key);
}
