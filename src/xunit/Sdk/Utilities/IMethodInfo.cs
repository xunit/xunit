using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents information about a method.
    /// </summary>
    public interface IMethodInfo
    {
        /// <summary>
        /// Gets a value which represents the class that this method was
        /// reflected from (i.e., equivalent to MethodInfo.ReflectedType)
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Class", Justification = "This would be a breaking change.")]
        ITypeInfo Class { get; }

        /// <summary>
        /// Gets a value indicating whether the method is abstract.
        /// </summary>
        bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the method is static.
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Gets the underlying <see cref="MethodInfo"/> for the method, if available.
        /// </summary>
        MethodInfo MethodInfo { get; }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the fully qualified type name of the return type.
        /// </summary>
        string ReturnType { get; }

        /// <summary>
        /// Gets the fully qualified type name of the type that this method belongs to. If
        /// using reflection, this should be the ReflectedType.
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Creates an instance of the type where this test method was found. If using
        /// reflection, this should be the ReflectedType.
        /// </summary>
        /// <returns>A new instance of the type.</returns>
        object CreateInstance();

        /// <summary>
        /// Gets all the custom attributes for the method that are of the given type.
        /// </summary>
        /// <param name="attributeType">The type of the attribute</param>
        /// <returns>The matching attributes that decorate the method</returns>
        IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType);

        /// <summary>
        /// Determines if the method has at least one instance of the given attribute type.
        /// </summary>
        /// <param name="attributeType">The type of the attribute</param>
        /// <returns>True if the method has at least one instance of the given attribute type; false, otherwise</returns>
        bool HasAttribute(Type attributeType);

        /// <summary>
        /// Invokes the test on the given class, with the given parameters.
        /// </summary>
        /// <param name="testClass">The instance of the test class (may be null if
        /// the test method is static).</param>
        /// <param name="parameters">The parameters to be passed to the test method.</param>
        void Invoke(object testClass, params object[] parameters);
    }
}