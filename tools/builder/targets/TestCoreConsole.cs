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

		// v3
		// TODO: Convert to console runner when it's available
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

			await context.Exec("dotnet", $"exec {fileName} {context.TestFlagsParallel}-xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
		}
	}
}
