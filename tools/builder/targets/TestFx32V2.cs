using System;
using System.IO;
using System.Threading.Tasks;

[Target(
	BuildTarget.TestFx32V2,
	BuildTarget.Build32
)]
public static class TestFx32V2
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Running v2 .NET Framework (32-bit) tests");

		if (context.NeedMono)
		{
			context.WriteLineColor(ConsoleColor.Yellow, $"Skipping v2 tests on non-Windows machines.");
			Console.WriteLine();
			return;
		}

		var v2Folder = Path.Combine(context.BaseFolder, "src", "xunit.v2.tests", "bin", context.ConfigurationText, "net452");
		var v2OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v2.tests-net452-x86");
		await context.Exec(context.ConsoleRunner32Exe, $"{v2Folder}/xunit.v2.tests.dll {context.TestFlagsParallel}-appdomains denied -xml \"{v2OutputFileName}.xml\" -html \"{v2OutputFileName}.html\"", workingDirectory: v2Folder);
	}
}
