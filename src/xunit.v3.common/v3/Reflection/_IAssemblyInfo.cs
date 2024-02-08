using System.Collections.Generic;

namespace Xunit.v3;

/// <summary>
/// Represents information about an assembly. The primary implementation is based on runtime
/// reflection, but may also be implemented by runner authors to provide non-reflection-based
/// test discovery (for example, AST-based runners like CodeRush or Resharper).
/// </summary>
public interface _IAssemblyInfo
{
	/// <summary>
	/// Gets the on-disk location of the assembly under test. If the assembly path is not
	/// known (for example, in AST-based runners), you must return <c>null</c>.
	/// </summary>
	/// <remarks>
	/// This is used by the test framework wrappers to find the co-located unit test framework
	/// assembly (f.e., xunit.dll or xunit.execution.*.dll). AST-based runners will need to directly create
	/// instances of <see cref="T:Xunit.Xunit1"/> and <see cref="T:Xunit.Xunit2"/> (using the constructors that
	/// support an explicit path to the test framework DLL) rather than relying on the
	/// use of <see cref="T:Xunit.XunitFrontController"/>.
	/// </remarks>
	string? AssemblyPath { get; }

	/// <summary>
	/// Gets the assembly name. May return a fully qualified name for assemblies found via
	/// reflection (i.e., "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
	/// or may return just assembly name only for assemblies found via source code introspection
	/// (i.e., "mscorlib").
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets all the custom attributes for the assembly that are of the given attribute type.
	/// </summary>
	/// <param name="attributeType">The type of the attribute to find. Will accept attribute types that are concrete,
	/// closed generic, and open generic. When provided an open generic type (e.g., MyAttribute&lt;&gt;) it will
	/// return matching closed generic attributes (e.g., MyAttribute&gt;int&lt;)</param>
	/// <returns>The matching attributes that decorate the assembly</returns>
	IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(_ITypeInfo attributeType);

	/// <summary>
	/// Gets a <see cref="_ITypeInfo"/> for the given type.
	/// </summary>
	/// <param name="typeName">The fully qualified type name.</param>
	/// <returns>The <see cref="_ITypeInfo"/> if the type exists, or <c>null</c> if not.</returns>
	_ITypeInfo? GetType(string typeName);

	/// <summary>
	/// Gets all the types for the assembly.
	/// </summary>
	/// <param name="includePrivateTypes">Set to <c>true</c> to return all types in the assembly,
	/// or <c>false</c> to return only public types.</param>
	/// <returns>The types in the assembly.</returns>
	IReadOnlyCollection<_ITypeInfo> GetTypes(bool includePrivateTypes);
}
