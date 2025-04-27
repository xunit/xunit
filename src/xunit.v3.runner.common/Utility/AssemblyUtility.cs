using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Mono.Cecil;
using Xunit.Runner.Common;

namespace Xunit;

/// <summary>
/// Utility functions for assemblies.
/// </summary>
public static class AssemblyUtility
{
	/// <summary>
	/// Gets metadata (including target framework and xUnit.net version) for the given assembly (on disk).
	/// This uses Mono Cecil to prevent officially loading the assembly into memory.
	/// </summary>
	/// <param name="assemblyFileName">The assembly filename.</param>
	/// <returns>The assembly metadata, if the assembly was found; <c>null</c>, otherwise.</returns>
	public static AssemblyMetadata? GetAssemblyMetadata(string assemblyFileName)
	{
		if (!string.IsNullOrWhiteSpace(assemblyFileName) && File.Exists(assemblyFileName))
			try
			{
				assemblyFileName = Path.GetFullPath(assemblyFileName);

				using var moduleDefinition = ModuleDefinition.ReadModule(assemblyFileName);
				var targetFrameworkAttribute =
					moduleDefinition
						.GetCustomAttributes()
						.FirstOrDefault(ca => ca.AttributeType.FullName == typeof(TargetFrameworkAttribute).FullName);

				var targetFramework = targetFrameworkAttribute?.ConstructorArguments[0].Value as string;
				var xunitVersion = 0;

				// Trust for references is stronger than what's on disk, so try those first, and
				// we always want to consider the "highest" reference most important, since we have
				// test projects that might reference more than one for purposes of integration testing.
				var references = new HashSet<string>(moduleDefinition.AssemblyReferences.Select(r => r.Name), StringComparer.OrdinalIgnoreCase);
				if (references.Contains("xunit.v3.core"))
					xunitVersion = 3;
				else if (references.Contains("xunit.core"))
					xunitVersion = 2;
				else if (references.Contains("xunit"))
					xunitVersion = 1;
				// Fall back to looking for one of our desired files on disk
				else
				{
					var folder = Path.GetDirectoryName(assemblyFileName);
					if (folder is not null)
					{
						if (Directory.GetFiles(folder, "xunit.v3.core.dll").Length != 0)
							xunitVersion = 3;
						else if (Directory.GetFiles(folder, "xunit.core.dll").Length != 0)
							xunitVersion = 2;
						else if (Directory.GetFiles(folder, "xunit.dll").Length != 0)
							xunitVersion = 1;
					}
				}

				return new AssemblyMetadata(xunitVersion, targetFramework);
			}
			catch { }

		return null;
	}
}
