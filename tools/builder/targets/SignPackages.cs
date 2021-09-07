using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(
	BuildTarget.SignPackages,
	BuildTarget.Packages
)]
public static class SignPackages
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Signing NuGet packages");

		var signClientUser = Environment.GetEnvironmentVariable("SignClientUser");
		var signClientSecret = Environment.GetEnvironmentVariable("SignClientSecret");
		if (string.IsNullOrWhiteSpace(signClientUser) || string.IsNullOrWhiteSpace(signClientSecret))
		{
			context.WriteLineColor(ConsoleColor.Yellow, $"Skipping packing signing because environment variables 'SignClientUser' and/or 'SignClientSecret' are not set.{Environment.NewLine}");
			return;
		}

		var packageFiles =
			Directory
				.GetFiles(context.PackageOutputFolder, "*.nupkg", SearchOption.AllDirectories)
				.Select(x => x.Substring(context.BaseFolder.Length + 1));

		var signClientAppSettings = Path.Combine(context.BaseFolder, "tools", "SignClient", "appsettings.json");
		foreach (var packageFile in packageFiles.OrderBy(x => x))
		{
			var args = $"SignClient sign -c \"{signClientAppSettings}\" -r \"{signClientUser}\" -s \"{signClientSecret}\" -n \"xUnit.net\" -d \"xUnit.net\" -u \"https://github.com/xunit/xunit\" -i \"{packageFile}\"";
			var redactedArgs =
				args.Replace(signClientUser, "[redacted]")
					.Replace(signClientSecret, "[redacted]");

			await context.Exec("dotnet", args, redactedArgs);
		}
	}
}
