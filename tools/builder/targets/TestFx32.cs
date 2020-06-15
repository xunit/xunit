using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(
    BuildTarget.TestFx32,
    BuildTarget.Build32
)]
public static class TestFx32
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Running .NET Framework (32-bit) tests");

        if (context.NeedMono)
        {
            context.WriteLineColor(ConsoleColor.Yellow, $"Skipping 32-bit tests on non-Windows machines.");
            return;
        }

        var v1Folder = Path.Combine(context.BaseFolder, "src", "xunit.v1.tests", "bin", context.ConfigurationText, "net45");
        var v1OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v1.tests-net45-x86");
        await context.Exec(context.ConsoleRunner32Exe, $"{v1Folder}/xunit.v1.tests.dll -appdomains denied {context.TestFlagsNonParallel} -xml \"{v1OutputFileName}.xml\" -html \"{v1OutputFileName}.html\"", workingDirectory: v1Folder);

        var v2Folder = Path.Combine(context.BaseFolder, "src", "xunit.v2.tests", "bin", context.ConfigurationText, "net452");
        var v2OutputFileName = Path.Combine(context.TestOutputFolder, "xunit.v2.tests-net452-x86");
        await context.Exec(context.ConsoleRunner32Exe, $"{v2Folder}/xunit.v2.tests.dll -appdomains denied {context.TestFlagsParallel} -xml \"{v2OutputFileName}.xml\" -html \"{v2OutputFileName}.html\"", workingDirectory: v2Folder);

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
