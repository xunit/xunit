using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents information about a type.
    /// </summary>
    public interface ITypeInfo
    {
        /// <summary>
        /// Gets a value indicating whether the type is abstract.
        /// </summary>
        bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the type is sealed.
        /// </summary>
        bool IsSealed { get; }

        /// <summary>
        /// Gets the underlying <see cref="Type"/> object, if available.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets all the custom attributes for the type that are of the given attribute type.
        /// </summary>
        /// <param name="attributeType">The type of the attribute</param>
        /// <returns>The matching attributes that decorate the type</returns>
        IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType);

        /// <summary>
        /// Gets a test method by name.
        /// </summary>
        /// <param name="methodName">The name of the method</param>
        /// <returns>The method, if it exists; null, otherwise.</returns>
        IMethodInfo GetMethod(string methodName);

        /// <summary>
        /// Gets all the methods 
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "The execution of this may require non-trivial work, therefore it is appropriate as a method.")]
        IEnumerable<IMethodInfo> GetMethods();

        /// <summary>
        /// Determines if the type has at least one instance of the given attribute type.
        /// </summary>
        /// <param name="attributeType">The type of the attribute</param>
        /// <returns>True if the type has at least one instance of the given attribute type; false, otherwise</returns>
        bool HasAttribute(Type attributeType);

        /// <summary>
        /// Determines if the type implements the given interface.
        /// </summary>
        /// <param name="interfaceType">The type of the interface</param>
        /// <returns>True if the type implements the given interface; false, otherwise</returns>
        bool HasInterface(Type interfaceType);
    }
}