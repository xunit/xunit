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
		var v2OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v2-net452");
		var v1Folder = Path.Combine(context.BaseFolder, "src", "xunit.v1.tests", "bin", context.ConfigurationText, "net45");
		var v1OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v1-net45");

		if (!context.V3Only)
		{
			await context.Exec(context.ConsoleRunnerExe, $"{v2Folder}/xunit.v2.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v2OutputFileName}-AnyCPU.xml\" -html \"{v2OutputFileName}-AnyCPU.html\" -ctrf \"{v2OutputFileName}-AnyCPU.ctrf\" -trx \"{v2OutputFileName}-AnyCPU.trx\"", workingDirectory: v2Folder);
			await context.Exec(context.ConsoleRunnerExe, $"{v1Folder}/xunit.v1.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v1OutputFileName}-AnyCPU.xml\" -html \"{v1OutputFileName}-AnyCPU.html\" -ctrf \"{v1OutputFileName}-AnyCPU.ctrf\" -trx \"{v1OutputFileName}-AnyCPU.trx\"", workingDirectory: v1Folder);
		}

		// ------------- Forced x86 -------------

		if (context.NoX86)
			return;

		context.BuildStep($"Running .NET Framework tests (x86, via Console runner)");

		await RunTestAssemblies(context, "xunit.v3.*.tests.exe", x86: true);

		if (!context.V3Only)
		{
			await context.Exec(context.ConsoleRunner32Exe, $"{v2Folder}/xunit.v2.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v2OutputFileName}-x86.xml\" -html \"{v2OutputFileName}-x86.html\" -ctrf \"{v2OutputFileName}-x86.ctrf\" -trx \"{v2OutputFileName}-x86.trx\"", workingDirectory: v2Folder);
			await context.Exec(context.ConsoleRunner32Exe, $"{v1Folder}/xunit.v1.tests.dll {context.TestFlagsParallel}-appdomains required -xml \"{v1OutputFileName}-x86.xml\" -html \"{v1OutputFileName}-x86.html\" -ctrf \"{v1OutputFileName}-x86.ctrf\" -trx \"{v1OutputFileName}-x86.trx\"", workingDirectory: v1Folder);
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
				.OrderBy(x => x);

		var outputFileName = Path.Combine(context.TestOutputFolder, $"xunit.v3-net472-{(x86 ? "x86" : "AnyCPU")}");

		await context.Exec(context.ConsoleRunnerExe, $"{string.Join(" ", testAssemblies)} {context.TestFlagsParallel}-preEnumerateTheories -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\" -ctrf \"{outputFileName}.ctrf\" -trx \"{outputFileName}.trx\"", workingDirectory: context.BaseFolder);
	}
}
