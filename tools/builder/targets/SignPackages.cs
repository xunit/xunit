using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(
	BuildTarget.SignPackages,
	BuildTarget.RestoreTools
)]
public static class SignPackages
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Signing NuGet packages");

		Directory.CreateDirectory(context.PackageOutputFolder);

		var tenantId = Environment.GetEnvironmentVariable("SIGN_TENANT");
		var vaultUri = Environment.GetEnvironmentVariable("SIGN_VAULT_URI");
		var applicationId = Environment.GetEnvironmentVariable("SIGN_APP_ID");
		var applicationSecret = Environment.GetEnvironmentVariable("SIGN_APP_SECRET");
		var certificateName = Environment.GetEnvironmentVariable("SIGN_CERT_NAME");

		if (string.IsNullOrWhiteSpace(tenantId) ||
			string.IsNullOrWhiteSpace(vaultUri) ||
			string.IsNullOrWhiteSpace(applicationId) ||
			string.IsNullOrWhiteSpace(applicationSecret) ||
			string.IsNullOrWhiteSpace(certificateName))
		{
			context.WriteLineColor(ConsoleColor.Yellow, $"Skipping packing signing because one or more environment variables are missing: SIGN_TENANT, SIGN_VAULT_URI, SIGN_APP_ID, SIGN_APP_SECRET, SIGN_CERT_NAME{Environment.NewLine}");
			return;
		}

		var args =
			$"sign code azure-key-vault **/*.nupkg" +
			$" --base-directory \"{context.PackageOutputFolder}\"" +
			$" --description \"xUnit.net\"" +
			$" --description-url https://github.com/xunit" +
			$" --azure-key-vault-url {vaultUri}" +
			$" --azure-key-vault-client-id {applicationId}" +
			$" --azure-key-vault-client-secret \"{applicationSecret}\"" +
			$" --azure-key-vault-tenant-id {tenantId}" +
			$" --azure-key-vault-certificate {certificateName}";

		var redactedArgs =
			args.Replace(tenantId, "[redacted]")
				.Replace(vaultUri, "[redacted]")
				.Replace(applicationId, "[redacted]")
				.Replace(applicationSecret, "[redacted]")
				.Replace(certificateName, "[redacted]");

		await context.Exec("dotnet", args, redactedArgs);
	}
}
