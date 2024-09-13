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
		await RunTests(context, "net6.0", "Net6");
		await RunTests(context, "net8.0", "Net8");
	}

	static async Task RunTests(
		BuildContext context,
		string framework,
		string targetPrefix)
	{
		// ------------- AnyCPU -------------

		context.BuildStep($"Running .NET tests ({framework}, AnyCPU, via MSBuild runner)");

		await context.Exec("dotnet", $"msbuild tools/builder/msbuild/netcore.proj -target:{targetPrefix}_AnyCPU -property:Configuration={context.ConfigurationText} -verbosity:{context.Verbosity} -nologo");

		// ------------- Forced x86 -------------

		// Only run 32-bit .NET Core tests on Windows
		if (context.NeedMono || context.NoX86)
			return;

		// Only run 32-bit .NET Core tests if 32-bit .NET Core is installed
		var programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
		if (programFilesX86 == null)
			return;

		var x86Dotnet = Path.Combine(programFilesX86, "dotnet", "dotnet.exe");
		if (!File.Exists(x86Dotnet))
			return;

		context.BuildStep($"Running .NET tests ({framework}, x86, via MSBuild runner)");

		await context.Exec("dotnet", $"msbuild tools/builder/msbuild/netcore.proj -target:{targetPrefix}_x86 -property:Configuration={context.ConfigurationText} -verbosity:{context.Verbosity} -nologo");
	}
}
