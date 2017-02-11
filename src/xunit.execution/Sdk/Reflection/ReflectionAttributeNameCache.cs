using System;
using System.Collections.Concurrent;

namespace Xunit.Sdk
{
    class ReflectionAttributeNameCache
    {
        static ConcurrentDictionary<string, Type> attributeTypeCache = new ConcurrentDictionary<string, Type>();

        internal static Type GetType(string assemblyQualifiedAttributeTypeName)
        {
            return attributeTypeCache.GetOrAdd(assemblyQualifiedAttributeTypeName, name => SerializationHelper.GetType(name));
        }
    }
}
