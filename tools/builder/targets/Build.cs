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

        await context.Exec("dotnet", $"build --no-restore --configuration {context.ConfigurationText} --verbosity {context.Verbosity}");
        // await context.Exec("dotnet", $"msbuild src/xunit.console/xunit.console.csproj -p:Configuration={context.ConfigurationText} -p:Platform=x86");
    }
}
