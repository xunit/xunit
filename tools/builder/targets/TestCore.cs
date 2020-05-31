using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(BuildTarget.TestCore,
        BuildTarget.Build)]
public static class TestCore
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Running .NET Core tests");

        var netCoreSubpath = Path.Combine("bin", context.ConfigurationText, "netcoreapp");
        var testDlls = Directory.GetFiles(context.BaseFolder, "xunit.v3.*.tests.dll", SearchOption.AllDirectories)
                                .Where(x => x.Contains(netCoreSubpath))
                                .OrderBy(x => x)
                                .Select(x => x.Substring(context.BaseFolder.Length + 1));

        foreach (var testDll in testDlls)
        {
            var fileName = Path.GetFileName(testDll);
            var folder = Path.GetDirectoryName(testDll);
            var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(testDll) + "-" + Path.GetFileName(folder));

            await context.Exec("dotnet", $"exec {fileName} {context.TestFlagsParallel} -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
        }
    }
}
