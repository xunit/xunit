using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.PublishAOT,
	BuildTarget.Restore
)]
public static class PublishAOT
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep($"Publishing Native AOT tests");

		var aotTestProjects =
			Directory
				.GetFiles(context.BaseFolder, "*.aot.tests.csproj", SearchOption.AllDirectories)
				.Where(project => !project.Contains(".acceptance."))
				.OrderBy(x => x);

		foreach (var aotTestProject in aotTestProjects)
		{
			var buildLog = Path.Combine(context.BuildArtifactsFolder, $"publish-{Path.GetFileNameWithoutExtension(aotTestProject)}.binlog");

			await context.Exec("dotnet", $@"publish ""{aotTestProject}"" --configuration {context.ConfigurationText} -binaryLogger:""{buildLog}""");
		}
	}
}
