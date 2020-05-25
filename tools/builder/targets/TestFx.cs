using System;
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
        // TODO: Need to re-enable v1 tests, and port over some v2 tests
        var testDlls = Directory.GetFiles(context.BaseFolder, "xunit.v3.*.tests.exe", SearchOption.AllDirectories)
                                .Where(x => x.Contains(netFxSubpath))
                                .OrderBy(x => x)
                                .Select(x => x.Substring(context.BaseFolder.Length + 1));

        foreach (var testDll in testDlls)
        {
            var fileName = Path.GetFileName(testDll);
            var folder = Path.GetDirectoryName(testDll);

            // TODO: XML output?
            await context.Exec(fileName, "", workingDirectory: folder);
        }
    }
}
