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

		Directory.CreateDirectory(context.TestOutputFolder);

		// v3 (default bitness)
		var netFxSubpath = Path.Combine("bin", context.ConfigurationText, "net4");
		var v3OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v3.tests-netfx");
		var v3TestExes =
			Directory
				.GetFiles(context.BaseFolder, "xunit.v3.*.tests.exe", SearchOption.AllDirectories)
				.Where(x => x.Contains(netFxSubpath));

#if false
		// TODO: When we officially move to console runner, combine x86 and AnyCPU binaries into a single run (and output file)
		await context.Exec(context.ConsoleRunnerExe, $"\"{string.Join("\" \"", v3TestExes)}\" {context.TestFlagsParallel}-preenumeratetheories -xml \"{v3OutputFileName}.xml\" -html \"{v3OutputFileName}.html\"");
#else
		foreach (var v3TestExe in v3TestExes.OrderBy(x => x))
		{
			var folder = Path.GetDirectoryName(v3TestExe);
			var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3TestExe) + "-" + Path.GetFileName(folder));
			await context.Exec(v3TestExe, $"{context.TestFlagsParallel}-preenumeratetheories -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
		}
#endif

		// Mono is only supported for v3, at whatever bitness the user installs for Mono
		if (context.NeedMono)
			return;

		// v3 (forced 32-bit)
		var netFx32Subpath = Path.Combine("bin", context.ConfigurationText + "_x86", "net4");
		var v3x86TestExes =
			Directory
				.GetFiles(context.BaseFolder, "xunit.v3.*.tests.x86.exe", SearchOption.AllDirectories)
				.Where(x => x.Contains(netFx32Subpath));

		if (context.V3Only)
			return;

#if false
		await context.Exec(context.ConsoleRunnerExe, $"\"{string.Join("\" \"", v3x86TestExes)}\" {context.TestFlagsParallel}-preenumeratetheories -xml \"{v3OutputFileName}-x86.xml\" -html \"{v3OutputFileName}-x86.html\"");
#else
		foreach (var v3x86TestExe in v3x86TestExes.OrderBy(x => x))
		{
			var folder = Path.GetDirectoryName(v3x86TestExe);
			var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3x86TestExe) + "-" + Path.GetFileName(folder) + "-x86");
			await context.Exec(v3x86TestExe, $"{context.TestFlagsParallel}-preenumeratetheories -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
		}
#endif

		// v2
		var v2Folder = Path.Combine(context.BaseFolder, "src", "xunit.v2.tests", "bin", context.ConfigurationText, "net452");
		var v2OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v2.tests-netfx");
		await context.Exec(context.ConsoleRunnerExe, $"{v2Folder}/xunit.v2.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v2OutputFileName}.xml\" -html \"{v2OutputFileName}.html\"", workingDirectory: v2Folder);
		await context.Exec(context.ConsoleRunner32Exe, $"{v2Folder}/xunit.v2.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v2OutputFileName}-x86.xml\" -html \"{v2OutputFileName}-x86.html\"", workingDirectory: v2Folder);

		// v1
		var v1Folder = Path.Combine(context.BaseFolder, "src", "xunit.v1.tests", "bin", context.ConfigurationText, "net45");
		var v1OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v1.tests-netfx");
		await context.Exec(context.ConsoleRunnerExe, $"{v1Folder}/xunit.v1.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v1OutputFileName}.xml\" -html \"{v1OutputFileName}.html\"", workingDirectory: v1Folder);
		await context.Exec(context.ConsoleRunner32Exe, $"{v1Folder}/xunit.v1.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v1OutputFileName}-x86.xml\" -html \"{v1OutputFileName}-x86.html\"", workingDirectory: v1Folder);
	}
}
