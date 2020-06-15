using System.Threading.Tasks;

[Target(
    BuildTarget.Build32,
    BuildTarget.Restore
)]
public static class Build32
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Compiling binaries (32-bit)");

        await context.Exec("dotnet", $"msbuild -verbosity:{context.Verbosity} -p:Configuration={context.ConfigurationText} -p:BuildX86=true");
    }
}
