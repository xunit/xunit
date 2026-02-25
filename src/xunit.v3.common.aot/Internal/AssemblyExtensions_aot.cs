using System.Reflection;
using System.Runtime.InteropServices;

namespace Xunit.Internal;

partial class AssemblyExtensions
{
	/// <summary>
	/// Safely gets the location of an assembly.
	/// </summary>
	/// <param name="assembly">The assembly.</param>
	/// <returns>If the assembly is null, or is dynamic, then it returns <see langword="null"/>; otherwise, it returns the value
	/// from <see cref="Assembly.Location"/>.</returns>
	public static string? GetSafeLocation(this Assembly? assembly)
	{
		if (assembly is null)
			return null;

		var assemblyName =
			assembly.GetName().Name
				?? throw new InvalidOperationException("Must be able to inspect the assembly name to get its location");

		// Unpublished Native AOT will still exist as .dll
		var result = Path.Combine(AppContext.BaseDirectory, assemblyName + ".dll");
		if (!File.Exists(result))
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				result = Path.Combine(AppContext.BaseDirectory, assemblyName + ".exe");
			else
				result = Path.Combine(AppContext.BaseDirectory, assemblyName);
		}

		return File.Exists(result) ? result : null;
	}
}
