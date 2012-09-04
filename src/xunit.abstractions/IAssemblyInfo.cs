using System;
using System.Collections.Generic;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents information about an assembly. The primary implementation is based on runtime
    /// reflection, but may also be implemented by runner authors to provide non-reflection-based
    /// test discovery (for example, AST-based runners like CodeRush or Resharper).
    /// </summary>
    public interface IAssemblyInfo
    {
        /// <summary>
        /// Gets all the custom attributes for the given assembly.
        /// </summary>
        /// <param name="attributeType">The type of the attribute</param>
        /// <returns>The matching attributes that decorate the assembly</returns>
        IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType);

        /// <summary>
        /// Gets all the types for the assembly.
        /// </summary>
        /// <param name="includePrivateTypes">Set to <c>true</c> to return all types in the assembly,
        /// or <c>false</c> to return only public types.</param>
        /// <returns>Returns the types in the assembly.</returns>
        IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes);
    }
}
