using System.IO;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

public static partial class Build
{
	public static partial async Task PerformBuild(BuildContext context)
	{
		context.BuildStep("Compiling binaries");

		var buildLog = Path.Combine(context.BuildLogOutputFolder, "build.binlog");

		await context.Exec("dotnet", $"msbuild -nologo -maxCpuCount -restore:False -verbosity:{context.Verbosity} -p:Configuration={context.ConfigurationText} -bl:{buildLog}");
	}
}
