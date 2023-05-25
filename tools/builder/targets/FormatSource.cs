using System;
using System.IO;
using System.Threading.Tasks;

[Target(
	BuildTarget.FormatSource,
	BuildTarget.Restore
)]
public static class FormatSource
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Formatting source");

		var foundBOM = false;

		foreach (var (file, bytes) in context.FindFilesWithBOMs())
		{
			if (!foundBOM)
			{
				Console.WriteLine("  Removed UTF-8 byte order mark:");
				foundBOM = true;
			}

			Console.WriteLine("    - {0}", file[(context.BaseFolder.Length + 1)..]);
			File.WriteAllBytes(file, bytes.AsSpan().Slice(3).ToArray());
		}

		if (foundBOM)
			Console.WriteLine();

		await context.Exec("dotnet", $"dotnet-format --folder --exclude src/xunit.v3.assert/Asserts --exclude tools/NuGetKeyVaultSignTool");
	}
}
