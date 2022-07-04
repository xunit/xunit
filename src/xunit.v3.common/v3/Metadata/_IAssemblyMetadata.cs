namespace Xunit.v3;

/// <summary>
/// Represents metadata about a test assembly.
/// </summary>
public interface _IAssemblyMetadata
{
	/// <summary>
	/// Gets the assembly name. May return a fully qualified name for assemblies found via
	/// reflection (i.e., "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
	/// or may return just assembly name only for assemblies found via source code introspection
	/// (i.e., "mscorlib").
	/// </summary>
	string AssemblyName { get; }

	/// <summary>
	/// Gets the on-disk location of the assembly under test. If the assembly path is not
	/// known (for example, in AST-based runners), you must return <c>null</c>.
	/// </summary>
	string? AssemblyPath { get; }

	/// <summary>
	/// Gets the full path of the configuration file name, if one is present.
	/// May be <c>null</c> if there is no configuration file.
	/// </summary>
	string? ConfigFilePath { get; }
}
