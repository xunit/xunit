using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(
	BuildTarget.TestCoreV3,
	BuildTarget.Build
)]
public static class TestCore_V3
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Running v3 .NET Core tests");

		var netCoreSubpath = Path.Combine("bin", context.ConfigurationText, "netcoreapp");
		var v3TestDlls = Directory.GetFiles(context.BaseFolder, "xunit.v3.*.tests.dll", SearchOption.AllDirectories)
			.Where(x => x.Contains(netCoreSubpath))
			.OrderBy(x => x)
			.Select(x => x.Substring(context.BaseFolder.Length + 1));

		foreach (var v3TestDll in v3TestDlls)
		{
			var fileName = Path.GetFileName(v3TestDll);
			var folder = Path.GetDirectoryName(v3TestDll);
			var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3TestDll) + "-" + Path.GetFileName(folder));

			await context.Exec("dotnet", $"exec {fileName} {context.TestFlagsNonParallel}-xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
		}
	}
}
