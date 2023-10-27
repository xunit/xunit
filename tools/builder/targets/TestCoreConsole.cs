using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestCoreConsole,
	BuildTarget.Build
)]
public static class TestCoreConsole
{
	public static async Task OnExecute(BuildContext context)
	{
		var refPath = Path.DirectorySeparatorChar + "ref" + Path.DirectorySeparatorChar;

		// ------------- AnyCPU -------------

		context.BuildStep("Running .NET Core tests (AnyCPU, via Console runner)");

		// v3
		var netCoreSubpath = Path.Combine("bin", context.ConfigurationText, "net6");
		var v3TestDlls =
			Directory
				.GetFiles(context.BaseFolder, "xunit.v3.*.tests.dll", SearchOption.AllDirectories)
				.Where(x => x.Contains(netCoreSubpath) && !x.Contains(refPath))
				.OrderBy(x => x)
				.Select(x => x.Substring(context.BaseFolder.Length + 1));

		// TODO: When we officially move to console runner, combine x86 and AnyCPU binaries into a single run (and output file)
#if false
		var v3OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v3.tests-netcore");

		await context.Exec(context.ConsoleRunnerExe, $"\"{string.Join("\" \"", v3TestDlls)}\" {context.TestFlagsParallel}-preenumeratetheories -xml \"{v3OutputFileName}.xml\" -html \"{v3OutputFileName}.html\" -trx \"{v3OutputFileName}.trx\"");
#else
		foreach (var v3TestDll in v3TestDlls)
		{
			var fileName = Path.GetFileName(v3TestDll);
			var folder = Path.GetDirectoryName(v3TestDll);
			var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3TestDll) + "-" + Path.GetFileName(folder));

			await context.Exec("dotnet", $"exec {fileName} {context.TestFlagsParallel}-preenumeratetheories -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\" -trx \"{outputFileName}.trx\"", workingDirectory: folder);
		}
#endif

		// ------------- Forced x86 -------------

		// Only run 32-bit .NET Core tests on Windows
		if (context.NeedMono)
			return;

		// Only run 32-bit .NET Core tests if 32-bit .NET Core is installed
		var programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
		if (programFilesX86 == null)
			return;

		var x86Dotnet = Path.Combine(programFilesX86, "dotnet", "dotnet.exe");
		if (!File.Exists(x86Dotnet))
			return;

		context.BuildStep("Running .NET Core tests (x86, via Console runner)");

		// v3 (forced 32-bit)
		var netCore32Subpath = Path.Combine("bin", context.ConfigurationText + "_x86", "net6");
		var v3x86TestDlls =
			Directory
				.GetFiles(context.BaseFolder, "xunit.v3.*.tests.x86.dll", SearchOption.AllDirectories)
				.Where(x => x.Contains(netCore32Subpath) && !x.Contains(refPath))
				.OrderBy(x => x)
				.Select(x => x.Substring(context.BaseFolder.Length + 1));

#if false
		await context.Exec(context.ConsoleRunnerExe, $"\"{string.Join("\" \"", v3x86TestDlls)}\" {context.TestFlagsParallel}-preenumeratetheories -xml \"{v3OutputFileName}-x86.xml\" -html \"{v3OutputFileName}-x86.html\" -trx \"{v3OutputFileName}-x86.trx\"");
#else
		foreach (var v3x86TestDll in v3x86TestDlls)
		{
			var fileName = Path.GetFileName(v3x86TestDll);
			var folder = Path.GetDirectoryName(v3x86TestDll);
			var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3x86TestDll) + "-" + Path.GetFileName(folder));

			await context.Exec(x86Dotnet, $"exec {fileName} {context.TestFlagsParallel}-preenumeratetheories -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\" -trx \"{outputFileName}.trx\"", workingDirectory: folder);
		}
#endif
	}
}
