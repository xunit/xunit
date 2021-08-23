using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(
	BuildTarget.TestCoreConsole,
	BuildTarget.Build
)]
public static class TestCoreConsole
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Running .NET Core tests (via Console runner)");

		// v3 (default bitness)
		// TODO: Convert to console runner when it's available
		var netCoreSubpath = Path.Combine("bin", context.ConfigurationText, "netcoreapp");
		var v3TestDlls =
			Directory
				.GetFiles(context.BaseFolder, "xunit.v3.*.tests.dll", SearchOption.AllDirectories)
				.Where(x => x.Contains(netCoreSubpath))
				.OrderBy(x => x)
				.Select(x => x.Substring(context.BaseFolder.Length + 1));

		foreach (var v3TestDll in v3TestDlls)
		{
			var fileName = Path.GetFileName(v3TestDll);
			var folder = Path.GetDirectoryName(v3TestDll);
			var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3TestDll) + "-" + Path.GetFileName(folder));

			await context.Exec("dotnet", $"exec {fileName} {context.TestFlagsParallel}-preenumeratetheories -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
		}

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

		// v3 (forced 32-bit)
		var netCore32Subpath = Path.Combine("bin", context.ConfigurationText + "_x86", "netcoreapp");
		var v3x86TestDlls =
			Directory
				.GetFiles(context.BaseFolder, "xunit.v3.*.tests.x86.dll", SearchOption.AllDirectories)
				.Where(x => x.Contains(netCore32Subpath))
				.OrderBy(x => x)
				.Select(x => x.Substring(context.BaseFolder.Length + 1));

		foreach (var v3x86TestDll in v3x86TestDlls)
		{
			var fileName = Path.GetFileName(v3x86TestDll);
			var folder = Path.GetDirectoryName(v3x86TestDll);
			var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3x86TestDll) + "-" + Path.GetFileName(folder));

			await context.Exec(x86Dotnet, $"exec {fileName} {context.TestFlagsParallel}-preenumeratetheories -xml \"{outputFileName}-x86.xml\" -html \"{outputFileName}-x86.html\"", workingDirectory: folder);
		}
	}
}
