using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestMTP,
	BuildTarget.Build
)]
public static class TestMTP
{
	static readonly string refSubPath = Path.DirectorySeparatorChar + "ref" + Path.DirectorySeparatorChar;

	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep($"Running tests [via dotnet test]");

		var x86DotNet = context.GetDotnetX86Path(requireSdk: true);
		var noNetCoreX86 = x86DotNet is null;
		var noNetFrameworkX86 = context.NeedMono;

		var testAssemblies = new List<(string assembly, bool x86, string framework)>();

		if (context.TestFramework != Framework.NetFx)
		{
			testAssemblies.AddRange(FindTestAssemblies(context, "xunit.v3.*.tests.dll", "net8.0", x86: false));
			if (!context.NoX86 && !noNetCoreX86)
				testAssemblies.AddRange(FindTestAssemblies(context, "xunit.v3.*.tests.dll", "net8.0", x86: true));
		}

		if (context.TestFramework != Framework.Net)
		{
			testAssemblies.AddRange(FindTestAssemblies(context, "xunit.v3.*.tests.exe", "net472", x86: false));
			if (!context.NoX86 && !noNetCoreX86 && !noNetFrameworkX86)
				testAssemblies.AddRange(FindTestAssemblies(context, "xunit.v3.*.tests.exe", "net472", x86: true));
		}

		foreach (var (testAssembly, x86, framework) in testAssemblies)
		{
			// Go up three directories, for 'bin/{configuration}/{framework}'
			var projectFolder = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(testAssembly))));

			try
			{
				await context.Exec(
					x86 ? x86DotNet! : "dotnet",
					$"test --directory {projectFolder} --configuration {context.ConfigurationText} --framework {framework} --no-build",
					workingDirectory: context.BaseFolder
				);
			}
			catch
			{
				var logFile = Path.Join(
					context.BaseFolder,
					projectFolder,
					"bin",
					context.ConfigurationText,
					framework,
					"TestResults",
					Path.GetFileNameWithoutExtension(testAssembly) + "_" + framework + "_" + (x86 ? "x86" : "x64") + ".log"
				);

				if (File.Exists(logFile))
				{
					Console.WriteLine();
					context.WriteLineColor(ConsoleColor.Red, "=== Log file contents ===");
					Console.WriteLine();

					foreach (var line in File.ReadAllLines(logFile))
						context.WriteLineColor(ConsoleColor.DarkGray, "  " + line);
				}

				throw;
			}
		}

		// Clean out all the 'dotnet test' log files, because if we got this far everything succeeded
		foreach (var logFile in Directory.GetFiles(context.TestOutputFolder, "*.log"))
			File.Delete(logFile);
	}

	static IEnumerable<(string assembly, bool x86, string framework)> FindTestAssemblies(
		BuildContext context,
		string pattern,
		string framework,
		bool x86)
	{
		var binSubPath = Path.Combine("bin", context.ConfigurationText, framework);

		return
			Directory
				.GetFiles(context.BaseFolder, pattern, SearchOption.AllDirectories)
				.Where(x => x.Contains(binSubPath) && !x.Contains(refSubPath) && (x.Contains(".x86") == x86))
				.OrderBy(x => x)
				.Select(x => (x.Substring(context.BaseFolder.Length + 1), x86, framework));
	}
}
