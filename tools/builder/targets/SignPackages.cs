using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(nameof(SignPackages),
        nameof(Packages))]
public static class SignPackages
{
    public static async Task OnExecute(BuildContext context)
    {
        var signClientUser = Environment.GetEnvironmentVariable("SignClientUser");
        var signClientSecret = Environment.GetEnvironmentVariable("SignClientSecret");
        if (signClientUser == null || signClientSecret == null)
        {
            context.WriteLineColor(ConsoleColor.Yellow, $"Skipping packing signing because environment variables 'SignClientUser' and/or 'SignClientSecret' are not set.{Environment.NewLine}");
            return;
        }

        var signClientFolder = Path.Combine(context.BaseFolder, "packages", $"SignClient.{context.SignClientVersion}");
        if (!Directory.Exists(signClientFolder))
        {
            context.BuildStep($"Downloading SignClient {context.SignClientVersion}");

            await context.Exec(context.NuGetExe, $"install SignClient -version {context.SignClientVersion} -SolutionDir \"{context.BaseFolder}\" -Verbosity quiet -NonInteractive");
        }

        context.BuildStep("Signing NuGet packages");

        var appPath = Path.Combine(signClientFolder, "tools", "netcoreapp2.0", "SignClient.dll");
        var packageFiles = Directory.GetFiles(context.PackageOutputFolder, "*.nupkg", SearchOption.AllDirectories)
                                    .OrderBy(x => x)
                                    .Select(x => x.Substring(context.BaseFolder.Length + 1));

        var signClientAppSettings = Path.Combine(context.BaseFolder, "tools", "SignClient", "appsettings.json");
        foreach (var packageFile in packageFiles)
        {
            var args = $"\"{appPath}\" sign -c \"{signClientAppSettings}\" -r \"{signClientUser}\" -s \"{signClientSecret}\" -n \"xUnit.net\" -d \"xUnit.net\" -u \"https://github.com/xunit/xunit\" -i \"{packageFile}\"";
            var redactedArgs = args.Replace(signClientUser, "[redacted]")
                                   .Replace(signClientSecret, "[redacted]");

            await context.Exec("dotnet", args, redactedArgs);
        }
    }
}
