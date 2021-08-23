using System.Threading.Tasks;

[Target(
	BuildTarget.Build,
	BuildTarget.Restore
)]
public static class Build
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Compiling binaries (AnyCPU)");

		await context.Exec("dotnet", $"msbuild -maxCpuCount -restore:False -verbosity:{context.Verbosity} -p:Configuration={context.ConfigurationText}");

		context.BuildStep("Compiling binaries (32-bit)");

		await context.Exec("dotnet", $"msbuild -maxCpuCount -restore:False -verbosity:{context.Verbosity} -p:Configuration={context.ConfigurationText}_x86");
	}
}
