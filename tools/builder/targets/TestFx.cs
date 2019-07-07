using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(nameof(TestFx),
        nameof(Build))]
public static class TestFx
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Running .NET Framework tests");

        var net472Subpath = Path.Combine("bin", context.ConfigurationText, "net472");
        var testV1Dll = Path.Combine("test", "test.xunit1", "bin", context.ConfigurationText, "net45", "test.xunit1.dll");
        var testDlls = Directory.GetFiles(context.BaseFolder, "test.xunit.*.dll", SearchOption.AllDirectories)
                                .Where(x => x.Contains(net472Subpath))
                                .Select(x => x.Substring(context.BaseFolder.Length + 1));

        var xunitConsoleExe = Path.Combine("src", "xunit.console", "bin", context.ConfigurationText, "net472", "xunit.console.exe");

        await context.Exec(xunitConsoleExe, $"{testV1Dll} -xml artifacts/test/v1.xml -html artifacts/test/v1.html -appdomains denied {context.TestFlagsNonParallel}");
        await context.Exec(xunitConsoleExe, $"{string.Join(" ", testDlls)} -xml artifacts/test/v2.xml -html artifacts/test/v2.html -appdomains denied -serialize {context.TestFlagsParallel}");

        Console.WriteLine();
    }
}
