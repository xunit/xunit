using System.IO;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.DocFX,
	BuildTarget.RestoreTools
)]
public static class DocFX
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Creating API metadata for DocFX (reflection)");

		if (Directory.Exists(context.DocFXOutputFolder))
			Directory.Delete(context.DocFXOutputFolder, true);

		Directory.CreateDirectory(context.DocFXOutputFolder);

		await context.Exec("dotnet", "docfx ./docfx/docfx.json");

		context.BuildStep("Creating API metadata for DocFX (Native AOT)");

		if (Directory.Exists(context.DocFXOutputFolderAot))
			Directory.Delete(context.DocFXOutputFolderAot, true);

		Directory.CreateDirectory(context.DocFXOutputFolderAot);

		await context.Exec("dotnet", "docfx ./docfx/docfx-aot.json");
	}
}
