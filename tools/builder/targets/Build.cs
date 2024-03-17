using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

public static partial class Build
{
    public static partial async Task PerformBuild(BuildContext context)
    {
        context.BuildStep("Compiling binaries");

        await context.Exec("msbuild", $"xunit.sln /t:build /p:Configuration={context.ConfigurationText} /v:{context.Verbosity} /m");
    }
}
