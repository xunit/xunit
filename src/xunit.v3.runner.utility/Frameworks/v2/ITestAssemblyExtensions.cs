using System.Linq;
using Xunit.Abstractions;

static class ITestAssemblyExtensions
{
	public static string? GetTargetFramework(this IAssemblyInfo? assembly)
	{
		var attrib = assembly?.GetCustomAttributes("System.Runtime.Versioning.TargetFrameworkAttribute").FirstOrDefault();
		return attrib?.GetConstructorArguments().FirstOrDefault() as string;
	}
}
