using System;
using System.IO;
using System.Threading.Tasks;

[Target(
	BuildTarget.TestFx32V1,
	BuildTarget.Build32
)]
public static class TestFx32V1
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Running v1 .NET Framework (32-bit) tests");

		if (context.NeedMono)
		{
			context.WriteLineColor(ConsoleColor.Yellow, $"Skipping v1 tests on non-Windows machines.");
			Console.WriteLine();
			return;
		}

		var v1Folder = Path.Combine(context.BaseFolder, "src", "xunit.v1.tests", "bin", context.ConfigurationText, "net45");
		var v1OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v1.tests-net45-x86");
		await context.Exec(context.ConsoleRunner32Exe, $"{v1Folder}/xunit.v1.tests.dll {context.TestFlagsParallel}-serialize -appdomains required -xml \"{v1OutputFileName}.xml\" -html \"{v1OutputFileName}.html\"", workingDirectory: v1Folder);
	}
}
