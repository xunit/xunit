using System;
using System.IO;
using System.Threading.Tasks;
using SimpleExec;

[Target(nameof(Build),
        nameof(Restore))]
public static class Build
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Compiling binaries");

        var assertFiles = Directory.GetFiles(Path.Combine(context.BaseFolder, "src", "xunit.assert", "Asserts"));
        if (assertFiles.Length == 0)
            await context.Exec("git", "submodule update --init");

        await context.Exec("dotnet", $"msbuild -p:Configuration={context.ConfigurationText}");
        await context.Exec("dotnet", $"msbuild src/xunit.console/xunit.console.csproj -p:Configuration={context.ConfigurationText} -p:Platform=x86");
    }
}
