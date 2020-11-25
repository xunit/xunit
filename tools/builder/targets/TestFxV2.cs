using System;
using System.IO;
using System.Threading.Tasks;

[Target(
	BuildTarget.TestFxV2,
	BuildTarget.Build
)]
public static class TestFxV2
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Running v2 .NET Framework tests");

		if (context.NeedMono)
		{
			context.WriteLineColor(ConsoleColor.Yellow, $"Skipping v2 tests on non-Windows machines.");
			Console.WriteLine();
			return;
		}

		var v2Folder = Path.Combine(context.BaseFolder, "src", "xunit.v2.tests", "bin", context.ConfigurationText, "net452");
		var v2OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v2.tests-net452");
		await context.Exec(context.ConsoleRunnerExe, $"{v2Folder}/xunit.v2.tests.dll {context.TestFlagsParallel}-serialize -appdomains required -xml \"{v2OutputFileName}.xml\" -html \"{v2OutputFileName}.html\"", workingDirectory: v2Folder);
	}
}
