using System.Collections.Generic;

namespace Xunit.Sdk;

/// <summary>
/// Represents metadata about a test assembly.
/// </summary>
public interface IAssemblyMetadata
{
	/// <summary>
	/// Gets the assembly name. May return a simple assembly name (i.e., "mscorlib"), or may return a
	/// fully qualified name (i.e., "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").
	/// </summary>
	string AssemblyName { get; }

	/// <summary>
	/// Gets the on-disk location of the assembly under test.
	/// </summary>
	string AssemblyPath { get; }

	/// <summary>
	/// Gets the full path of the configuration file name, if one is present.
	/// May be <c>null</c> if there is no configuration file.
	/// </summary>
	string? ConfigFilePath { get; }

	/// <summary>
	/// Gets the trait values associated with this test assembly. If
	/// there are none, or the framework does not support traits,
	/// this should return an empty dictionary (not <c>null</c>).
	/// </summary>
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; }

	/// <summary>
	/// Gets the unique ID for this test assembly.
	/// </summary>
	/// <remarks>
	/// The unique identifier for a test assembly should be able to discriminate among test assemblies with
	/// their associated configuration file (so the same assembly with two different configuration files
	/// should have two different unique IDs). This identifier should remain stable until such time as
	/// the developer changes some fundamental part of the identity. Recompilation of the test assembly
	/// is reasonable as a stability changing event.
	/// </remarks>
	string UniqueID { get; }
}
