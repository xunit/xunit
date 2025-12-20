using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestConsole,
	BuildTarget.Build
)]
public static class TestConsole
{
	static readonly string refSubPath = Path.DirectorySeparatorChar + "ref" + Path.DirectorySeparatorChar;

	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep($"Running tests [via xunit.v3.runner.console]");

		var noNetCoreX86 = context.GetDotnetX86Path(requireSdk: false) is null;

		// ------------- v3 -------------

		var v3TestAssemblies = new List<string>();

		if (context.TestFramework != Framework.NetFx)
		{
			v3TestAssemblies.AddRange(FindTestAssemblies(context, "xunit.v3.*.tests.dll", "net8.0", x86: false));
			if (!context.NoX86 && !noNetCoreX86)
				v3TestAssemblies.AddRange(FindTestAssemblies(context, "xunit.v3.*.tests.dll", "net8.0", x86: true));
		}

		if (context.TestFramework != Framework.Net && context.IsWindows)
		{
			v3TestAssemblies.AddRange(FindTestAssemblies(context, "xunit.v3.*.tests.exe", "net472", x86: false));
			if (!context.NoX86)
				v3TestAssemblies.AddRange(FindTestAssemblies(context, "xunit.v3.*.tests.exe", "net472", x86: true));
		}

		await RunTests(context, v3TestAssemblies, Path.Combine(context.TestOutputFolder, $"v3"));

		if (context.V3Only || !context.IsWindows)
			return;

		// ------------- v2 -------------

		var v2Folder = Path.Combine(context.BaseFolder, "src", "xunit.v2.tests", "bin", context.ConfigurationText, "net452");
		await RunTests(context, [Path.Combine(v2Folder, "xunit.v2.tests.dll")], Path.Combine(context.TestOutputFolder, "v2"), "-appdomains required");
		if (!context.NoX86)
			await RunTests(context, [Path.Combine(v2Folder, "xunit.v2.tests.dll")], Path.Combine(context.TestOutputFolder, "v2-x86"), "-appdomains required", context.ConsoleRunner32Exe);

		// ------------- v1 -------------

		var v1Folder = Path.Combine(context.BaseFolder, "src", "xunit.v1.tests", "bin", context.ConfigurationText, "net45");
		await RunTests(context, [Path.Combine(v1Folder, "xunit.v1.tests.dll")], Path.Combine(context.TestOutputFolder, "v1"), "-appdomains required");
		if (!context.NoX86)
			await RunTests(context, [Path.Combine(v1Folder, "xunit.v1.tests.dll")], Path.Combine(context.TestOutputFolder, "v1-x86"), "-appdomains required", context.ConsoleRunner32Exe);
	}

	static IEnumerable<string> FindTestAssemblies(
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
				.Select(x => x.Substring(context.BaseFolder.Length + 1));
	}

	static async Task RunTests(
		BuildContext context,
		List<string> testAssemblies,
		string outputFilePrefix,
		string extraArgs = "-preEnumerateTheories",
		string? runner = null) =>
			await context.Exec(runner ?? context.ConsoleRunnerExe, $"{string.Join(" ", testAssemblies)} {extraArgs} -xml \"{outputFilePrefix}.xml\" -html \"{outputFilePrefix}.html\" -ctrf \"{outputFilePrefix}.ctrf\" -trx \"{outputFilePrefix}.trx\"", workingDirectory: context.BaseFolder);
}
