using System;
using System.Collections.Concurrent;
using Xunit.Internal;

namespace Xunit.Sdk;

static class ReflectionAttributeNameCache
{
	static readonly ConcurrentDictionary<string, Type?> attributeTypeCache = new();

	internal static Type? GetType(string assemblyQualifiedAttributeTypeName)
	{
		Guard.ArgumentNotNull(assemblyQualifiedAttributeTypeName);

		return attributeTypeCache.GetOrAdd(assemblyQualifiedAttributeTypeName, name => TypeHelper.GetType(name));
	}
}
