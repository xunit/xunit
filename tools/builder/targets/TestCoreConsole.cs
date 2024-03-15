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
	static readonly string refSubPath = Path.DirectorySeparatorChar + "ref" + Path.DirectorySeparatorChar;

	public static async Task OnExecute(BuildContext context)
	{
		await RunTests(context, "net6.0");
		await RunTests(context, "net8.0");
	}

	static async Task RunTests(
		BuildContext context,
		string framework)
	{
		// ------------- AnyCPU -------------

		context.BuildStep($"Running .NET tests ({framework}, AnyCPU, via Console runner)");

		await RunTestAssemblies(context, "dotnet", framework, x86: false);

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

		context.BuildStep($"Running .NET tests ({framework}, x86, via Console runner)");

		await RunTestAssemblies(context, x86Dotnet, framework, x86: true);
	}

	static async Task RunTestAssemblies(
		BuildContext context,
		string command,
		string framework,
		bool x86)
	{
		var binSubPath = Path.Combine("bin", context.ConfigurationText, framework);
		var testAssemblies =
			Directory
				.GetFiles(context.BaseFolder, "xunit.v3.*.tests.dll", SearchOption.AllDirectories)
				.Where(x => x.Contains(binSubPath) && !x.Contains(refSubPath) && (x.Contains(".x86") == x86))
				.OrderBy(x => x)
				.Select(x => x.Substring(context.BaseFolder.Length + 1));

		foreach (var testAssembly in testAssemblies)
		{
			var fileName = Path.GetFileName(testAssembly);
			var folder = Path.GetDirectoryName(testAssembly);
			var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(testAssembly) + "-" + Path.GetFileName(folder));

			await context.Exec(command, $"exec {fileName} {context.TestFlagsParallel}-preenumeratetheories -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\" -trx \"{outputFileName}.trx\"", workingDirectory: folder);
		}
	}
}
