using System;
using System.IO;
using System.Threading.Tasks;

[Target(
	BuildTarget.TestFxV1,
	BuildTarget.Build
)]
public static class TestFxV1
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Running v1 .NET Framework tests");

		if (context.NeedMono)
		{
			context.WriteLineColor(ConsoleColor.Yellow, $"Skipping v1 tests on non-Windows machines.");
			Console.WriteLine();
			return;
		}

		var v1Folder = Path.Combine(context.BaseFolder, "src", "xunit.v1.tests", "bin", context.ConfigurationText, "net45");
		var v1OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v1.tests-net45");
		await context.Exec(context.ConsoleRunnerExe, $"{v1Folder}/xunit.v1.tests.dll {context.TestFlagsParallel}-serialize -appdomains denied -xml \"{v1OutputFileName}.xml\" -html \"{v1OutputFileName}.html\"", workingDirectory: v1Folder);
	}
}
