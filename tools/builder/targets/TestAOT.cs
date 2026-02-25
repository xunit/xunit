using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestAOT,
	BuildTarget.Build, BuildTarget.PublishAOT
)]
public static class TestAOT
{
	public static async Task OnExecute(BuildContext context)
	{
		var executableExtension = context.IsWindows ? ".exe" : string.Empty;

		context.BuildStep($"Running Native AOT tests [via xunit.v3.runner.console]");

		var v3TestAssemblies =
			Directory
				.GetFiles(context.BaseFolder, "xunit.v3.*.aot.tests" + executableExtension, SearchOption.AllDirectories)
				.Where(x => x.Contains("publish") && !x.Contains(".acceptance."))
				.OrderBy(x => x)
				.Select(x => x.Substring(context.BaseFolder.Length + 1));

		await context.Exec(context.ConsoleRunnerExe, $"{string.Join(" ", v3TestAssemblies)}", workingDirectory: context.BaseFolder);
	}
}
