using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestFxConsole,
	BuildTarget.Build
)]
public static class TestFxConsole
{
	public static async Task OnExecute(BuildContext context)
	{
		var refPath = Path.DirectorySeparatorChar + "ref" + Path.DirectorySeparatorChar;

		var v2Folder = Path.Combine(context.BaseFolder, "src", "xunit.v2.tests", "bin", context.ConfigurationText, "net452");
		var v2OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v2.tests-netfx");
		var v1Folder = Path.Combine(context.BaseFolder, "src", "xunit.v1.tests", "bin", context.ConfigurationText, "net45");
		var v1OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v1.tests-netfx");

		// ------------- AnyCPU -------------

		context.BuildStep("Running .NET Framework tests (AnyCPU, via Console runner)");

		// v3
		var netFxSubpath = Path.Combine("bin", context.ConfigurationText, "net4");
		var v3TestExes =
			Directory
				.GetFiles(context.BaseFolder, "xunit.v3.*.tests.exe", SearchOption.AllDirectories)
				.Where(x => x.Contains(netFxSubpath) && !x.Contains(refPath));

		// TODO: When we officially move to console runner, combine x86 and AnyCPU binaries into a single run (and output file)
#if false
		var v3OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v3.tests-netfx");

		await context.Exec(context.ConsoleRunnerExe, $"\"{string.Join("\" \"", v3TestExes)}\" {context.TestFlagsParallel}-preenumeratetheories -xml \"{v3OutputFileName}.xml\" -html \"{v3OutputFileName}.html\" -trx \"{v3OutputFileName}.trx\"");
#else
		foreach (var v3TestExe in v3TestExes.OrderBy(x => x))
		{
			var folder = Path.GetDirectoryName(v3TestExe);
			var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3TestExe) + "-" + Path.GetFileName(folder));
			await context.Exec(v3TestExe, $"{context.TestFlagsParallel}-preenumeratetheories -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\" -trx \"{outputFileName}.trx\"", workingDirectory: folder);
		}
#endif

		// Mono is only supported for v3, at whatever bitness the user installs for Mono
		if (context.NeedMono)
			return;

		if (!context.V3Only)
		{
			await context.Exec(context.ConsoleRunnerExe, $"{v2Folder}/xunit.v2.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v2OutputFileName}.xml\" -html \"{v2OutputFileName}.html\" -trx \"{v2OutputFileName}.trx\"", workingDirectory: v2Folder);
			await context.Exec(context.ConsoleRunnerExe, $"{v1Folder}/xunit.v1.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v1OutputFileName}.xml\" -html \"{v1OutputFileName}.html\" -trx \"{v1OutputFileName}.trx\"", workingDirectory: v1Folder);
		}

		// ------------- Forced x86 -------------

		context.BuildStep("Running .NET Framework tests (x86, via Console runner)");

		// v3 (forced 32-bit)
		var netFx32Subpath = Path.Combine("bin", context.ConfigurationText + "_x86", "net4");
		var v3x86TestExes =
			Directory
				.GetFiles(context.BaseFolder, "xunit.v3.*.tests.x86.exe", SearchOption.AllDirectories)
				.Where(x => x.Contains(netFx32Subpath) && !x.Contains(refPath));

#if false
		await context.Exec(context.ConsoleRunnerExe, $"\"{string.Join("\" \"", v3x86TestExes)}\" {context.TestFlagsParallel}-preenumeratetheories -xml \"{v3OutputFileName}-x86.xml\" -html \"{v3OutputFileName}-x86.html\" -trx \"{v3OutputFileName}-x86.trx\"");
#else
		foreach (var v3x86TestExe in v3x86TestExes.OrderBy(x => x))
		{
			var folder = Path.GetDirectoryName(v3x86TestExe);
			var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3x86TestExe) + "-" + Path.GetFileName(folder));
			await context.Exec(v3x86TestExe, $"{context.TestFlagsParallel}-preenumeratetheories -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\" -trx \"{outputFileName}.trx\"", workingDirectory: folder);
		}
#endif

		if (!context.V3Only)
		{
			await context.Exec(context.ConsoleRunner32Exe, $"{v2Folder}/xunit.v2.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v2OutputFileName}-x86.xml\" -html \"{v2OutputFileName}-x86.html\" -trx \"{v2OutputFileName}-x86.trx\"", workingDirectory: v2Folder);
			await context.Exec(context.ConsoleRunner32Exe, $"{v1Folder}/xunit.v1.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v1OutputFileName}-x86.xml\" -html \"{v1OutputFileName}-x86.html\" -trx \"{v1OutputFileName}-x86.trx\"", workingDirectory: v1Folder);
		}
	}
}
