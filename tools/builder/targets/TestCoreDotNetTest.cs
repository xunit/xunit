using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestCoreDotNetTest,
	BuildTarget.Build
)]
public static class TestCoreDotNetTest
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

		context.BuildStep($"Running .NET tests ({framework}, AnyCPU, via 'dotnet test')");

		await RunTestAssemblies(context, "dotnet", framework, x86: false);

		// ------------- Forced x86 -------------

		// On non-Windows machines, dotnet only runs on default architecture and bitness
		if (context.NeedMono)
			return;

		// Only run 32-bit .NET Core tests if 32-bit .NET Core is installed
		var programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
		if (programFilesX86 == null)
			return;

		var x86Dotnet = Path.Combine(programFilesX86, "dotnet", "dotnet.exe");
		if (!File.Exists(x86Dotnet))
			return;

		context.BuildStep($"Running .NET tests ({framework}, x86, via 'dotnet test')");

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
			var projectFolder = testAssembly.Substring(0, testAssembly.IndexOf(binSubPath) - 1);
			var assemblyFolder = Path.GetDirectoryName(testAssembly)!;
			var testResultsFolder = Path.Combine(assemblyFolder, "TestResults")!;

			if (Directory.Exists(testResultsFolder))
				Directory.Delete(testResultsFolder, recursive: true);

			try
			{
				await context.Exec(command, $"test --no-build --framework {framework} --configuration {context.ConfigurationText} {projectFolder}");
			}
			catch
			{
				if (Directory.Exists(testResultsFolder))
				{
					var file = Directory.GetFiles(testResultsFolder, "*.log").FirstOrDefault();
					if (file is not null)
					{
						Console.Error.WriteLine();
						Console.Error.Write(File.ReadAllText(file));
					}
				}

				throw;
			}
		}
	}
}
