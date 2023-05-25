using System.Threading.Tasks;

[Target(BuildTarget.RestoreTools)]
public static class RestoreTools
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Restoring .NET Core command-line tools");

		await context.Exec("dotnet", $"tool restore --verbosity {context.Verbosity}");
	}
}
