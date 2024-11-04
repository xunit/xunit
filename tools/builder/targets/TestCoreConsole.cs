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

		await RunTestAssemblies(context, framework, x86: false);

		// ------------- Forced x86 -------------

		if (context.NoX86)
			return;

		var x86Dotnet = context.GetDotnetX86Path(requireSdk: false);
		if (x86Dotnet is null)
			return;

		context.BuildStep($"Running .NET tests ({framework}, x86, via Console runner)");

		await RunTestAssemblies(context, framework, x86: true);
	}

	static async Task RunTestAssemblies(
		BuildContext context,
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

		var outputFileName = Path.Combine(context.TestOutputFolder, $"xunit.v3-{framework}-{(x86 ? "x86" : "AnyCPU")}");

		await context.Exec(context.ConsoleRunnerExe, $"{string.Join(" ", testAssemblies)} {context.TestFlagsParallel}-preEnumerateTheories -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\" -ctrf \"{outputFileName}.ctrf\" -trx \"{outputFileName}.trx\"", workingDirectory: context.BaseFolder);
	}
}
