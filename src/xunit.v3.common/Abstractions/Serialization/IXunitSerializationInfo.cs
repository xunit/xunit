using System;

namespace Xunit.Sdk;

/// <summary>
/// An interface that is implemented by the serialization infrastructure in xUnit.net.
/// This is passed to objects which implement <see cref="IXunitSerializable"/> so they
/// can serialize and deserialize themselves from the serialization store.
/// </summary>
public interface IXunitSerializationInfo
{
	/// <summary>
	/// Adds a value to the serialization. Supported value types include the built-in
	/// intrinsics (string, int, long, float, double, and decimal, including nullable
	/// versions of those), any class which implements <see cref="IXunitSerializable"/>),
	/// or arrays of any supported types.
	/// </summary>
	/// <param name="key">The key to store the value with</param>
	/// <param name="value">The value to be stored</param>
	/// <param name="valueType">The type of the value to be stored; optional, unless the
	/// value of <paramref name="value"/> is <c>null</c>.</param>
	/// <exception cref="ArgumentException">Thrown if the value is not a supported type.</exception>
	void AddValue(
		string key,
		object? value,
		Type? valueType);

	/// <summary>
	/// Gets a value from the serialization.
	/// </summary>
	/// <param name="key">The key of the value to retrieve</param>
	/// <returns>The value, if present; <c>null</c>, otherwise</returns>
	object? GetValue(string key);
}
