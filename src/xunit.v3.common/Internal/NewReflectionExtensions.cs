#nullable enable  // This file is temporarily shared with xunit.v2.tests, which is not nullable-enabled

using System;
using System.Linq;
using System.Reflection;

namespace Xunit.Internal
{
	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
	public static class NewReflectionExtensions
	{
		/// <summary/>
		public static Attribute[] GetCustomAttributes(this Assembly assembly) =>
			assembly.GetCustomAttributes(false).Cast<Attribute>().ToArray();

		/// <summary/>
		public static bool IsFromLocalAssembly(this Type type)
		{
			var assemblyName = type.Assembly.GetName().Name;

			try
			{
				Assembly.Load(assemblyName ?? throw new ArgumentException($"Could not find the name of the assembly for type '{type.FullName}'"));
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary/>
		public static bool IsNullableEnum(this Type type) =>
			type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && type.GetGenericArguments()[0].IsEnum;

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
}
