using System.Linq;
using System.Runtime.Versioning;
using Xunit.v3;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class IAssemblyInfoExtensions
{
	/// <summary/>
	public static string GetTargetFramework(this _IAssemblyInfo assembly)
	{
		Guard.ArgumentNotNull(assembly);

		string? result = null;

		var attrib = assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute)).FirstOrDefault();
		if (attrib is not null)
			result = attrib.GetConstructorArguments().Cast<string>().First();

		return result ?? AssemblyExtensions.UnknownTargetFramework;
	}
}
