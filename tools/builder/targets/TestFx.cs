using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
    BuildTarget.TestFx,
    BuildTarget.Build
)]
public static class TestFx
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Running .NET Framework tests (AnyCPU)");
        await RunTests(context, context.ConsoleRunnerExe, "-x64");

        context.BuildStep("Running .NET Framework tests (x86)");
        await RunTests(context, context.ConsoleRunner32Exe, "-x86");
    }

    static async Task RunTests(
        BuildContext context,
        string runner,
        string reportSuffix)
    {
        // v2
        var netFxSubpath = Path.Combine("bin", context.ConfigurationText, "net4");
        var v2TestDlls =
            Directory
                .GetFiles(context.BaseFolder, "test.xunit.*.dll", SearchOption.AllDirectories)
                .Where(x => x.Contains(netFxSubpath))
                .OrderBy(x => x)
                .Select(x => '"' + x + '"');

        foreach (var v2TestDll in v2TestDlls)
        {
            var outputFileName = Path.Combine(context.TestOutputFolder, $"{Path.GetFileNameWithoutExtension(v2TestDll)}-netfx{reportSuffix}");
            await context.Exec(runner, $"{v2TestDll} {context.TestFlagsParallel} -serialize -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html");
        }

        // v1
        var v1TestDll = Path.Combine(context.BaseFolder, "test", "test.xunit1", "bin", context.ConfigurationText, "net40", "test.xunit1.dll");
        var v1OutputFileName = Path.Combine(context.TestOutputFolder, $"test.xunit1-netfx{reportSuffix}");

        await context.Exec(runner, $"\"{v1TestDll}\" {context.TestFlagsNonParallel} -xml \"{v1OutputFileName}.xml\" -html \"{v1OutputFileName}.html");
    }
}
