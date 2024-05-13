using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.Serve,
	BuildTarget.Restore
)]
public static class Serve
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Building web site");

		// Don't want it to throw on a non-zero exit code, since we assume Ctrl+C will generate an exit code, and this process
		// is intended to live forever until the user pressed Ctrl+C
		await context.ExecBundle($"exec jekyll serve -s {context.SiteSourceFolder} -d {context.SiteDestFolder}", throwOnNonZeroExitCode: false);
	}
}
