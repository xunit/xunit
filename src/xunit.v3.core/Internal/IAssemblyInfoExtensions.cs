using System.Linq;
using System.Runtime.Versioning;
using Xunit.v3;

namespace Xunit.Internal
{
	/// <summary>
	/// Internal extension methods for <see cref="_IAssemblyInfo"/>.
	/// </summary>
	public static class IAssemblyInfoExtensions
	{
		/// <summary>
		/// Gets the target framework name for the given assembly.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <returns>The target framework (typically in a format like ".NETFramework,Version=v4.7.2"
		/// or ".NETCoreApp,Version=v2.1"). If the target framework type is unknown (missing file,
		/// missing attribute, etc.) then returns "UnknownTargetFramework".</returns>
		public static string GetTargetFramework(this _IAssemblyInfo assembly)
		{
			Guard.ArgumentNotNull(nameof(assembly), assembly);

			string? result = null;

			var attrib = assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute)).FirstOrDefault();
			if (attrib != null)
				result = attrib.GetConstructorArguments().Cast<string>().First();

			return result ?? AssemblyExtensions.UnknownTargetFramework;
		}
	}
}
