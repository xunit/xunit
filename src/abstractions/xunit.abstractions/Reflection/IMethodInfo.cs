using System.Collections.Generic;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents information about a method. The primary implementation is based on runtime
    /// reflection, but may also be implemented by runner authors to provide non-reflection-based
    /// test discovery (for example, AST-based runners like CodeRush or Resharper).
    /// </summary>
    public interface IMethodInfo
    {
        /// <summary>
        /// Gets a value indicating whether the method is abstract.
        /// </summary>
        bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the method is a generic definition (i.e., an open generic).
        /// </summary>
        bool IsGenericMethodDefinition { get; }

        /// <summary>
        /// Gets a value indicating whether the method is public.
        /// </summary>
        bool IsPublic { get; }

        /// <summary>
        /// Gets a value indicating whether the method is static.
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the fully qualified type name of the return type.
        /// </summary>
        ITypeInfo ReturnType { get; }

        /// <summary>
        /// Gets a value which represents the class that this method was
        /// reflected from (i.e., equivalent to MethodInfo.ReflectedType)
        /// </summary>
        ITypeInfo Type { get; }

        /// <summary>
        /// Gets all the custom attributes for the method that are of the given type.
        /// </summary>
        /// <param name="assemblyQualifiedAttributeTypeName">The type of the attribute, in assembly qualified form</param>
        /// <returns>The matching attributes that decorate the method</returns>
        IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName);

        /// <summary>
        /// Gets the types of the generic arguments for generic methods.
        /// </summary>
        /// <returns>The argument types.</returns>
        IEnumerable<ITypeInfo> GetGenericArguments();

        /// <summary>
        /// Gets information about the parameters to the method.
        /// </summary>
        /// <returns>The method's parameters.</returns>
        IEnumerable<IParameterInfo> GetParameters();

        /// <summary>
        /// Converts an open generic method into a closed generic method, using the provided type arguments.
        /// </summary>
        /// <param name="typeArguments">The type arguments to be used in the generic definition.</param>
        /// <returns>A new <see cref="IMethodInfo"/> that represents the closed generic method.</returns>
        IMethodInfo MakeGenericMethod(params ITypeInfo[] typeArguments);
    }
}