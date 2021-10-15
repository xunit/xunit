using System.Threading.Tasks;

[Target(
	BuildTarget.FormatSource,
	BuildTarget.Restore
)]
public static class FormatSource
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Formatting source");

		await context.Exec("dotnet", $"dotnet-format --folder --verbosity {context.Verbosity} --exclude src/xunit.v3.assert/Asserts");
	}
}
