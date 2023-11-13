using System;
using System.Collections.Generic;
using System.IO;
using McMaster.Extensions.CommandLineUtils;

namespace Xunit.BuildTools.Models;

public partial class BuildContext
{
	string? consoleRunnerExe;
	string? consoleRunner32Exe;
	string? testFlagsNonParallel;
	string? testFlagsParallel;

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

	// User-controllable command-line options

	[Option("-3|--v3only", Description = "Only run tests for v3 projects (skip tests for v1 and v2)")]
	public bool V3Only { get; }

	public partial IReadOnlyList<string> GetSkippedAnalysisFolders() =>
		Array.Empty<string>();

	partial void Initialize()
	{
		ConsoleRunnerExe = Path.Combine(BaseFolder, "src", "xunit.v3.runner.console", "bin", ConfigurationText, "net472", "merged", "xunit.v3.runner.console.exe");
		ConsoleRunner32Exe = Path.Combine(BaseFolder, "src", "xunit.v3.runner.console", "bin", ConfigurationText + "_x86", "net472", "merged", "xunit.v3.runner.console.x86.exe");

		TestFlagsNonParallel = "-parallel none ";
		TestFlagsParallel = "";

		// Run parallelizable tests with a single thread in CI to help catch Task-related deadlocks
		if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CI")))
			TestFlagsParallel = "-maxthreads 1 ";
	}
}
