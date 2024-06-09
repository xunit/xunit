using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using SimpleExec;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestCoreMSBuild,
	BuildTarget.Build
)]
public static class TestCoreMSBuild
{
	public static async Task OnExecute(BuildContext context)
	{
		try
		{
			await RunTests(context, "net6.0", "Net6");
			await RunTests(context, "net8.0", "Net8");
		}
		catch (Win32Exception ex)
		{
			if (ex.NativeErrorCode != 2)
				throw;

			context.WriteLineColor(ConsoleColor.Red, "Could not find 'msbuild.exe' on the system PATH. Please run the build from a developer command prompt.");
			throw new ExitCodeException(-2);
		}
	}

	static async Task RunTests(
		BuildContext context,
		string framework,
		string targetPrefix)
	{
		// ------------- AnyCPU -------------

		context.BuildStep($"Running .NET tests ({framework}, AnyCPU, via MSBuild runner)");

		await context.Exec("msbuild", $"tools/builder/msbuild/netcore.proj -target:{targetPrefix}_AnyCPU -property:Configuration={context.ConfigurationText} -verbosity:{context.Verbosity}");

		// ------------- Forced x86 -------------

		// Only run 32-bit .NET Core tests on Windows
		if (context.NeedMono)
			return;

		// Only run 32-bit .NET Core tests if 32-bit .NET Core is installed
		var programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
		if (programFilesX86 == null)
			return;

		var x86Dotnet = Path.Combine(programFilesX86, "dotnet", "dotnet.exe");
		if (!File.Exists(x86Dotnet))
			return;

		context.BuildStep($"Running .NET tests ({framework}, x86, via MSBuild runner)");

		await context.Exec("msbuild", $"tools/builder/msbuild/netcore.proj -target:{targetPrefix}_x86 -property:Configuration={context.ConfigurationText} -verbosity:{context.Verbosity}");
	}
}
