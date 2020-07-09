using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(
    BuildTarget.TestFxV3,
    BuildTarget.Build
)]
public static class TestFxV3
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Running v3 .NET Framework tests");

        var netFxSubpath = Path.Combine("bin", context.ConfigurationText, "net4");
        var v3TestExes =
            Directory.GetFiles(context.BaseFolder, "xunit.v3.*.tests.exe", SearchOption.AllDirectories)
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
