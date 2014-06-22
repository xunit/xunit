using System.Collections.Generic;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents information about a type. The primary implementation is based on runtime
    /// reflection, but may also be implemented by runner authors to provide non-reflection-based
    /// test discovery (for example, AST-based runners like CodeRush or Resharper).
    /// </summary>
    public interface ITypeInfo
    {
        /// <summary>
        /// Gets the assembly this type is located in.
        /// </summary>
        IAssemblyInfo Assembly { get; }

        /// <summary>
        /// Gets the base type of the given type.
        /// </summary>
        ITypeInfo BaseType { get; }

        /// <summary>
        /// Gets the interfaces implemented by the given type.
        /// </summary>
        IEnumerable<ITypeInfo> Interfaces { get; }

        /// <summary>
        /// Gets a value indicating whether the type is abstract.
        /// </summary>
        bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the type represents a generic parameter.
        /// </summary>
        bool IsGenericParameter { get; }

        /// <summary>
        /// Gets a value indicating whether the type is a generic type.
        /// </summary>
        bool IsGenericType { get; }

        /// <summary>
        /// Gets a value indicating whether the type is sealed.
        /// </summary>
        bool IsSealed { get; }

        /// <summary>
        /// Gets a value indicating whether the type is a value type.
        /// </summary>
        bool IsValueType { get; }

        /// <summary>
        /// Gets the fully qualified type name (for non-generic parameters), or the
        /// simple type name (for generic parameters).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets all the custom attributes for the given type.
        /// </summary>
        /// <param name="assemblyQualifiedAttributeTypeName">The type of the attribute, in assembly qualified form</param>
        /// <returns>The matching attributes that decorate the type</returns>
        IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName);

        /// <summary>
        /// Gets the generic type arguments for a generic type.
        /// </summary>
        /// <returns>The list of generic types.</returns>
        IEnumerable<ITypeInfo> GetGenericArguments();

        /// <summary>
        /// Gets a specific method.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="includePrivateMethod">Set to <c>true</c> to look for the method in both public and private.</param>
        /// <returns>The method.</returns>
        IMethodInfo GetMethod(string methodName, bool includePrivateMethod);

        /// <summary>
        /// Gets all the methods in this type.
        /// </summary>
        /// <param name="includePrivateMethods">Set to <c>true</c> to return all methods in the type,
        /// or <c>false</c> to return only public methods.</param>
        IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods);
    }
}