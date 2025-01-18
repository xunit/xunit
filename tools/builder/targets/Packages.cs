using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

public static partial class Packages
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Creating NuGet packages");

		// Clean up any existing packages to force re-packing
		var packageFiles = Directory.GetFiles(context.PackageOutputFolder, "*.nupkg");
		foreach (var packageFile in packageFiles)
			File.Delete(packageFile);

		// Enumerate the .nuspec files and pack those
		var srcFolder = Path.Join(context.BaseFolder, "src");
		var nuspecFiles =
			Directory
				.GetFiles(srcFolder, "*.nuspec", SearchOption.AllDirectories)
				.ToList();

		// You can't see the created package name in .NET 9+ SDK without doing detailed verbosity
		var extraArgs =
			context.DotNetSdkVersion.Major <= 8
				? string.Empty
				: " --tl:off";

		// Pack the .nuspec file(s)
		foreach (var nuspecFile in nuspecFiles.OrderBy(x => x))
			await context.Exec("dotnet", $"pack --nologo --no-build --configuration {context.ConfigurationText} --output {context.PackageOutputFolder} --verbosity {context.Verbosity} \"{Path.GetDirectoryName(nuspecFile)}\" -p:NuspecFile={Path.GetFileName(nuspecFile)}{extraArgs}");
	}
}
