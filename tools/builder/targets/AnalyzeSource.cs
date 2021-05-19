using System.Threading.Tasks;

[Target(
	BuildTarget.AnalyzeSource,
	BuildTarget.Restore
)]
public static class AnalyzeSource
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Analyzing source");

		await context.Exec("dotnet", $"format -f --check --exclude src/xunit.v3.assert/Asserts --verbosity {context.Verbosity}");
	}
}
