using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(
    BuildTarget.TestCore,
    BuildTarget.Build
)]
public static class TestCore
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Running .NET Core tests");

        var netCoreSubpath = Path.Combine("bin", context.ConfigurationText, "netcoreapp");

        IEnumerable<string> FindTests(string pattern) =>
            Directory.GetFiles(context.BaseFolder, pattern, SearchOption.AllDirectories)
                .Where(x => x.Contains(netCoreSubpath))
                .OrderBy(x => x)
                .Select(x => x.Substring(context.BaseFolder.Length + 1));

        await context.Exec("dotnet", $"test --no-build --nologo --configuration {context.ConfigurationText} --framework netcoreapp2.1 --verbosity quiet", workingDirectory: Path.Combine(context.BaseFolder, "src", "xunit.v2.tests"));

        foreach (var v3TestDll in FindTests("xunit.v3.*.tests.dll"))
        {
            var fileName = Path.GetFileName(v3TestDll);
            var folder = Path.GetDirectoryName(v3TestDll);
            var outputFileName = Path.Combine(context.TestOutputFolder, Path.GetFileNameWithoutExtension(v3TestDll) + "-" + Path.GetFileName(folder));

            await context.Exec("dotnet", $"exec {fileName} {context.TestFlagsParallel} -xml \"{outputFileName}.xml\" -html \"{outputFileName}.html\"", workingDirectory: folder);
        }
    }
}
