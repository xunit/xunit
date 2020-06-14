using System.IO;
using System.Threading.Tasks;

[Target(
    BuildTarget.Build,
    BuildTarget.Restore
)]
public static class Build
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Compiling binaries");

        var assertFiles = Directory.GetFiles(Path.Combine(context.BaseFolder, "src", "xunit.v3.assert", "Asserts"));
        var mediaFiles = Directory.GetFiles(Path.Combine(context.BaseFolder, "tools", "media"));
        if (assertFiles.Length == 0 || mediaFiles.Length == 0)
            await context.Exec("git", "submodule update --init");

        await context.Exec("dotnet", $"build --no-restore --configuration {context.ConfigurationText} --verbosity {context.Verbosity}");
        // await context.Exec("dotnet", $"msbuild src/xunit.console/xunit.console.csproj -p:Configuration={context.ConfigurationText} -p:Platform=x86");
    }
}
