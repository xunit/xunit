#nullable enable

using System;
using System.Linq;
using System.Reflection;

static class NewReflectionExtensions
{
	public static Attribute[] GetCustomAttributes(this Assembly assembly) =>
		assembly.GetCustomAttributes(false).Cast<Attribute>().ToArray();

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

	public static bool IsNullableEnum(this Type type) =>
		type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && type.GetGenericArguments()[0].IsEnum;

	public static Type UnwrapNullable(this Type type)
	{
		if (!type.IsGenericType)
			return type;
		if (type.GetGenericTypeDefinition() != typeof(Nullable<>))
			return type;
		return type.GetGenericArguments()[0];
	}
}
