using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(
    BuildTarget.Packages,
    BuildTarget.Build, BuildTarget.Build32
)]
public static class Packages
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Creating NuGet packages");

        var srcFolder = Path.Join(context.BaseFolder, "src");
        var projectFolders =
            Directory.GetFiles(srcFolder, "xunit.v3.*.csproj", SearchOption.AllDirectories)
                .Where(x => !x.EndsWith(".tests.csproj"))
                .OrderBy(x => x)
                .Select(x => Path.GetDirectoryName(x).Substring(context.BaseFolder.Length + 1));

        foreach (var projectFolder in projectFolders)
        {
            var packArgs = $"pack --nologo --no-build --configuration {context.ConfigurationText} --output {context.PackageOutputFolder} --verbosity {context.Verbosity} {projectFolder}";

            // Pack the project
            await context.Exec("dotnet", packArgs);

            // Pack any .nuspec files that might be here as well
            var nuspecFiles =
                Directory.GetFiles(projectFolder, "*.nuspec")
                    .OrderBy(x => x)
                    .Select(x => Path.GetFileName(x));

            foreach (var nuspecFile in nuspecFiles)
                await context.Exec("dotnet", $"{packArgs} -p:NuspecFile={nuspecFile}");
        }
    }
}
