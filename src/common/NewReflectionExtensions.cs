using System;
using System.Reflection;

/// <summary>
/// Methods which help bridge and contain the differences between Type and TypeInfo.
/// </summary>
internal static class NewReflectionExtensions
{
    // New methods

    public static Assembly GetAssembly(this Type type)
    {
#if NEW_REFLECTION
        return type.GetTypeInfo().Assembly;
#else
        return type.Assembly;
#endif
    }

    public static MethodInfo GetDeclaredMethod(this Type type, string methodName)
    {
#if NEW_REFLECTION
        return type.GetTypeInfo().GetDeclaredMethod(methodName);
#else
        return type.GetMethod(methodName);
#endif
    }

    public static bool IsEnum(this Type type)
    {
#if NEW_REFLECTION
        return type.GetTypeInfo().IsEnum;
#else
        return type.IsEnum;
#endif
    }

    public static bool IsGenericType(this Type type)
    {
#if NEW_REFLECTION
        return type.GetTypeInfo().IsGenericType;
#else
        return type.IsGenericType;
#endif
    }

    public static bool IsNullableEnum(this Type type)
    {
        return type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>) && type.GetGenericArguments()[0].IsEnum();
    }

    public static bool IsValueType(this Type type)
    {
#if NEW_REFLECTION
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

#if NEW_REFLECTION
    public static Type[] GetGenericArguments(this Type type)
    {
        return type.GetTypeInfo().GenericTypeArguments;
    }

    public static bool IsAssignableFrom(this Type type, Type otherType)
    {
        return type.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());
    }
#endif
}