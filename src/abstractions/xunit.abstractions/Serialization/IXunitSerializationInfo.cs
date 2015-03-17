using System;
namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents serialization support in xUnit.net.
    /// </summary>
    public interface IXunitSerializationInfo
    {
        /// <summary>
        /// Adds a value to the serialization. Supported value types include the built-in
        /// intrinsics (string, int, long, float, double, and decimal, including nullable
        /// versions of those), any class which implements <see cref="IXunitSerializable"/>),
        /// or arrays of any supported types.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        /// <param name="type">The optional type of the value</param>
        void AddValue(string key, object value, Type type = null);

        /// <summary>
        /// Gets a value from the serialization.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="type">The type of the value</param>
        /// <returns>The value, if present; <c>null</c>, otherwise</returns>
        object GetValue(string key, Type type);

        /// <summary>
        /// Gets a value from the serialization.
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The value, if present; default(T), otherwise</returns>
        T GetValue<T>(string key);
    }
}