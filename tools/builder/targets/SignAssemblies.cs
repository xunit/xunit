using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

public static partial class SignAssemblies
{
	public static Task OnExecute(BuildContext context)
	{
		// Check early because we don't need to make copies or show the banner for non-signed scenarios
		if (!context.CanSign)
			return Task.CompletedTask;

		context.BuildStep("Signing binaries");

		// Note that any changes to .nuspec files means this list needs to be updated, and nuspec files should
		// always reference the original signed paths, and not dependency copies (i.e., xunit.v3.common.dll)
		var binaries =
			new[] {
				Path.Combine(context.BaseFolder, "src", "xunit.v3.assert",                "bin", context.ConfigurationText, "netstandard2.0",           "xunit.v3.assert.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.assert",                "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.assert.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.common",                "bin", context.ConfigurationText, "netstandard2.0",           "xunit.v3.common.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.core",                  "bin", context.ConfigurationText, "netstandard2.0",           "xunit.v3.core.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.msbuildtasks",          "bin", context.ConfigurationText, "net472",                   "xunit.v3.msbuildtasks.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.msbuildtasks",          "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.msbuildtasks.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.common",         "bin", context.ConfigurationText, "netstandard2.0", "merged", "xunit.v3.runner.common.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.console",        "bin", context.ConfigurationText, "net472",         "merged", "xunit.v3.runner.console.exe"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.console",        "bin", context.ConfigurationText, "net48",          "merged", "xunit.v3.runner.console.exe"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.console",        "bin", context.ConfigurationText, "net481",         "merged", "xunit.v3.runner.console.exe"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.console.x86",    "bin", context.ConfigurationText, "net472",         "merged", "xunit.v3.runner.console.x86.exe"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.console.x86",    "bin", context.ConfigurationText, "net48",          "merged", "xunit.v3.runner.console.x86.exe"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.console.x86",    "bin", context.ConfigurationText, "net481",         "merged", "xunit.v3.runner.console.x86.exe"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.inproc.console", "bin", context.ConfigurationText, "net472",                   "xunit.v3.runner.inproc.console.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.inproc.console", "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.runner.inproc.console.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.msbuild",        "bin", context.ConfigurationText, "net472",         "merged", "xunit.v3.runner.msbuild.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.msbuild",        "bin", context.ConfigurationText, "net8.0",         "merged", "xunit.v3.runner.msbuild.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.utility",        "bin", context.ConfigurationText, "net472",                   "xunit.v3.runner.utility.netfx.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.utility",        "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.runner.utility.netcore.dll"),
			}.Select(unsignedPath =>
			{
				var unsignedFolder = Path.GetDirectoryName(unsignedPath) ?? throw new InvalidOperationException($"Path '{unsignedPath}' did not have a folder");
				var signedFolder = Path.Combine(unsignedFolder, "signed");
				Directory.CreateDirectory(signedFolder);

				var signedPath = Path.Combine(signedFolder, Path.GetFileName(unsignedPath));
				File.Copy(unsignedPath, signedPath, overwrite: true);

				return signedPath;
			}).ToArray();

		return context.SignFiles(context.BaseFolder, binaries);
	}
}
