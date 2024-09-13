using System;
using System.ComponentModel;
using System.Threading.Tasks;
using SimpleExec;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestFxMSBuild,
	BuildTarget.Build
)]
public static class TestFxMSBuild
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Running .NET Framework tests (AnyCPU, via MSBuild runner)");

		try
		{
			await context.Exec("msbuild", $"tools/builder/msbuild/netfx.proj -target:TestV3_AnyCPU -property:Configuration={context.ConfigurationText} -verbosity:{context.Verbosity}");

			// Mono is only supported for v3 at the installed bitness
			if (context.NeedMono)
				return;

			if (!context.V3Only)
			{
				await context.Exec("msbuild", $"tools/builder/msbuild/netfx.proj -target:TestV2 -property:Configuration={context.ConfigurationText} -verbosity:{context.Verbosity}");
				await context.Exec("msbuild", $"tools/builder/msbuild/netfx.proj -target:TestV1 -property:Configuration={context.ConfigurationText} -verbosity:{context.Verbosity}");
			}

			if (!context.NoX86)
			{
				context.BuildStep("Running .NET Framework tests (x86, via MSBuild runner)");

				await context.Exec("msbuild", $"tools/builder/msbuild/netfx.proj -target:TestV3_x86 -property:Configuration={context.ConfigurationText} -verbosity:{context.Verbosity}");
			}
		}
		catch (Win32Exception ex)
		{
			if (ex.NativeErrorCode != 2)
				throw;

			context.WriteLineColor(ConsoleColor.Yellow, "Skipping MSBuild tests because 'msbuild' is not on the system PATH.");
			Console.WriteLine();
		}
	}
}
