using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(
	BuildTarget.TestFx32V3,
	BuildTarget.Build32
)]
public static class TestFx32V3
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Running v3 .NET Framework (32-bit) tests");

		if (context.NeedMono)
		{
			context.WriteLineColor(ConsoleColor.Yellow, $"Skipping 32-bit tests on non-Windows machines.");
			Console.WriteLine();
			return;
		}

		var netFx32Subpath = Path.Combine("bin", context.ConfigurationText + "_x86", "net4");
		var v3TestExes = Directory.GetFiles(context.BaseFolder, "xunit.v3.*.tests.exe", SearchOption.AllDirectories)
			.Where(x => x.Contains(netFx32Subpath))
			.OrderBy(x => x);

		foreach (var v3TestExe in v3TestExes)
		{
			var folder = Path.GetDirectoryName(v3TestExe);
			var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3TestExe) + "-" + Path.GetFileName(folder) + "-x86");

			await context.Exec(v3TestExe, $"{context.TestFlagsParallel} -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
		}
	}
}
