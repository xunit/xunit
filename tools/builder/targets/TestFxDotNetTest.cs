using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestFxDotNetTest,
	BuildTarget.Build
)]
public static class TestFxDotNetTest
{
	static readonly string refSubPath = Path.DirectorySeparatorChar + "ref" + Path.DirectorySeparatorChar;

	public static async Task OnExecute(BuildContext context)
	{
		// ------------- AnyCPU -------------

		context.BuildStep("Running .NET Framework tests (AnyCPU, via 'dotnet test')");

		await RunTestAssemblies(context, x86: false);

		// ------------- Forced x86 -------------

		// Mono is only supported at whatever bitness the user installs for Mono
		if (context.NeedMono)
			return;

		context.BuildStep("Running .NET Framework tests (x86, via Console runner)");

		await RunTestAssemblies(context, x86: true);
	}

	static async Task RunTestAssemblies(
		BuildContext context,
		bool x86)
	{
		var binSubPath = Path.Combine("bin", context.ConfigurationText, "net4");
		var testAssemblies =
			Directory
				.GetFiles(context.BaseFolder, "xunit.v3.*.tests.exe", SearchOption.AllDirectories)
				.Where(x => x.Contains(binSubPath) && !x.Contains(refSubPath) && (x.Contains(".x86") == x86))
				.OrderBy(x => x);

		foreach (var testAssembly in testAssemblies)
		{
			var projectFolder = testAssembly.Substring(0, testAssembly.IndexOf(binSubPath) - 1);
			var assemblyFolder = Path.GetDirectoryName(testAssembly)!;
			var testResultsFolder = Path.Combine(assemblyFolder, "TestResults")!;
			var framework = Path.GetFileName(assemblyFolder);

			if (Directory.Exists(testResultsFolder))
				Directory.Delete(testResultsFolder, recursive: true);

			try
			{
				await context.Exec("dotnet", $"test --no-build --framework {framework} --configuration {context.ConfigurationText} {projectFolder}");
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
