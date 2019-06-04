using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Serializes and de-serializes objects
    /// </summary>
    static class SerializationHelper
    {
        static readonly ConcurrentDictionary<Type, string> typeToTypeNameMap = new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// De-serializes an object.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="serializedValue">The object's serialized value</param>
        /// <returns>The de-serialized object</returns>
        public static T Deserialize<T>(string serializedValue)
        {
            if (serializedValue == null)
                throw new ArgumentNullException(nameof(serializedValue));

            var pieces = serializedValue.Split(new[] { ':' }, 2);
            if (pieces.Length != 2)
                throw new ArgumentException("De-serialized string is in the incorrect format.");

            var deserializedType = GetType(pieces[0]);
            if (deserializedType == null)
                throw new ArgumentException($"Could not load type '{pieces[0]}' from serialization value '{serializedValue}'", nameof(serializedValue));

            if (!typeof(IXunitSerializable).IsAssignableFrom(deserializedType))
                throw new ArgumentException("Cannot de-serialize an object that does not implement " + typeof(IXunitSerializable).FullName, nameof(serializedValue));

            var obj = XunitSerializationInfo.Deserialize(deserializedType, pieces[1]);
            if (obj is XunitSerializationInfo.ArraySerializer arraySerializer)
                obj = arraySerializer.ArrayData;

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
                throw new ArgumentNullException(nameof(value));

            if (value is object[] array)
                value = new XunitSerializationInfo.ArraySerializer(array);

            if (!(value is IXunitSerializable serializable))
                throw new ArgumentException("Cannot serialize an object that does not implement " + typeof(IXunitSerializable).FullName, nameof(value));

            var serializationInfo = new XunitSerializationInfo(serializable);
            return $"{GetTypeNameForSerialization(value.GetType())}:{serializationInfo.ToSerializedString()}";
        }

        /// <summary>Gets whether the specified <paramref name="value"/> is serializable with <see cref="Serialize"/>.</summary>
        /// <param name="value">The object to test for serializability.</param>
        /// <returns>true if the object can be serialized; otherwise, false.</returns>
        public static bool IsSerializable(object value)
        {
            return XunitSerializationInfo.CanSerializeObject(value);
        }

        /// <summary>
        /// Converts an assembly qualified type name into a <see cref="Type"/> object.
        /// </summary>
        /// <param name="assemblyQualifiedTypeName">The assembly qualified type name.</param>
        /// <returns>The instance of the <see cref="Type"/>, if available; <c>null</c>, otherwise.</returns>
        public static Type GetType(string assemblyQualifiedTypeName)
        {
            var firstOpenSquare = assemblyQualifiedTypeName.IndexOf('[');
            if (firstOpenSquare > 0)
            {
                var backtick = assemblyQualifiedTypeName.IndexOf('`');
                if (backtick > 0 && backtick < firstOpenSquare)
                {
                    // Run the string looking for the matching closing square brace. Can't just assume the last one
                    // is the end, since the type could be trailed by array designators.
                    var depth = 1;
                    var lastOpenSquare = firstOpenSquare + 1;
                    var sawNonArrayDesignator = false;
                    for (; depth > 0 && lastOpenSquare < assemblyQualifiedTypeName.Length; ++lastOpenSquare)
                    {
                        switch (assemblyQualifiedTypeName[lastOpenSquare])
                        {
                            case '[': ++depth; break;
                            case ']': --depth; break;
                            case ',': break;
                            default: sawNonArrayDesignator = true; break;
                        }
                    }

                    if (sawNonArrayDesignator)
                    {
                        if (depth != 0)  // Malformed, because we never closed what we opened
                            return null;

                        var genericArgument = assemblyQualifiedTypeName.Substring(firstOpenSquare + 1, lastOpenSquare - firstOpenSquare - 2);  // Strip surrounding [ and ]
                        var innerTypeNames = SplitAtOuterCommas(genericArgument).Select(x => x.Substring(1, x.Length - 2));  // Strip surrounding [ and ] from each type name
                        var innerTypes = innerTypeNames.Select(s => GetType(s)).ToArray();
                        if (innerTypes.Any(t => t == null))
                            return null;

                        var genericDefinitionName = assemblyQualifiedTypeName.Substring(0, firstOpenSquare) + assemblyQualifiedTypeName.Substring(lastOpenSquare);
                        var genericDefinition = GetType(genericDefinitionName);
                        if (genericDefinition == null)
                            return null;

                        // Push array ranks so we can get down to the actual generic definition
                        var arrayRanks = new Stack<int>();
                        while (genericDefinition.IsArray)
                        {
                            arrayRanks.Push(genericDefinition.GetArrayRank());
                            genericDefinition = genericDefinition.GetElementType();
                        }

                        var closedGenericType = genericDefinition.MakeGenericType(innerTypes);
                        while (arrayRanks.Count > 0)
                        {
                            var rank = arrayRanks.Pop();
                            closedGenericType = rank > 1 ? closedGenericType.MakeArrayType(rank) : closedGenericType.MakeArrayType();
                        }

                        return closedGenericType;
                    }
                }
            }

            IList<string> parts = SplitAtOuterCommas(assemblyQualifiedTypeName, true);
            return
                parts.Count == 0 ? null :
                parts.Count == 1 ? Type.GetType(parts[0]) :
                GetType(parts[1], parts[0]);
        }

        /// <summary>
        /// Converts an assembly name + type name into a <see cref="Type"/> object.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <param name="typeName">The type name.</param>
        /// <returns>The instance of the <see cref="Type"/>, if available; <c>null</c>, otherwise.</returns>
        public static Type GetType(string assemblyName, string typeName)
        {
#if XUNIT_FRAMEWORK    // This behavior is only for v2, and only done on the remote app domain side
            if (assemblyName.EndsWith(ExecutionHelper.SubstitutionToken, StringComparison.OrdinalIgnoreCase))
                assemblyName = assemblyName.Substring(0, assemblyName.Length - ExecutionHelper.SubstitutionToken.Length + 1) + ExecutionHelper.PlatformSuffix;
#endif

#if NETFRAMEWORK
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
#else
            Assembly assembly = null;
            try
            {
                // Make sure we only use the short form
                var an = new AssemblyName(assemblyName);
                assembly = Assembly.Load(new AssemblyName { Name = an.Name, Version = an.Version });

            }
            catch { }
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
            // Use the abstract Type instead of concretes like RuntimeType
            if (typeof(Type).IsAssignableFrom(type))
                type = typeof(Type);

            return typeToTypeNameMap.GetOrAdd(type, GetTypeNameAsString);

            string GetTypeNameAsString(Type typeToMap)
            {
                if (!type.IsFromLocalAssembly())
                    throw new ArgumentException($"We cannot serialize type {type.FullName} because it lives in the GAC", nameof(type));

                var typeName = typeToMap.FullName;
                var assemblyName = typeToMap.GetAssembly().FullName.Split(',')[0];

                var arrayRanks = new Stack<int>();
                while (typeToMap.IsArray)
                {
                    arrayRanks.Push(typeToMap.GetArrayRank());
                    typeToMap = typeToMap.GetElementType();
                }

                if (typeToMap.IsGenericType() && !typeToMap.IsGenericTypeDefinition())
                {
                    var typeDefinition = typeToMap.GetGenericTypeDefinition();
                    var innerTypes = typeToMap.GetGenericArguments()
                                              .Select(t => $"[{GetTypeNameForSerialization(t)}]")
                                              .ToArray();
                    typeName = $"{typeDefinition.FullName}[{string.Join(",", innerTypes)}]";

                    while (arrayRanks.Count > 0)
                    {
                        typeName += '[';
                        for (var commas = arrayRanks.Pop() - 1; commas > 0; --commas)
                            typeName += ',';
                        typeName += ']';
                    }
                }

                if (string.Equals(assemblyName, "mscorlib", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(assemblyName, "System.Private.CoreLib", StringComparison.OrdinalIgnoreCase))
                    return typeName;

#if XUNIT_FRAMEWORK // This behavior is only for v2, and only done on the remote app domain side
                // If this is a platform specific assembly, strip off the trailing . and name and replace it with the token
                if (typeToMap.GetAssembly().GetCustomAttributes().FirstOrDefault(a => a != null && a.GetType().FullName == "Xunit.Sdk.PlatformSpecificAssemblyAttribute") != null)
                    assemblyName = assemblyName.Substring(0, assemblyName.LastIndexOf('.')) + ExecutionHelper.SubstitutionToken;
#endif

                return $"{typeName}, {assemblyName}";
            }
        }

        /// <summary>
        /// Retrieves a substring from the string, with whitespace trimmed on both ends.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="startIndex">The starting index.</param>
        /// <param name="length">The length.</param>
        /// <returns>
        /// A substring starting no earlier than startIndex and ending no later
        /// than startIndex + length.
        /// </returns>
        static string SubstringTrim(string str, int startIndex, int length)
        {
            int endIndex = startIndex + length;

            while (startIndex < endIndex && char.IsWhiteSpace(str[startIndex]))
                startIndex++;

            while (endIndex > startIndex && char.IsWhiteSpace(str[endIndex - 1]))
                endIndex--;

            return str.Substring(startIndex, endIndex - startIndex);
        }

        static IList<string> SplitAtOuterCommas(string value, bool trimWhitespace = false)
        {
            var results = new List<string>();

            var startIndex = 0;
            var endIndex = 0;
            var depth = 0;

            for (; endIndex < value.Length; ++endIndex)
            {
                switch (value[endIndex])
                {
                    case '[': ++depth; break;
                    case ']': --depth; break;
                    case ',':
                        if (depth == 0)
                        {
                            results.Add(trimWhitespace ?
                                SubstringTrim(value, startIndex, endIndex - startIndex) :
                                value.Substring(startIndex, endIndex - startIndex));
                            startIndex = endIndex + 1;
                        }
                        break;
                }
            }

            if (depth != 0 || startIndex >= endIndex)
            {
                results.Clear();
            }
            else
            {
                results.Add(trimWhitespace ?
                    SubstringTrim(value, startIndex, endIndex - startIndex) :
                    value.Substring(startIndex, endIndex - startIndex));
            }

            return results;
        }
    }
}
