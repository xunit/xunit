using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

public static partial class SignAssemblies
{
	static readonly string[] PublishRuntimes = ["win-arm64", "linux-x64", "linux-arm", "linux-arm64", "osx-x64", "osx-arm64"];

	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep($"Building stand-alone xunit.v3.runner.console");

		// Publish all the platform-specific binaries so they can be signed
		foreach (var rid in PublishRuntimes)
			await context.Exec("dotnet", $"publish src/xunit.v3.runner.console --verbosity {context.Verbosity} --configuration {context.ConfigurationText} --framework net10.0 --runtime {rid}");

		// Check early because we don't need to make copies or show the banner for non-signed scenarios
		if (!context.CanSign)
			return;

		context.BuildStep("Signing binaries");

		// Note that any changes to .nuspec files means this list needs to be updated, and nuspec files should
		// always reference the original signed paths, and not dependency copies (i.e., xunit.v3.common.dll)
		var binaries =
			new List<string> {
				Path.Combine(context.BaseFolder, "src", "xunit.v3.assert",                       "bin", context.ConfigurationText, "netstandard2.0",           "xunit.v3.assert.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.assert",                       "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.assert.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.assert.aot",                   "bin", context.ConfigurationText, "net9.0",                   "xunit.v3.assert.aot.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.common",                       "bin", context.ConfigurationText, "netstandard2.0",           "xunit.v3.common.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.common.aot",                   "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.common.aot.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.core",                         "bin", context.ConfigurationText, "netstandard2.0",           "xunit.v3.core.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.core.aot",                     "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.core.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.core.aot.generators",          "bin", context.ConfigurationText, "netstandard2.0",           "xunit.v3.core.aot.generators.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.msbuildtasks",                 "bin", context.ConfigurationText, "net472",                   "xunit.v3.msbuildtasks.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.msbuildtasks",                 "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.msbuildtasks.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.mtp-v1",                       "bin", context.ConfigurationText, "net472",                   "xunit.v3.mtp-v1.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.mtp-v1",                       "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.mtp-v1.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.mtp-v2",                       "bin", context.ConfigurationText, "net472",                   "xunit.v3.mtp-v2.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.mtp-v2",                       "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.mtp-v2.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.mtp-v2.aot",                   "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.mtp-v2.aot.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.common",                "bin", context.ConfigurationText, "netstandard2.0",           "xunit.v3.runner.common.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.common",                "bin", context.ConfigurationText, "netstandard2.0", "merged", "xunit.v3.runner.common.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.common.aot",            "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.runner.common.aot.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.common.aot.generators", "bin", context.ConfigurationText, "netstandard2.0",           "xunit.v3.runner.common.aot.generators.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.console",               "bin", context.ConfigurationText, "net472",         "merged", "xunit.v3.runner.console.exe"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.console",               "bin", context.ConfigurationText, "net48",          "merged", "xunit.v3.runner.console.exe"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.console",               "bin", context.ConfigurationText, "net481",         "merged", "xunit.v3.runner.console.exe"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.console.x86",           "bin", context.ConfigurationText, "net472",         "merged", "xunit.v3.runner.console.x86.exe"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.console.x86",           "bin", context.ConfigurationText, "net48",          "merged", "xunit.v3.runner.console.x86.exe"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.console.x86",           "bin", context.ConfigurationText, "net481",         "merged", "xunit.v3.runner.console.x86.exe"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.inproc.console",        "bin", context.ConfigurationText, "net472",                   "xunit.v3.runner.inproc.console.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.inproc.console",        "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.runner.inproc.console.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.inproc.console.aot",    "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.runner.inproc.console.aot.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.msbuild",               "bin", context.ConfigurationText, "net472",         "merged", "xunit.v3.runner.msbuild.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.msbuild",               "bin", context.ConfigurationText, "net8.0",         "merged", "xunit.v3.runner.msbuild.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.utility",               "bin", context.ConfigurationText, "net472",                   "xunit.v3.runner.utility.netfx.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.utility",               "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.runner.utility.netcore.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.utility.aot",           "bin", context.ConfigurationText, "net8.0",                   "xunit.v3.runner.utility.aot.dll"),
			};

		foreach (var runtime in PublishRuntimes.Where(rid => rid.StartsWith("win-")))
			binaries.Add(Path.Combine(context.BaseFolder, "src", "xunit.v3.runner.console", "bin", context.ConfigurationText, "net10.0", "publish", runtime, "xunit.v3.runner.console.exe"));

		var binariesToSign =
			binaries.Select(unsignedPath =>
 			{
 				var unsignedFolder = Path.GetDirectoryName(unsignedPath) ?? throw new InvalidOperationException($"Path '{unsignedPath}' did not have a folder");
 				var signedFolder = Path.Combine(unsignedFolder, "signed");
 				Directory.CreateDirectory(signedFolder);

 				var signedPath = Path.Combine(signedFolder, Path.GetFileName(unsignedPath));
 				File.Copy(unsignedPath, signedPath, overwrite: true);

 				return signedPath;
 			}).ToArray();

		await context.SignFiles(context.BaseFolder, binariesToSign);
	}
}
