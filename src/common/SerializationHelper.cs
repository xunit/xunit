using System;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Serializes and de-serializes objects
    /// </summary>
    internal static class SerializationHelper
    {
        /// <summary>
        /// De-serializes an object.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="serializedValue">The object's serialized value</param>
        /// <returns>The de-serialized object</returns>
        public static T Deserialize<T>(string serializedValue)
        {
            if (serializedValue == null)
                throw new ArgumentNullException("serializedValue");

            var pieces = serializedValue.Split(new[] { ':' }, 2);
            if (pieces.Length != 2)
                throw new ArgumentException("De-serialized string is in the incorrect format.");

            var deserializedType = GetType(pieces[0]);
            if (deserializedType == null)
                throw new ArgumentException("Could not load type " + pieces[0], "serializedValue");

#if NEW_REFLECTION
            if (!typeof(IXunitSerializable).GetTypeInfo().IsAssignableFrom(deserializedType.GetTypeInfo()))
                throw new ArgumentException("Cannot de-serialize an object that does not implement " + typeof(IXunitSerializable).FullName, "T");
#else
            if (!typeof(IXunitSerializable).IsAssignableFrom(deserializedType))
                throw new ArgumentException("Cannot de-serialize an object that does not implement " + typeof(IXunitSerializable).FullName, "T");
#endif

            var obj = XunitSerializationInfo.Deserialize(deserializedType, pieces[1]);
            if (obj is XunitSerializationInfo.ArraySerializer)
                obj = ((XunitSerializationInfo.ArraySerializer)obj).ArrayData;

            return (T)obj;
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <returns>The serialized value</returns>
        public static string Serialize(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            var array = value as object[];
            if (array != null)
                value = new XunitSerializationInfo.ArraySerializer(array);

            var serializable = value as IXunitSerializable;
            if (serializable == null)
                throw new ArgumentException("Cannot serialize an object that does not implement " + typeof(IXunitSerializable).FullName, "value");

            var serializationInfo = new XunitSerializationInfo(serializable);
            return String.Format("{0}:{1}", GetTypeNameForSerialization(value.GetType()), serializationInfo.ToSerializedString());
        }

        /// <summary>
        /// Converts an assembly qualified type name into a <see cref="Type"/> object.
        /// </summary>
        /// <param name="assemblyQualifiedTypeName">The assembly qualified type name.</param>
        /// <returns>The instance of the <see cref="Type"/>, if available; <c>null</c>, otherwise.</returns>
        public static Type GetType(string assemblyQualifiedTypeName)
        {
            var parts = assemblyQualifiedTypeName.Split(new[] { ',' }).Select(x => x.Trim()).ToList();
            if (parts.Count == 0)
                return null;

            if (parts.Count == 1)
                return Type.GetType(parts[0]);

            return GetType(parts[1], parts[0]);
        }

        /// <summary>
        /// Converts an assembly name + type name into a <see cref="Type"/> object.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <param name="typeName">The type name.</param>
        /// <returns>The instance of the <see cref="Type"/>, if available; <c>null</c>, otherwise.</returns>
        public static Type GetType(string assemblyName, string typeName)
        {
            // Take a generic reference to xunit.execution.dll and swap it out for the current execution library
            if (String.Equals(assemblyName, "xunit.execution", StringComparison.OrdinalIgnoreCase))
                assemblyName = ExecutionHelper.AssemblyName;

#if WINDOWS_PHONE_APP || WINDOWS_PHONE || ASPNETCORE50
            Assembly assembly = null;
            try
            {
                // Make sure we only use the short form for WPA81
                var an = new AssemblyName(assemblyName);
                assembly = Assembly.Load(new AssemblyName { Name = an.Name });

            }
            catch { }
#else
            // Support both long name ("assembly, version=x.x.x.x, etc.") and short name ("assembly")
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == assemblyName || a.GetName().Name == assemblyName);
            if (assembly == null)
            {
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch { }
            }
#endif

            if (assembly == null)
                return null;

            return assembly.GetType(typeName);
        }

        /// <summary>
        /// Gets an assembly qualified type name for serialization, with special dispensation for types which
        /// originate in the execution assembly.
        /// </summary>
        public static string GetTypeNameForSerialization(Type type)
        {
            var pieces = type.AssemblyQualifiedName.Split(',').Select(p => p.Trim()).ToArray();
            if (pieces.Length < 2 || String.Equals(pieces[1], "mscorlib", StringComparison.OrdinalIgnoreCase))
                return pieces[0];

            if (String.Equals(pieces[1], ExecutionHelper.AssemblyName, StringComparison.OrdinalIgnoreCase))
                pieces[1] = "xunit.execution";

            return String.Format("{0}, {1}", pieces[0], pieces[1]);
        }
    }
}
