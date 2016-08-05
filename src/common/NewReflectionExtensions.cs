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
#if PLATFORM_DOTNET
        return type.GetTypeInfo().Assembly;
#else
        return type.Assembly;
#endif
    }

    public static Attribute[] GetCustomAttributes(this Assembly assembly)
    {
#if PLATFORM_DOTNET
        return assembly.GetCustomAttributes<Attribute>().ToArray();
#else
        return assembly.GetCustomAttributes(false).Cast<Attribute>().ToArray();
#endif
    }

    public static bool IsEnum(this Type type)
    {
#if PLATFORM_DOTNET
        return type.GetTypeInfo().IsEnum;
#else
        return type.IsEnum;
#endif
    }

    public static bool IsFromLocalAssembly(this Type type)
    {
        var assemblyName = type.GetAssembly().GetName().Name;

        try
        {
#if PLATFORM_DOTNET
            Assembly.Load(new AssemblyName { Name = assemblyName });
#else
            Assembly.Load(assemblyName);
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
#if PLATFORM_DOTNET
        return type.GetTypeInfo().IsGenericType;
#else
        return type.IsGenericType;
#endif
    }

    public static bool IsGenericTypeDefinition(this Type type)
    {
#if PLATFORM_DOTNET
        return type.GetTypeInfo().IsGenericTypeDefinition;
#else
        return type.IsGenericTypeDefinition;
#endif
    }

    public static bool IsNullableEnum(this Type type)
    {
        return type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>) && type.GetGenericArguments()[0].IsEnum();
    }

    public static bool IsValueType(this Type type)
    {
#if PLATFORM_DOTNET
        return type.GetTypeInfo().IsValueType;
#else
        return type.IsValueType;
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

#if PLATFORM_DOTNET
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
