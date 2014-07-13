using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

/// <summary>
/// This class represents utility methods needed to supplement the
/// reflection capabilities provided by the CLR
/// </summary>
public static class ReflectionAbstractionExtensions
{
    /// <summary>
    /// Gets a MethodInfo instance from an IMethodInfo
    /// </summary>
    /// <param name="type">The type</param>
    /// <param name="methodInfo">The method</param>
    /// <returns>The reflection method information</returns>
    public static MethodInfo GetMethodInfoFromIMethodInfo(this Type type, IMethodInfo methodInfo)
    {
        // The old logic only flattened hierarchy for static methods
        var methods = from method in methodInfo.IsStatic ? type.GetRuntimeMethods() : type.GetTypeInfo().DeclaredMethods
                      where method.IsPublic == methodInfo.IsPublic &&
                            method.IsStatic == methodInfo.IsStatic &&
                            method.Name == methodInfo.Name
                      select method;

        return methods.SingleOrDefault();
    }

    /// <summary>
    /// Gets methods in the target type that match the protection level of the supplied method
    /// </summary>
    /// <param name="type">The type</param>
    /// <param name="methodInfo">The method</param>
    /// <returns>The reflection method informations that match</returns>
    public static IEnumerable<MethodInfo> GetMatchingMethods(this Type type, MethodInfo methodInfo)
    {
        var methods = from method in methodInfo.IsStatic ? type.GetRuntimeMethods() : type.GetTypeInfo().DeclaredMethods
                      where method.IsPublic == methodInfo.IsPublic &&
                            method.IsStatic == methodInfo.IsStatic
                      select method;

        return methods;
    }

    /// <summary>
    /// Gets all the custom attributes for the given assembly.
    /// </summary>
    /// <param name="assemblyInfo">The assembly</param>
    /// <param name="attributeType">The type of the attribute</param>
    /// <returns>The matching attributes that decorate the assembly</returns>
    public static IEnumerable<IAttributeInfo> GetCustomAttributes(this IAssemblyInfo assemblyInfo, Type attributeType)
    {
        return assemblyInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
    }

    /// <summary>
    /// Gets all the custom attributes for the given attribute.
    /// </summary>
    /// <param name="attributeInfo">The attribute</param>
    /// <param name="attributeType">The type of the attribute to find</param>
    /// <returns>The matching attributes that decorate the attribute</returns>
    public static IEnumerable<IAttributeInfo> GetCustomAttributes(this IAttributeInfo attributeInfo, Type attributeType)
    {
        return attributeInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
    }

    /// <summary>
    /// Gets all the custom attributes for the method that are of the given type.
    /// </summary>
    /// <param name="methodInfo">The method</param>
    /// <param name="attributeType">The type of the attribute</param>
    /// <returns>The matching attributes that decorate the method</returns>
    public static IEnumerable<IAttributeInfo> GetCustomAttributes(this IMethodInfo methodInfo, Type attributeType)
    {
        return methodInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
    }

    /// <summary>
    /// Gets all the custom attributes for the given type.
    /// </summary>
    /// <param name="typeInfo">The type</param>
    /// <param name="attributeType">The type of the attribute</param>
    /// <returns>The matching attributes that decorate the type</returns>
    public static IEnumerable<IAttributeInfo> GetCustomAttributes(this ITypeInfo typeInfo, Type attributeType)
    {
        return typeInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
    }
}