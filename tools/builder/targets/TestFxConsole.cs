using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(
	BuildTarget.TestFxConsole,
	BuildTarget.Build
)]
public static class TestFxConsole
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Running .NET Framework tests (via Console runner)");

		// v1
		var v1Folder = Path.Combine(context.BaseFolder, "src", "xunit.v1.tests", "bin", context.ConfigurationText, "net45");
		var v1OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v1.tests-net45");
		await context.Exec(context.ConsoleRunnerExe, $"{v1Folder}/xunit.v1.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v1OutputFileName}.xml\" -html \"{v1OutputFileName}.html\"", workingDirectory: v1Folder);
		await context.Exec(context.ConsoleRunner32Exe, $"{v1Folder}/xunit.v1.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v1OutputFileName}-x86.xml\" -html \"{v1OutputFileName}-x86.html\"", workingDirectory: v1Folder);

		// v2
		var v2Folder = Path.Combine(context.BaseFolder, "src", "xunit.v2.tests", "bin", context.ConfigurationText, "net452");
		var v2OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v2.tests-net452");
		await context.Exec(context.ConsoleRunnerExe, $"{v2Folder}/xunit.v2.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v2OutputFileName}.xml\" -html \"{v2OutputFileName}.html\"", workingDirectory: v2Folder);
		await context.Exec(context.ConsoleRunner32Exe, $"{v2Folder}/xunit.v2.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v2OutputFileName}-x86.xml\" -html \"{v2OutputFileName}-x86.html\"", workingDirectory: v2Folder);

		// v3
		// TODO: Convert to console runner when it's available
		var netFxSubpath = Path.Combine("bin", context.ConfigurationText, "net4");
		var v3NetFxTestExes =
			Directory.GetFiles(context.BaseFolder, "xunit.v3.*.tests.exe", SearchOption.AllDirectories)
				.Where(x => x.Contains(netFxSubpath))
				.OrderBy(x => x);

		foreach (var v3NetFxTestExe in v3NetFxTestExes)
		{
			var folder = Path.GetDirectoryName(v3NetFxTestExe);
			var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3NetFxTestExe) + "-" + Path.GetFileName(folder));
			await context.Exec(v3NetFxTestExe, $"{context.TestFlagsParallel}-xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
		}

		// Can't run 32-bit binaries on Mono (no side-by-side 32- and 64-bit, unlike .NET Framework on Windows)
		if (!context.NeedMono)
		{
			var netFx32Subpath = Path.Combine("bin", context.ConfigurationText + "_x86", "net4");
			var v3NetFx32TestExes = Directory.GetFiles(context.BaseFolder, "xunit.v3.*.tests.exe", SearchOption.AllDirectories)
				.Where(x => x.Contains(netFx32Subpath))
				.OrderBy(x => x);

			foreach (var v3NetFx32TestExe in v3NetFx32TestExes)
			{
				var folder = Path.GetDirectoryName(v3NetFx32TestExe);
				var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3NetFx32TestExe) + "-" + Path.GetFileName(folder) + "-x86");
				await context.Exec(v3NetFx32TestExe, $"{context.TestFlagsParallel}-xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
			}
		}
	}
}
