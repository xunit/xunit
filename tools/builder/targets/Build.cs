using System;
using System.IO;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

public static partial class Build
{
	public static partial async Task PerformBuild(BuildContext context)
	{
		context.BuildStep("Listing folders in C:\\Program Files\\dotnet\\packs\\Microsoft.NETCore.App.Ref");

		foreach (var directory in Directory.GetDirectories("C:\\Program Files\\dotnet\\packs\\Microsoft.NETCore.App.Ref"))
			Console.WriteLine("  {0}", directory);

		Console.WriteLine();

		context.BuildStep("Compiling binaries");

		var buildLog = Path.Combine(context.BuildArtifactsFolder, "build.binlog");

		await context.Exec("dotnet", $"msbuild -nologo -maxCpuCount -restore:False -verbosity:{context.Verbosity} -property:Configuration={context.ConfigurationText} -binaryLogger:{buildLog}");
	}
}
