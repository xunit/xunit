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

		Directory.CreateDirectory(context.PackageOutputFolder);

		var signClientUser = Environment.GetEnvironmentVariable("SignClientUser");
		var signClientSecret = Environment.GetEnvironmentVariable("SignClientSecret");
		if (string.IsNullOrWhiteSpace(signClientUser) || string.IsNullOrWhiteSpace(signClientSecret))
		{
			context.WriteLineColor(ConsoleColor.Yellow, $"Skipping packing signing because environment variables 'SignClientUser' and/or 'SignClientSecret' are not set.{Environment.NewLine}");
			return;
		}

		var signClientAppSettings = Path.Combine(context.BaseFolder, "tools", "SignClient", "appsettings.json");
		var args = $"SignClient sign --config \"{signClientAppSettings}\" --user \"{signClientUser}\" --secret \"{signClientSecret}\" --name \"xUnit.net\" --description \"xUnit.net\" -u \"https://github.com/xunit/xunit\" --baseDirectory \"{context.PackageOutputFolder}\" --input **/*.nupkg";
		var redactedArgs =
			args.Replace(signClientUser, "[redacted]")
				.Replace(signClientSecret, "[redacted]");

		await context.Exec("dotnet", args, redactedArgs);
	}
}
