using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Xunit.Internal;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents an assembly in an <see cref="XunitProject"/>.
/// </summary>
/// <param name="project">The project this assembly belongs to.</param>
/// <param name="assemblyFileName">The assembly filename</param>
/// <param name="assemblyMetadata">The assembly metadata</param>
public class XunitProjectAssembly(
	XunitProject project,
	string assemblyFileName,
	AssemblyMetadata assemblyMetadata)
{
	/// <summary>
	/// Gets or sets the assembly under test. May be <c>null</c> when the test assembly is not
	/// loaded into the current <see cref="AppDomain"/>.
	/// </summary>
	public Assembly? Assembly { get; set; }

	/// <summary>
	/// Gets the assembly display name.
	/// </summary>
	public string AssemblyDisplayName =>
		Path.GetFileNameWithoutExtension(AssemblyFileName);

	/// <summary>
	/// Gets or sets the assembly file name.
	/// </summary>
	public string AssemblyFileName { get; set; } = Guard.ArgumentNotNull(assemblyFileName);

	/// <summary>
	/// Gets or sets the metadata about the assembly.
	/// </summary>
	public AssemblyMetadata AssemblyMetadata { get; set; } = Guard.ArgumentNotNull(assemblyMetadata);

	/// <summary>
	/// Gets or sets the config file name.
	/// </summary>
	public string? ConfigFileName { get; set; }

	/// <summary>
	/// Gets the configuration values for the test assembly.
	/// </summary>
	public TestAssemblyConfiguration Configuration { get; } = new();

	/// <summary>
	/// Gets an identifier for the current assembly. This is guaranteed to be unique, but not necessarily repeatable
	/// across runs (because it relies on <see cref="Assembly.GetHashCode"/>).
	/// </summary>
	public string Identifier =>
		ConfigFileName is null
			? AssemblyFileName
			: string.Format(CultureInfo.InvariantCulture, "{0} :: {1}", AssemblyFileName, ConfigFileName);

	/// <summary>
	/// Gets the project that this project assembly belongs to.
	/// </summary>
	public XunitProject Project { get; } = Guard.ArgumentNotNull(project);

	/// <summary>
	/// Gets a list of serialized test cases to be run. If the list is empty, then all test cases
	/// (that match the filters) will be run.
	/// </summary>
	public List<string> TestCasesToRun { get; } = [];
}
