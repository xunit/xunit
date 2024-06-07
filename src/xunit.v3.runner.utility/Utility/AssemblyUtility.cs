using System;
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
	/// Gets the target framework name for the given assembly (on disk). This uses Mono Cecil
	/// to prevent loading the assembly into memory.
	/// </summary>
	/// <param name="assemblyFileName">The assembly filename.</param>
	/// <returns>The target framework (typically in a format like ".NETFramework,Version=v4.7.2"
	/// or ".NETCoreApp,Version=v6.0"). If the target framework type is unknown (missing file,
	/// missing attribute, etc.) then returns "UnknownTargetFramework".</returns>
	public static AssemblyMetadata? GetAssemblyMetadata(string assemblyFileName)
	{
		if (!string.IsNullOrWhiteSpace(assemblyFileName) && File.Exists(assemblyFileName))
			try
			{
				var moduleDefinition = ModuleDefinition.ReadModule(assemblyFileName);
				var targetFrameworkAttribute =
					moduleDefinition
						.GetCustomAttributes()
						.FirstOrDefault(ca => ca.AttributeType.FullName == typeof(TargetFrameworkAttribute).FullName);

				var targetFramework = targetFrameworkAttribute?.ConstructorArguments[0].Value as string;
				var xunitVersion = 0;

				for (int idx = 0; xunitVersion == 0 && idx < moduleDefinition.AssemblyReferences.Count; ++idx)
				{
					var reference = moduleDefinition.AssemblyReferences[idx].Name;
					if (reference.Equals("xunit", StringComparison.OrdinalIgnoreCase))
						xunitVersion = 1;
					else if (reference.Equals("xunit.core", StringComparison.OrdinalIgnoreCase))
						xunitVersion = 2;
					else if (reference.Equals("xunit.v3.core", StringComparison.OrdinalIgnoreCase))
						xunitVersion = 3;
				}

				return new AssemblyMetadata(xunitVersion, targetFramework);
			}
			catch { }

		return null;
	}
}
