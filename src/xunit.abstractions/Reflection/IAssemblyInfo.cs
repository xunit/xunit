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
        /// Gets the on-disk location of the assembly under test. If the assembly path is not
        /// known (for example, in AST-based runners), you must return <c>null</c>.
        /// </summary>
        /// <remarks>
        /// This is used by the test framework wrappers to find the co-located unit test framework
        /// assembly (f.e., xunit.dll or xunit.execution.dll). AST-based runners will need to directly create
        /// instances of <see cref="T:Xunit.Xunit1"/> and <see cref="T:Xunit.Xunit2"/> (using the constructors that
        /// support an explicit path to the test framework DLL) rather than relying on the
        /// use of <see cref="T:Xunit.XunitFrontController"/>.
        /// </remarks>
        string AssemblyPath { get; }

        /// <summary>
        /// Gets the assembly name. May return a fully qualified name for assemblies found via
        /// reflection (i.e., "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
        /// or may return just assembly name only for assemblies found via source code introspection
        /// (i.e., "mscorlib").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets all the custom attributes for the given assembly.
        /// </summary>
        /// <param name="assemblyQualifiedAttributeTypeName">The type of the attribute, in assembly-qualified form</param>
        /// <returns>The matching attributes that decorate the assembly</returns>
        IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName);

        /// <summary>
        /// Gets a <see cref="ITypeInfo"/> for the given type.
        /// </summary>
        /// <param name="typeName">The fully qualified type name.</param>
        /// <returns>The <see cref="ITypeInfo"/> if the type exists, or <c>null</c> if not.</returns>
        ITypeInfo GetType(string typeName);

        /// <summary>
        /// Gets all the types for the assembly.
        /// </summary>
        /// <param name="includePrivateTypes">Set to <c>true</c> to return all types in the assembly,
        /// or <c>false</c> to return only public types.</param>
        /// <returns>The types in the assembly.</returns>
        IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes);
    }
}