using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(
    BuildTarget.TestFx,
    BuildTarget.Build
)]
public static class TestFx
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Running .NET Framework tests");

        var netFxSubpath = Path.Combine("bin", context.ConfigurationText, "net4");

        IEnumerable<string> FindTests(string pattern) =>
            Directory.GetFiles(context.BaseFolder, pattern, SearchOption.AllDirectories)
                .Where(x => x.Contains(netFxSubpath))
                .OrderBy(x => x);

        if (context.NeedMono)
        {
            context.WriteLineColor(ConsoleColor.Yellow, $"Skipping xUnit.net v1 tests on non-Windows machines.");
            Console.WriteLine();
        }
        else
        {
            foreach (var v1TestDll in FindTests("xunit.v1.tests.dll"))
            {
                var folder = Path.GetDirectoryName(v1TestDll);
                var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v1TestDll));

                await context.Exec(context.ConsoleRunnerExe, $"\"{v1TestDll}\" -appdomains denied {context.TestFlagsNonParallel} -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
            }
        }

        foreach (var v2TestDll in FindTests("xunit.v2.tests.dll"))
        {
            var folder = Path.GetDirectoryName(v2TestDll);
            var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v2TestDll));

            await context.Exec(context.ConsoleRunnerExe, $"\"{v2TestDll}\" -appdomains denied {context.TestFlagsParallel} -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
        }

        foreach (var v3TestExe in FindTests("xunit.v3.*.tests.exe"))
        {
            var folder = Path.GetDirectoryName(v3TestExe);
            var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3TestExe) + "-" + Path.GetFileName(folder));

            await context.Exec(v3TestExe, $"{context.TestFlagsParallel} -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
        }
    }
}
