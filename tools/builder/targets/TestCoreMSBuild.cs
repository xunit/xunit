using System.Threading.Tasks;
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

		if (context.NoX86)
			return;

		var x86Dotnet = context.GetDotnetX86Path(requireSdk: false);
		if (x86Dotnet is null)
			return;

		context.BuildStep($"Running .NET tests ({framework}, x86, via MSBuild runner)");

		await context.Exec("dotnet", $"msbuild tools/builder/msbuild/netcore.proj -target:{targetPrefix}_x86 -property:Configuration={context.ConfigurationText} -verbosity:{context.Verbosity} -nologo");
	}
}
