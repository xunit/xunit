using System;
using System.Collections.Concurrent;
using Xunit.Internal;

namespace Xunit.Sdk
{
	class ReflectionAttributeNameCache
	{
		static readonly ConcurrentDictionary<string, Type?> attributeTypeCache = new ConcurrentDictionary<string, Type?>();

		internal static Type? GetType(string assemblyQualifiedAttributeTypeName)
		{
			Guard.ArgumentNotNull(nameof(assemblyQualifiedAttributeTypeName), assemblyQualifiedAttributeTypeName);

			return attributeTypeCache.GetOrAdd(assemblyQualifiedAttributeTypeName, name => SerializationHelper.GetType(name));
		}
	}
}
