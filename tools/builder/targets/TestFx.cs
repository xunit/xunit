using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(BuildTarget.TestFx,
        BuildTarget.Build)]
public static class TestFx
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Running .NET Framework tests");

        var netFxSubpath = Path.Combine("bin", context.ConfigurationText, "net4");

        // Validate that we can run v1 tests

        var v1TestDlls = Directory.GetFiles(context.BaseFolder, "xunit.v1.tests.dll", SearchOption.AllDirectories)
                                  .Where(x => x.Contains(netFxSubpath))
                                  .OrderBy(x => x);

        foreach (var v1TestDll in v1TestDlls)
        {
            var folder = Path.GetDirectoryName(v1TestDll);
            var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v1TestDll));

            await context.Exec(context.ConsoleRunnerExe, $"\"{v1TestDll}\" {context.TestFlagsNonParallel} -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
        }

        // TODO: Need to port over some v2 tests

        // Run v3 tests

        var v3TestExes = Directory.GetFiles(context.BaseFolder, "xunit.v3.*.tests.exe", SearchOption.AllDirectories)
                                  .Where(x => x.Contains(netFxSubpath))
                                  .OrderBy(x => x);

        foreach (var v3TestExe in v3TestExes)
        {
            var folder = Path.GetDirectoryName(v3TestExe);
            var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3TestExe) + "-" + Path.GetFileName(folder));

            await context.Exec(v3TestExe, $"{context.TestFlagsParallel} -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
        }
    }
}
