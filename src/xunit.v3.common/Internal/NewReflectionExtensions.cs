#nullable enable  // This file is temporarily shared with xunit.v2.tests, which is not nullable-enabled

using System;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class NewReflectionExtensions
{
	/// <summary/>
	public static Type UnwrapNullable(this Type type)
	{
		if (!type.IsGenericType)
			return type;
		if (type.GetGenericTypeDefinition() != typeof(Nullable<>))
			return type;
		return type.GetGenericArguments()[0];
	}
}
