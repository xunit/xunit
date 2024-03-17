using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

public static partial class SignAssemblies
{
    public static async Task OnExecute(BuildContext context)
    {
        // We need to run the dotnet publish before we sign anything, since we use the published console runner binaries
        context.BuildStep("Publishing projects for packaging");

        foreach (var targetFramework in new[] { "netcoreapp1.0", "netcoreapp2.0", "net6.0" })
            await context.Exec("msbuild", @$"src\xunit.console\xunit.console.csproj /p:TargetFramework={targetFramework} /p:Configuration={context.ConfigurationText} /t:publish /v:minimal /m");

        // Check early because we don't need to make copies or show the banner for non-signed scenarios
        if (!context.CanSign)
            return;

        context.BuildStep("Signing binaries");

        // Note that any changes to .nuspec files means this list needs to be updated, and nuspec files should
        // always reference the original signed paths, and not dependency copies (i.e., xunit.runner.utility.*.dll)
        var binaries =
            new[] {
                Path.Combine(context.BaseFolder, "src", "xunit.assert",           "bin", context.ConfigurationText, "netstandard1.1",            "xunit.assert.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.assert",           "bin", context.ConfigurationText, "net6.0",                    "xunit.assert.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.console",          "bin", context.ConfigurationText, "net452",                    "xunit.console.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console",          "bin", context.ConfigurationText, "net46",                     "xunit.console.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console",          "bin", context.ConfigurationText, "net461",                    "xunit.console.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console",          "bin", context.ConfigurationText, "net462",                    "xunit.console.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console",          "bin", context.ConfigurationText, "net47",                     "xunit.console.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console",          "bin", context.ConfigurationText, "net471",                    "xunit.console.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console",          "bin", context.ConfigurationText, "net472",                    "xunit.console.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console",          "bin", context.ConfigurationText, "net48",                     "xunit.console.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console",          "bin", context.ConfigurationText, "net481",                    "xunit.console.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console.x86",      "bin", context.ConfigurationText, "net452",                    "xunit.console.x86.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console.x86",      "bin", context.ConfigurationText, "net46",                     "xunit.console.x86.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console.x86",      "bin", context.ConfigurationText, "net461",                    "xunit.console.x86.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console.x86",      "bin", context.ConfigurationText, "net462",                    "xunit.console.x86.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console.x86",      "bin", context.ConfigurationText, "net47",                     "xunit.console.x86.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console.x86",      "bin", context.ConfigurationText, "net471",                    "xunit.console.x86.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console.x86",      "bin", context.ConfigurationText, "net472",                    "xunit.console.x86.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console.x86",      "bin", context.ConfigurationText, "net48",                     "xunit.console.x86.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console.x86",      "bin", context.ConfigurationText, "net481",                    "xunit.console.x86.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.console",          "bin", context.ConfigurationText, "netcoreapp1.0",  "publish", "xunit.console.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.console",          "bin", context.ConfigurationText, "netcoreapp2.0",  "publish", "xunit.console.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.console",          "bin", context.ConfigurationText, "net6.0",         "publish", "xunit.console.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.console",          "bin", context.ConfigurationText, "net6.0",         "publish", "xunit.console.exe"),
                Path.Combine(context.BaseFolder, "src", "xunit.core",             "bin", context.ConfigurationText, "netstandard1.1",            "xunit.core.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.core",             "bin", context.ConfigurationText, "netstandard1.1",            "xunit.core.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.execution",        "bin", context.ConfigurationText, "net452",                    "xunit.execution.desktop.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.execution",        "bin", context.ConfigurationText, "netstandard1.1",            "xunit.execution.dotnet.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.runner.msbuild",   "bin", context.ConfigurationText, "net452",                    "xunit.runner.msbuild.net452.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.runner.reporters", "bin", context.ConfigurationText, "net452",                    "xunit.runner.reporters.net452.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.runner.reporters", "bin", context.ConfigurationText, "netcoreapp1.0",             "xunit.runner.reporters.netcoreapp10.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.runner.reporters", "bin", context.ConfigurationText, "netstandard1.1",            "xunit.runner.reporters.netstandard11.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.runner.reporters", "bin", context.ConfigurationText, "netstandard1.5",            "xunit.runner.reporters.netstandard15.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.runner.tdnet",     "bin", context.ConfigurationText, "net452",                    "xunit.runner.tdnet.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.runner.utility",   "bin", context.ConfigurationText, "net35",                     "xunit.runner.utility.net35.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.runner.utility",   "bin", context.ConfigurationText, "net452",                    "xunit.runner.utility.net452.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.runner.utility",   "bin", context.ConfigurationText, "netcoreapp1.0",             "xunit.runner.utility.netcoreapp10.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.runner.utility",   "bin", context.ConfigurationText, "netstandard1.1",            "xunit.runner.utility.netstandard11.dll"),
                Path.Combine(context.BaseFolder, "src", "xunit.runner.utility",   "bin", context.ConfigurationText, "netstandard1.5",            "xunit.runner.utility.netstandard15.dll"),
            }.Select(unsignedPath =>
            {
                var unsignedFolder = Path.GetDirectoryName(unsignedPath) ?? throw new InvalidOperationException($"Path '{unsignedPath}' did not have a folder");
                var signedFolder = Path.Combine(unsignedFolder, "signed");
                Directory.CreateDirectory(signedFolder);

                var signedPath = Path.Combine(signedFolder, Path.GetFileName(unsignedPath));
                File.Copy(unsignedPath, signedPath, overwrite: true);

                return signedPath;
            }).ToArray();

        await context.SignFiles(context.BaseFolder, binaries);
    }
}
