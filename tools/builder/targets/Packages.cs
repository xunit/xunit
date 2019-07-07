using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(nameof(Packages),
        nameof(Build), nameof(DownloadNuGet))]
public static class Packages
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Creating NuGet packages");

        var nuspecFiles = Directory.GetFiles(context.BaseFolder, "*.nuspec", SearchOption.AllDirectories)
                                   .OrderBy(x => x)
                                   .Select(x => x.Substring(context.BaseFolder.Length + 1));

        foreach (var nuspecFile in nuspecFiles)
            await context.Exec(context.NuGetExe, $"pack {nuspecFile} -NonInteractive -NoPackageAnalysis -OutputDirectory {context.PackageOutputFolder} -Properties Configuration={context.ConfigurationText}");
    }
}
