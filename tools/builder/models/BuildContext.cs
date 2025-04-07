using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using McMaster.Extensions.CommandLineUtils;

namespace Xunit.BuildTools.Models;

public partial class BuildContext
{
	string? consoleRunnerExe;
	string? consoleRunner32Exe;
	string? docFXOutputFolder;
	string? dotnet32Path;
	bool dotnet32SdkInstalled;
	string? testFlagsNonParallel;
	string? testFlagsParallel;
	string? testFlagsParallelMTP;

	// Calculated properties

	public string ConsoleRunnerExe
	{
		get => consoleRunnerExe ?? throw new InvalidOperationException($"Tried to retrieve unset {nameof(BuildContext)}.{nameof(ConsoleRunnerExe)}");
		private set => consoleRunnerExe = value ?? throw new ArgumentNullException(nameof(ConsoleRunnerExe));
	}

	public string ConsoleRunner32Exe
	{
		get => consoleRunner32Exe ?? throw new InvalidOperationException($"Tried to retrieve unset {nameof(BuildContext)}.{nameof(ConsoleRunner32Exe)}");
		private set => consoleRunner32Exe = value ?? throw new ArgumentNullException(nameof(ConsoleRunner32Exe));
	}

	public string DocFXOutputFolder
	{
		get => docFXOutputFolder ?? throw new InvalidOperationException($"Tried to retrieve unset {nameof(BuildContext)}.{nameof(DocFXOutputFolder)}");
		private set => docFXOutputFolder = value ?? throw new ArgumentNullException(nameof(DocFXOutputFolder));
	}

	public string TestFlagsNonParallel
	{
		get => testFlagsNonParallel ?? throw new InvalidOperationException($"Tried to retrieve unset {nameof(BuildContext)}.{nameof(TestFlagsNonParallel)}");
		private set => testFlagsNonParallel = value ?? throw new ArgumentNullException(nameof(TestFlagsNonParallel));
	}

	public string TestFlagsParallel
	{
		get => testFlagsParallel ?? throw new InvalidOperationException($"Tried to retrieve unset {nameof(BuildContext)}.{nameof(TestFlagsParallel)}");
		private set => testFlagsParallel = value ?? throw new ArgumentNullException(nameof(TestFlagsParallel));
	}

	public string TestFlagsParallelMTP
	{
		get => testFlagsParallelMTP ?? throw new InvalidOperationException($"Tried to retrieve unset {nameof(BuildContext)}.{nameof(TestFlagsParallelMTP)}");
		private set => testFlagsParallelMTP = value ?? throw new ArgumentNullException(nameof(TestFlagsParallelMTP));
	}

	// User-controllable command-line options

	[Option("--no-x86", Description = "Do not try to run x86 tests")]
	public bool NoX86 { get; }

	[Option("-3|--v3only", Description = "Only run tests for v3 projects (skip tests for v1 and v2)")]
	public bool V3Only { get; }

	public string? GetDotnetX86Path(bool requireSdk)
	{
		if (!requireSdk || dotnet32SdkInstalled)
			return dotnet32Path;

		return null;
	}

	public partial IReadOnlyList<string> GetSkippedAnalysisFolders() =>
		["artifacts", "src/xunit.v3.templates/templates"];

	partial void Initialize()
	{
		ConsoleRunnerExe = Path.Combine(BaseFolder, "src", "xunit.v3.runner.console", "bin", ConfigurationText, "net472", "merged", "xunit.v3.runner.console.exe");
		ConsoleRunner32Exe = Path.Combine(BaseFolder, "src", "xunit.v3.runner.console.x86", "bin", ConfigurationText, "net472", "merged", "xunit.v3.runner.console.x86.exe");

		TestFlagsNonParallel = "-parallel none ";
		TestFlagsParallel = "";
		TestFlagsParallelMTP = "";

		DocFXOutputFolder = Path.Combine(ArtifactsFolder, "docfx");

		// Run parallelizable tests with a single thread in CI to help catch Task-related deadlocks
		if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CI")))
		{
			TestFlagsParallel = "-maxthreads 1 ";
			TestFlagsParallelMTP = "--max-threads 1 ";
		}

		// Get the path to the 32-bit dotnet.exe, for Windows only
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			var programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
			if (programFilesX86 is not null)
			{
				var x86Dotnet = Path.Combine(programFilesX86, "dotnet", "dotnet.exe");
				if (File.Exists(x86Dotnet))
				{
					dotnet32Path = x86Dotnet;

					var dotnetProcessInfo = new ProcessStartInfo(x86Dotnet, "sdk check") { RedirectStandardOutput = true, RedirectStandardError = true };
					var dotnetProcess = Process.Start(dotnetProcessInfo);
					if (dotnetProcess is not null)
					{
						dotnetProcess.WaitForExit();

						dotnet32SdkInstalled = dotnetProcess.ExitCode == 0;
					}
				}
			}
		}
	}
}
