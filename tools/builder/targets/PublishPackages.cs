using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(
	BuildTarget.PublishPackages,
	BuildTarget.Packages
)]
public static class PublishPackages
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Publishing NuGet packages");

		var publishToken = Environment.GetEnvironmentVariable("PublishToken");
		if (string.IsNullOrWhiteSpace(publishToken))
		{
			context.WriteLineColor(ConsoleColor.Yellow, $"Skipping package publishing because environment variable 'PublishToken' is not set.{Environment.NewLine}");
			return;
		}

		var packageFiles =
			Directory.GetFiles(context.PackageOutputFolder, "*.nupkg", SearchOption.AllDirectories)
				.OrderBy(x => x)
				.Select(x => x.Substring(context.BaseFolder.Length + 1));

		foreach (var packageFile in packageFiles)
		{
			var args = $"nuget push --source https://www.myget.org/F/xunit/api/v2/package --api-key {publishToken} {packageFile}";
			var redactedArgs = args.Replace(publishToken, "[redacted]");
			await context.Exec("dotnet", args, redactedArgs);
		}
	}
}
