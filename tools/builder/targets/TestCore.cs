using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
    BuildTarget.TestCore,
    BuildTarget.Build
)]
public static class TestCore
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Running .NET Core 2.0 tests");
        await RunTests(context, "netcoreapp2.0", "-netcore");

        context.BuildStep("Running .NET 6 tests");
        await RunTests(context, "net6.0", "-net6");
    }

    static Task RunTests(
        BuildContext context,
        string targetFramework,
        string reportSuffix)
    {
        var runner = Path.Combine(context.BaseFolder, "src", "xunit.console", "bin", context.ConfigurationText, targetFramework, "xunit.console.dll");
        var netFxSubpath = Path.Combine("bin", context.ConfigurationText, targetFramework);
        var v2TestDlls =
            Directory
                .GetFiles(context.BaseFolder, "test.xunit.*.dll", SearchOption.AllDirectories)
                .Where(x => x.Contains(netFxSubpath))
                .OrderBy(x => x)
                .Select(x => '"' + x + '"');
        var v2OutputFileName = Path.Combine(context.TestOutputFolder, $"v2{reportSuffix}");

        return context.Exec("dotnet", $"\"{runner}\" {string.Join(' ', v2TestDlls)} {context.TestFlagsNonParallel} -xml \"{v2OutputFileName}.xml\" -html \"{v2OutputFileName}.html\"");
    }
}
