using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(BuildTarget.Packages,
        BuildTarget.Build)]
public static class Packages
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Creating NuGet packages");

        var srcFolder = Path.Join(context.BaseFolder, "src");
        var projectFolders = Directory.GetFiles(srcFolder, "xunit.v3.*.csproj", SearchOption.AllDirectories)
                                      .Where(x => !x.EndsWith(".tests.csproj"))
                                      .OrderBy(x => x)
                                      .Select(x => Path.GetDirectoryName(x).Substring(context.BaseFolder.Length + 1));

        foreach (var projectFolder in projectFolders)
            await context.Exec("dotnet", $"pack --nologo --no-build --configuration {context.ConfigurationText} --output {context.PackageOutputFolder} {projectFolder}");
    }
}
