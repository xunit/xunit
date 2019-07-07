using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(nameof(TestCore),
        nameof(Build))]
public static class TestCore
{
    public static Task OnExecute(BuildContext context)
    {
        context.BuildStep("Running .NET Core tests");

        // var netCoreSubpath = Path.Combine("bin", context.ConfigurationText, "netcoreapp");
        // var testDlls = Directory.GetFiles(context.BaseFolder, "test.xunit.*.dll", SearchOption.AllDirectories)
        //                         .Where(x => x.Contains(netCoreSubpath))
        //                         .OrderBy(x => x)
        //                         .Select(x => x.Substring(context.BaseFolder.Length + 1));

        context.WriteLineColor(ConsoleColor.Yellow, ".NET Core tests are not running yet.");

        Console.WriteLine();
        return Task.CompletedTask;
    }
}
