using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(nameof(PushMyGet),
        nameof(DownloadNuGet))]
public static class PushMyGet
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Pushing packages to MyGet");

        var myGetApiKey = Environment.GetEnvironmentVariable("MyGetApiKey");
        if (myGetApiKey == null)
        {
            context.WriteLineColor(ConsoleColor.Yellow, $"Skipping MyGet push because environment variable 'MyGetApiKey' is not set.{Environment.NewLine}");
            return;
        }

        var packageFiles = Directory.GetFiles(context.PackageOutputFolder, "*.nupkg", SearchOption.AllDirectories)
                                    .OrderBy(x => x)
                                    .Select(x => x.Substring(context.BaseFolder.Length + 1));

        foreach (var packageFile in packageFiles)
        {
            var args = $"push -source https://www.myget.org/F/xunit/api/v2/package -apiKey {myGetApiKey} {packageFile}";
            var redactedArgs = args.Replace(myGetApiKey, "[redacted]");
            await context.Exec(context.NuGetExe, args, redactedArgs);
        }
    }
}
