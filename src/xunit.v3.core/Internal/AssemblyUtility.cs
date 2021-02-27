using System.Linq;
using System.Runtime.Versioning;
using Xunit.v3;

namespace Xunit.Internal
{
	static class AssemblyUtility
	{
		// Note: This value matches AssemblyUtility.UnknownTargetFramework from xunit.v3.runner.common
		public const string UnknownTargetFramework = "UnknownTargetFramework";

		public static string GetTargetFramework(_IAssemblyInfo assembly)
		{
			string? result = null;

			var attrib = assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute)).FirstOrDefault();
			if (attrib != null)
				result = attrib.GetConstructorArguments().Cast<string>().First();

			return result ?? UnknownTargetFramework;
		}
	}
}
