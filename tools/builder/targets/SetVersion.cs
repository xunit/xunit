using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(nameof(SetVersion))]
public static class SetVersion
{
    public static async Task OnExecute(BuildContext context)
    {
        if (context.BuildAssemblyVersion != null)
        {
            context.BuildStep($"Setting assembly version: {context.BuildAssemblyVersion}");

            var filesToPatch = Directory.GetFiles(context.BaseFolder, "GlobalAssemblyInfo.cs", SearchOption.AllDirectories);
            foreach (var fileToPatch in filesToPatch)
            {
                context.WriteLineColor(ConsoleColor.DarkGray, $"PATCH: {fileToPatch}");

                var text = await File.ReadAllTextAsync(fileToPatch);
                var newText = text.Replace("99.99.99.0", context.BuildAssemblyVersion);
                if (newText != text)
                    await File.WriteAllTextAsync(fileToPatch, newText);
            }

            Console.WriteLine();
        }

        if (context.BuildSemanticVersion != null)
        {
            context.BuildStep($"Setting semantic version: {context.BuildSemanticVersion}");

            var filesToPatch = Directory.GetFiles(context.BaseFolder, "GlobalAssemblyInfo.cs", SearchOption.AllDirectories)
                       .Concat(Directory.GetFiles(context.BaseFolder, "*.nuspec", SearchOption.AllDirectories));

            foreach (var fileToPatch in filesToPatch)
            {
                context.WriteLineColor(ConsoleColor.DarkGray, $"PATCH: {fileToPatch}");

                var text = await File.ReadAllTextAsync(fileToPatch);
                var newText = text.Replace("99.99.99-dev", context.BuildSemanticVersion);
                if (newText != text)
                    await File.WriteAllTextAsync(fileToPatch, newText);
            }

            Console.WriteLine();
        }
    }
}
