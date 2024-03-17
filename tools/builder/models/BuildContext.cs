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

    public partial IReadOnlyList<string> GetSkippedAnalysisFolders() =>
        new[]
        {
            "src/common/AssemblyResolution/Microsoft.DotNet.PlatformAbstractions",
            "src/common/AssemblyResolution/Microsoft.Extensions.DependencyModel",
        };

    partial void Initialize()
    {
        ConsoleRunnerExe = Path.Combine(BaseFolder, "src", "xunit.console", "bin", ConfigurationText, "net462", "xunit.console.exe");
        ConsoleRunner32Exe = Path.Combine(BaseFolder, "src", "xunit.console.x86", "bin", ConfigurationText, "net462", "xunit.console.x86.exe");

        TestFlagsNonParallel = "-parallel collections";
        TestFlagsParallel = "-parallel all";

        // Run parallelizable tests with a single thread in CI to help catch Task-related deadlocks
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CI")))
        {
            TestFlagsNonParallel = "-parallel none -maxthreads 1";
            TestFlagsParallel = "-parallel none -maxthreads 1";
        }
    }
}
