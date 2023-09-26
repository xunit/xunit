using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Mono.Cecil;
using Xunit.Internal;

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
	public static string GetTargetFramework(string assemblyFileName)
	{
		if (!string.IsNullOrWhiteSpace(assemblyFileName) && File.Exists(assemblyFileName))
		{
			try
			{
				var moduleDefinition = ModuleDefinition.ReadModule(assemblyFileName);
				var targetFrameworkAttribute =
					moduleDefinition
						.GetCustomAttributes()
						.FirstOrDefault(ca => ca.AttributeType.FullName == typeof(TargetFrameworkAttribute).FullName);

				if (targetFrameworkAttribute is not null)
				{
					var ctorArg = targetFrameworkAttribute.ConstructorArguments[0];
					if (ctorArg.Value is string targetFramework)
						return targetFramework;
				}
			}
			catch { }  // Eat exceptions so we just return our unknown value
		}

		return AssemblyExtensions.UnknownTargetFramework;
	}
}
