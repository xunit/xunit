using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.Build,
	BuildTarget.Restore
)]
public static class Build
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Building web site");

		await context.ExecBundle($"exec jekyll build -s {context.SiteSourceFolder} -d {context.SiteDestFolder}");
	}
}
