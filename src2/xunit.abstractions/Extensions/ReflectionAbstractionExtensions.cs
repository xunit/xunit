using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;

public static class ReflectionAbstractionExtensions
{
    /// <summary>
    /// Gets the binding flags for finding this method via reflection.
    /// </summary>
    /// <param name="methodInfo">The method to get binding flags for.</param>
    /// <returns>The binding flags.</returns>
    public static BindingFlags GetBindingFlags(this IMethodInfo methodInfo)
    {
        BindingFlags bindingFlags = 0;

        if (methodInfo.IsPublic)
            bindingFlags |= BindingFlags.Public;
        else
            bindingFlags |= BindingFlags.NonPublic;

        if (methodInfo.IsStatic)
            bindingFlags |= BindingFlags.Static | BindingFlags.FlattenHierarchy;
        else
            bindingFlags |= BindingFlags.Instance;

        return bindingFlags;
    }

    /// <summary>
    /// Gets the binding flags for finding this method via reflection.
    /// </summary>
    /// <param name="methodInfo">The method to get binding flags for.</param>
    /// <returns>The binding flags.</returns>
    public static BindingFlags GetBindingFlags(this MethodInfo methodInfo)
    {
        BindingFlags bindingFlags = 0;

        if (methodInfo.IsPublic)
            bindingFlags |= BindingFlags.Public;
        else
            bindingFlags |= BindingFlags.NonPublic;

        if (methodInfo.IsStatic)
            bindingFlags |= BindingFlags.Static | BindingFlags.FlattenHierarchy;
        else
            bindingFlags |= BindingFlags.Instance;

        return bindingFlags;
    }

    /// <summary>
    /// Gets all the custom attributes for the given assembly.
    /// </summary>
    /// <param name="attributeType">The type of the attribute</param>
    /// <returns>The matching attributes that decorate the assembly</returns>
    public static IEnumerable<IAttributeInfo> GetCustomAttributes(this IAssemblyInfo assemblyInfo, Type attributeType)
    {
        return assemblyInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
    }

    /// <summary>
    /// Gets all the custom attributes for the given attribute.
    /// </summary>
    /// <param name="attributeType">The type of the attribute to find</param>
    /// <returns>The matching attributes that decorate the attribute</returns>
    public static IEnumerable<IAttributeInfo> GetCustomAttributes(this IAttributeInfo attributeInfo, Type attributeType)
    {
        return attributeInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
    }

    /// <summary>
    /// Gets all the custom attributes for the method that are of the given type.
    /// </summary>
    /// <param name="attributeType">The type of the attribute</param>
    /// <returns>The matching attributes that decorate the method</returns>
    public static IEnumerable<IAttributeInfo> GetCustomAttributes(this IMethodInfo methodInfo, Type attributeType)
    {
        return methodInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
    }

    /// <summary>
    /// Gets all the custom attributes for the given type.
    /// </summary>
    /// <param name="attributeType">The type of the attribute</param>
    /// <returns>The matching attributes that decorate the type</returns>
    public static IEnumerable<IAttributeInfo> GetCustomAttributes(this ITypeInfo typeInfo, Type attributeType)
    {
        return typeInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
    }
}