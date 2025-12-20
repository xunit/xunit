using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestMSBuild,
	BuildTarget.Build
)]
public static class TestMSBuild
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep($"Running tests [via xunit.v3.runner.msbuild]");

		var noNetCoreX86 = context.GetDotnetX86Path(requireSdk: false) is null;

		// ------------- v3 via 'dotnet msbuild' -------------

		await context.Exec("dotnet", $"msbuild tools/builder/msbuild/dotnet-msbuild.proj -tl:off -p:Configuration={context.ConfigurationText} -p:SkipX86={context.NoX86} -p:SkipNetCoreX86={noNetCoreX86} -p:SkipNetFx={!context.IsWindows} -p:TestFramework={context.TestFramework} -v:{context.Verbosity}");

		if (context.V3Only || !context.IsWindows)
			return;

		// ------------- v1/v2 via 'msbuild' -------------

		if (context.TestFramework != Framework.Net)
			await context.Exec("msbuild", $"tools/builder/msbuild/msbuild.proj -tl:off -p:Configuration={context.ConfigurationText} -v:{context.Verbosity}");
	}
}
