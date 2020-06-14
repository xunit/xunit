using System;
using System.Linq;
using System.Reflection;

/// <summary>
/// Methods which help bridge and contain the differences between Type and TypeInfo.
/// </summary>
static class NewReflectionExtensions
{
    // New methods

    public static Assembly GetAssembly(this Type type)
    {
#if NET35
        return type.Assembly;
#else
        return type.GetTypeInfo().Assembly;
#endif
    }

    public static Attribute[] GetCustomAttributes(this Assembly assembly)
    {
#if NET35
        return assembly.GetCustomAttributes(false).Cast<Attribute>().ToArray();
#else
        return assembly.GetCustomAttributes<Attribute>().ToArray();
#endif
    }

    public static bool IsEnum(this Type type)
    {
#if NET35
        return type.IsEnum;
#else
        return type.GetTypeInfo().IsEnum;
#endif
    }

    public static bool IsFromLocalAssembly(this Type type)
    {
        var assemblyName = type.GetAssembly().GetName().Name;

        try
        {
#if NET35
            Assembly.Load(assemblyName);
#else
            Assembly.Load(new AssemblyName { Name = assemblyName });
#endif
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsGenericType(this Type type)
    {
#if NET35
        return type.IsGenericType;
#else
        return type.GetTypeInfo().IsGenericType;
#endif
    }

    public static bool IsGenericTypeDefinition(this Type type)
    {
#if NET35
        return type.IsGenericTypeDefinition;
#else
        return type.GetTypeInfo().IsGenericTypeDefinition;
#endif
    }

    public static bool IsNullableEnum(this Type type)
    {
        return type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>) && type.GetGenericArguments()[0].IsEnum();
    }

    public static bool IsValueType(this Type type)
    {
#if NET35
        return type.IsValueType;
#else
        return type.GetTypeInfo().IsValueType;
#endif
    }

    public static Type UnwrapNullable(this Type type)
    {
        if (!type.IsGenericType())
            return type;
        if (type.GetGenericTypeDefinition() != typeof(Nullable<>))
            return type;
        return type.GetGenericArguments()[0];
    }

    // Existing methods

#if !NET35
    public static Type[] GetGenericArguments(this Type type)
    {
        return type.GetTypeInfo().GenericTypeArguments;
    }

    public static Type[] GetInterfaces(this Type type)
    {
        return type.GetTypeInfo().ImplementedInterfaces.ToArray();
    }

    public static bool IsAssignableFrom(this Type type, Type otherType)
    {
        return type.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());
    }
#endif
}
