using System.IO;
using System.Threading.Tasks;

[Target(
	BuildTarget.TestCoreV2,
	BuildTarget.Build
)]
public static class TestCoreV2
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Running v2 .NET Core tests");

		await context.Exec("dotnet", $"test --no-build --nologo --configuration {context.ConfigurationText} --framework netcoreapp2.1 --verbosity quiet", workingDirectory: Path.Combine(context.BaseFolder, "src", "xunit.v2.tests"));
	}
}
