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

		// v3
		var netFxSubpath = Path.Combine("bin", context.ConfigurationText, "net4");
		var netFx32Subpath = Path.Combine("bin", context.ConfigurationText + "_x86", "net4");

		var v3TestExes =
			Directory
				.GetFiles(context.BaseFolder, "xunit.v3.*.tests.exe", SearchOption.AllDirectories)
				.Where(x => x.Contains(netFxSubpath));

		foreach (var v3TestExe in v3TestExes.OrderBy(x => x))
		{
			var folder = Path.GetDirectoryName(v3TestExe);
			var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3TestExe) + "-" + Path.GetFileName(folder));
			await context.Exec(v3TestExe, $"{context.TestFlagsParallel}-xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
		}

		if (!context.NeedMono)
		{
			var v3x86TestExes =
				Directory
					.GetFiles(context.BaseFolder, "xunit.v3.*.tests.exe", SearchOption.AllDirectories)
					.Where(x => x.Contains(netFx32Subpath));

			foreach (var v3x86TestExe in v3x86TestExes.OrderBy(x => x))
			{
				var folder = Path.GetDirectoryName(v3x86TestExe);
				var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3x86TestExe) + "-" + Path.GetFileName(folder) + "-x86");
				await context.Exec(v3x86TestExe, $"{context.TestFlagsParallel}-xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
			}
		}

		// v2
		var v2Folder = Path.Combine(context.BaseFolder, "src", "xunit.v2.tests", "bin", context.ConfigurationText, "net452");
		var v2OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v2.tests-netfx");
		await context.Exec(context.ConsoleRunnerExe, $"{v2Folder}/xunit.v2.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v2OutputFileName}.xml\" -html \"{v2OutputFileName}.html\"", workingDirectory: v2Folder);

		if (!context.NeedMono)
			await context.Exec(context.ConsoleRunner32Exe, $"{v2Folder}/xunit.v2.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v2OutputFileName}-x86.xml\" -html \"{v2OutputFileName}-x86.html\"", workingDirectory: v2Folder);

		// v1
		var v1Folder = Path.Combine(context.BaseFolder, "src", "xunit.v1.tests", "bin", context.ConfigurationText, "net45");
		var v1OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v1.tests-netfx");
		await context.Exec(context.ConsoleRunnerExe, $"{v1Folder}/xunit.v1.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v1OutputFileName}.xml\" -html \"{v1OutputFileName}.html\"", workingDirectory: v1Folder);

		if (!context.NeedMono)
			await context.Exec(context.ConsoleRunner32Exe, $"{v1Folder}/xunit.v1.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v1OutputFileName}-x86.xml\" -html \"{v1OutputFileName}-x86.html\"", workingDirectory: v1Folder);
	}
}
