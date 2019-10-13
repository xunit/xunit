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

#if false
        var nuspecFiles = Directory.GetFiles(context.BaseFolder, "*.nuspec", SearchOption.AllDirectories)
                                   .OrderBy(x => x)
                                   .Select(x => x.Substring(context.BaseFolder.Length + 1));

        foreach (var nuspecFile in nuspecFiles)
            await context.Exec(context.NuGetExe, $"pack {nuspecFile} -NonInteractive -NoPackageAnalysis -OutputDirectory {context.PackageOutputFolder} -Properties Configuration={context.ConfigurationText}");
#else
        var srcFolder = Path.Join(context.BaseFolder, "src");
        var projectFolders = Directory.GetFiles(srcFolder, "xunit.v3.*.csproj", SearchOption.AllDirectories)
                                      .OrderBy(x => x)
                                      .Select(x => Path.GetDirectoryName(x).Substring(context.BaseFolder.Length + 1));

        foreach (var projectFolder in projectFolders)
            await context.Exec("dotnet", $"pack --nologo --no-build --configuration {context.ConfigurationText} --output {context.PackageOutputFolder} {projectFolder}");
#endif
    }
}
