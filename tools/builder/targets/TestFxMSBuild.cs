using System;
using System.ComponentModel;
using System.Threading.Tasks;
using SimpleExec;

[Target(
	BuildTarget.TestFxMSBuild,
	BuildTarget.Build
)]
public static class TestFxMSBuild
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Running .NET Framework tests (via MSBuild runner)");

		try
		{
			await context.Exec("msbuild", $"tools/builder/msbuild/netfx.proj -target:TestV3 -property:Configuration={context.ConfigurationText} -verbosity:{context.Verbosity}");

			if (!context.NeedMono)
			{
				await context.Exec("msbuild", $"tools/builder/msbuild/netfx.proj -target:TestV2 -property:Configuration={context.ConfigurationText} -verbosity:{context.Verbosity}");
				await context.Exec("msbuild", $"tools/builder/msbuild/netfx.proj -target:TestV1 -property:Configuration={context.ConfigurationText} -verbosity:{context.Verbosity}");
			}
		}
		catch (Win32Exception ex)
		{
			if (ex.NativeErrorCode != 2)
				throw;

			context.WriteLineColor(ConsoleColor.Red, "Could not find 'msbuild.exe' on the system PATH. Please run the build from a developer command prompt.");
			throw new NonZeroExitCodeException(-2);
		}
	}
}
