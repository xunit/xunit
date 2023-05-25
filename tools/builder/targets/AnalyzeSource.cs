using System;
using System.Threading.Tasks;
using SimpleExec;

[Target(
	BuildTarget.AnalyzeSource,
	BuildTarget.Restore
)]
public static class AnalyzeSource
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Analyzing source (if this fails, run './build FormatSource' to fix)");

		var foundBOM = false;

		foreach (var (file, _) in context.FindFilesWithBOMs())
		{
			if (!foundBOM)
			{
				Console.WriteLine("  One of more files were found with UTF-8 byte order marks:");
				foundBOM = true;
			}

			Console.WriteLine("    - {0}", file[(context.BaseFolder.Length + 1)..]);
		}

		if (foundBOM)
			throw new ExitCodeException(-1);

		await context.Exec("dotnet", $"dotnet-format --check --folder --verbosity {context.Verbosity} --exclude src/xunit.v3.assert/Asserts --exclude tools/NuGetKeyVaultSignTool");
	}
}
