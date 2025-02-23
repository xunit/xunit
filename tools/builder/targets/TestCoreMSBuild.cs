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
		await RunTests(context, "net8.0");
	}

	static async Task RunTests(
		BuildContext context,
		string framework)
	{
		// ------------- AnyCPU -------------

		context.BuildStep($"Running .NET tests ({framework}, AnyCPU, via MSBuild runner)");

		await context.Exec("dotnet", $"msbuild tools/builder/msbuild/netcore.proj -target:Test_AnyCPU -property:Configuration={context.ConfigurationText} -verbosity:{context.Verbosity} -nologo");

		// ------------- Forced x86 -------------

		if (context.NoX86)
			return;

		var x86Dotnet = context.GetDotnetX86Path(requireSdk: false);
		if (x86Dotnet is null)
			return;

		context.BuildStep($"Running .NET tests ({framework}, x86, via MSBuild runner)");

		await context.Exec("dotnet", $"msbuild tools/builder/msbuild/netcore.proj -target:Test_x86 -property:Configuration={context.ConfigurationText} -verbosity:{context.Verbosity} -nologo");
	}
}
