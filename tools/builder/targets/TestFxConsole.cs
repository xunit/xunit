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
	static readonly string refSubPath = Path.DirectorySeparatorChar + "ref" + Path.DirectorySeparatorChar;

	public static async Task OnExecute(BuildContext context)
	{
		// ------------- AnyCPU -------------

		context.BuildStep($"Running .NET Framework tests (AnyCPU, via Console runner)");

		await RunTestAssemblies(context, "xunit.v3.*.tests.exe", x86: false);

		// Mono is only supported for v3, at whatever bitness the user installs for Mono
		if (context.NeedMono)
			return;

		var v2Folder = Path.Combine(context.BaseFolder, "src", "xunit.v2.tests", "bin", context.ConfigurationText, "net452");
		var v2OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v2.tests-netfx");
		var v1Folder = Path.Combine(context.BaseFolder, "src", "xunit.v1.tests", "bin", context.ConfigurationText, "net45");
		var v1OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v1.tests-netfx");

		if (!context.V3Only)
		{
			await context.Exec(context.ConsoleRunnerExe, $"{v2Folder}/xunit.v2.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v2OutputFileName}.xml\" -html \"{v2OutputFileName}.html\" -trx \"{v2OutputFileName}.trx\"", workingDirectory: v2Folder);
			await context.Exec(context.ConsoleRunnerExe, $"{v1Folder}/xunit.v1.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v1OutputFileName}.xml\" -html \"{v1OutputFileName}.html\" -trx \"{v1OutputFileName}.trx\"", workingDirectory: v1Folder);
		}

		// ------------- Forced x86 -------------

		context.BuildStep($"Running .NET Framework tests (x86, via Console runner)");

		await RunTestAssemblies(context, "xunit.v3.*.tests.exe", x86: true);

		if (!context.V3Only)
		{
			await context.Exec(context.ConsoleRunner32Exe, $"{v2Folder}/xunit.v2.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v2OutputFileName}-x86.xml\" -html \"{v2OutputFileName}-x86.html\" -trx \"{v2OutputFileName}-x86.trx\"", workingDirectory: v2Folder);
			await context.Exec(context.ConsoleRunner32Exe, $"{v1Folder}/xunit.v1.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v1OutputFileName}-x86.xml\" -html \"{v1OutputFileName}-x86.html\" -trx \"{v1OutputFileName}-x86.trx\"", workingDirectory: v1Folder);
		}
	}

	static async Task RunTestAssemblies(
		BuildContext context,
		string searchPattern,
		bool x86)
	{
		var binSubPath = Path.Combine("bin", context.ConfigurationText, "net4");
		var testAssemblies =
			Directory
				.GetFiles(context.BaseFolder, searchPattern, SearchOption.AllDirectories)
				.Where(x => x.Contains(binSubPath) && !x.Contains(refSubPath) && (x.Contains(".x86") == x86))
				.OrderBy(x => x)
				.Select(x => x.Substring(context.BaseFolder.Length + 1));

		foreach (var testAssembly in testAssemblies)
		{
			var fileName = Path.GetFileName(testAssembly);
			var folder = Path.GetDirectoryName(testAssembly);
			var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(testAssembly) + "-" + Path.GetFileName(folder));

			await context.Exec(fileName, $"{context.TestFlagsParallel}-preenumeratetheories -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\" -trx \"{outputFileName}.trx\"", workingDirectory: folder);
		}
	}
}
