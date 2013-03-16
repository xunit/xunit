using System;
using System.Collections.Generic;
using Xunit.Abstractions;

public static class ReflectionAbstractionExtensions
{
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