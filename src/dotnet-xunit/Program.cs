using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

class Program
{
    static HashSet<string> HelpArgs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "-?", "/?", "-h", "--help" };
    static HashSet<string> OutputFileArgs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "-xml", "-xmlv1", "-nunit", "-html" };
    static Version Version452 = new Version("4.5.2");

    string BuildStdProps;
    string Configuration;
    bool Force32bit;
    string FxVersion;
    bool InternalDiagnostics;
    string MsBuildVerbosity;
    bool NoBuild;
    bool NoColor;
    Dictionary<string, List<string>> ParsedArgs;
    bool Quiet;
    string ThisAssemblyPath;
    bool UseMsBuild;

    string DefaultMsBuildVerbosity => Quiet ? "quiet" : "minimal";

    static int Main(string[] args)
        => new Program().Execute(args);

    int Execute(string[] args)
    {
        // Let Ctrl+C pass down into the child processes, ignoring it here
        Console.CancelKeyPress += (sender, e) => e.Cancel = true;

        try
        {
            if (args.Any(HelpArgs.Contains))
            {
                PrintUsage();
                return 2;
            }

            string requestedTargetFramework;

            try
            {
                ParsedArgs = ArgParser.Parse(args);

                if (ParsedArgs.TryGetAndRemoveParameterWithoutValue("-x86"))
                    Force32bit = true;

                if (ParsedArgs.TryGetParameterWithoutValue("-internaldiagnostics"))
                    InternalDiagnostics = true;

                if (ParsedArgs.TryGetParameterWithoutValue("-quiet"))
                    Quiet = true;

                if (ParsedArgs.TryGetParameterWithoutValue("-nocolor"))
                    NoColor = true;

                if (ParsedArgs.TryGetAndRemoveParameterWithoutValue("-usemsbuild"))
                    UseMsBuild = true;

                MsBuildVerbosity = ParsedArgs.GetAndRemoveParameterWithValue("-msbuildverbosity");

                // The extra versions are unadvertised compatibility flags to match 'dotnet' command line switches
                requestedTargetFramework = ParsedArgs.GetAndRemoveParameterWithValue("-framework")
                                        ?? ParsedArgs.GetAndRemoveParameterWithValue("--framework")
                                        ?? ParsedArgs.GetAndRemoveParameterWithValue("-f");
                Configuration = ParsedArgs.GetAndRemoveParameterWithValue("-configuration")
                             ?? ParsedArgs.GetAndRemoveParameterWithValue("--configuration")
                             ?? ParsedArgs.GetAndRemoveParameterWithValue("-c")
                             ?? "Debug";
                FxVersion = ParsedArgs.GetAndRemoveParameterWithValue("-fxversion")
                         ?? ParsedArgs.GetAndRemoveParameterWithValue("--fx-version");
                NoBuild = ParsedArgs.GetAndRemoveParameterWithoutValue("-nobuild")
                       || ParsedArgs.GetAndRemoveParameterWithoutValue("--no-build");

                // Need to amend the paths for the report output, since we are always running
                // in the context of the bin folder, not the project folder
                var currentDirectory = Directory.GetCurrentDirectory();
                foreach (var key in OutputFileArgs)
                    if (ParsedArgs.TryGetSingleValue(key, out var fileName))
                        ParsedArgs[key][0] = Path.GetFullPath(Path.Combine(currentDirectory, fileName));
            }
            catch (ArgumentException ex)
            {
                WriteLineError(ex.Message);
                return 3;
            }

            var testProjects = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.*proj")
                                        .Where(f => !f.EndsWith(".xproj"))
                                        .ToList();

            if (testProjects.Count == 0)
            {
                WriteLineError("Could not find any project (*.*proj) file in the current directory.");
                return 3;
            }

            if (testProjects.Count > 1)
            {
                WriteLineError($"Multiple project files were found; only a single project file is supported. Found: {string.Join(", ", testProjects.Select(x => Path.GetFileName(x)))}");
                return 3;
            }

            ThisAssemblyPath = Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location);
            BuildStdProps = $"\"/p:_Xunit_ImportTargetsFile={Path.Combine(ThisAssemblyPath, "import.targets")}\" " +
                            $"/p:Configuration={Configuration}";

            var testProject = testProjects[0];

            var targetFrameworks = GetTargetFrameworks(testProject);
            if (targetFrameworks == null)
            {
                WriteLineError("Detection failed! Please ensure you're using 'xunit.core' v2.3 beta 2 or later.");
                return 3;
            }

            if (requestedTargetFramework != null)
            {
                if (!targetFrameworks.Contains(requestedTargetFramework, StringComparer.OrdinalIgnoreCase))
                {
                    WriteLineError($"Unknown target framework '{requestedTargetFramework}'; available frameworks: {string.Join(", ", targetFrameworks.Select(f => $"'{f}'"))}");
                    return 3;
                }

                return RunTargetFramework(testProject, requestedTargetFramework, amendOutputFileNames: false);
            }

            var returnValue = 0;

            foreach (var targetFramework in targetFrameworks)
            {
                var result = RunTargetFramework(testProject, targetFramework, amendOutputFileNames: targetFrameworks.Length > 1);
                if (result < 0)
                    return result;

                returnValue = Math.Max(result, returnValue);
            }

            return returnValue;
        }
        catch (Exception ex)
        {
            WriteLineError(ex.ToString());
            return 3;
        }
    }

    ProcessStartInfo GetMsBuildProcessStartInfo(string testProject)
    {
        var args = $"\"{testProject}\" /nologo /verbosity:{MsBuildVerbosity ?? DefaultMsBuildVerbosity} {BuildStdProps} ";

        if (UseMsBuild)
            return new ProcessStartInfo { FileName = MsBuild.MsBuildName, Arguments = args };
        else
            return new ProcessStartInfo { FileName = DotNetMuxer.MuxerPath, Arguments = $"msbuild {args}" };
    }

    string[] GetTargetFrameworks(string testProject)
    {
        var tmpFile = Path.GetTempFileName();

        try
        {
            var testProjectFileName = Path.GetFileName(testProject);
            WriteLine($"Detecting target frameworks in {testProjectFileName}...");

            var psi = GetMsBuildProcessStartInfo(testProject);
            psi.Arguments += $"/t:_Xunit_GetTargetFrameworks \"/p:_XunitInfoFile={tmpFile}\"";
            WriteLineDiagnostics($"EXEC: \"{psi.FileName}\" {psi.Arguments}");

            var process = Process.Start(psi);

            process.WaitForExit();
            if (process.ExitCode != 0)
                return null;

            return File.ReadAllLines(tmpFile);
        }
        finally
        {
            File.Delete(tmpFile);
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("xUnit.net .NET CLI Console Runner");
        Console.WriteLine("Copyright (C) .NET Foundation.");
        Console.WriteLine();
        Console.WriteLine("usage: dotnet xunit [configFile] [options] [reporter] [resultFormat filename [...]]");
        Console.WriteLine();
        Console.WriteLine("Note: Configuration files must end in .json (for JSON) or .config (for XML)");
        Console.WriteLine("      XML configuration files are only supported on net4x frameworks");
        Console.WriteLine();
        Console.WriteLine("Valid options (all frameworks):");
        Console.WriteLine("  -framework name        : set the framework (default: all targeted frameworks)");
        Console.WriteLine("  -configuration name    : set the build configuration (default: 'Debug')");
        Console.WriteLine("  -nobuild               : do not build the test assembly before running");
        Console.WriteLine("  -nologo                : do not show the copyright message");
        Console.WriteLine("  -nocolor               : do not output results with colors");
        Console.WriteLine("  -failskips             : convert skipped tests into failures");
        Console.WriteLine("  -stoponfail            : stop on first test failure");
        Console.WriteLine("  -parallel option       : set parallelization based on option");
        Console.WriteLine("                         :   none        - turn off parallelization");
        Console.WriteLine("                         :   collections - parallelize test collections");
        Console.WriteLine("  -maxthreads count      : maximum thread count for collection parallelization");
        Console.WriteLine("                         :   default   - run with default (1 thread per CPU thread)");
        Console.WriteLine("                         :   unlimited - run with unbounded thread count");
        Console.WriteLine("                         :   (number)  - limit task thread pool size to 'count'");
        Console.WriteLine("  -wait                  : wait for input after completion");
        Console.WriteLine("  -diagnostics           : enable diagnostics messages for all test assemblies");
        Console.WriteLine("  -internaldiagnostics   : enable internal diagnostics messages for all test assemblies");
        Console.WriteLine("  -debug                 : launch the debugger to debug the tests");
        Console.WriteLine("  -serialize             : serialize all test cases (for diagnostic purposes only)");
        Console.WriteLine("  -trait \"name=value\"    : only run tests with matching name/value traits");
        Console.WriteLine("                         : if specified more than once, acts as an OR operation");
        Console.WriteLine("  -notrait \"name=value\"  : do not run tests with matching name/value traits");
        Console.WriteLine("                         : if specified more than once, acts as an AND operation");
        Console.WriteLine("  -method \"name\"         : run a given test method (should be fully specified;");
        Console.WriteLine("                         : i.e., 'MyNamespace.MyClass.MyTestMethod')");
        Console.WriteLine("                         : if specified more than once, acts as an OR operation");
        Console.WriteLine("  -class \"name\"          : run all methods in a given test class (should be fully");
        Console.WriteLine("                         : specified; i.e., 'MyNamespace.MyClass')");
        Console.WriteLine("                         : if specified more than once, acts as an OR operation");
        Console.WriteLine("  -namespace \"name\"      : run all methods in a given namespace (i.e.,");
        Console.WriteLine("                         : 'MyNamespace.MySubNamespace')");
        Console.WriteLine("                         : if specified more than once, acts as an OR operation");
        Console.WriteLine("  -noautoreporters       : do not allow reporters to be auto-enabled by environment");
        Console.WriteLine("                         : (for example, auto-detecting TeamCity or AppVeyor)");
        Console.WriteLine("  -usemsbuild            : build with msbuild instead of dotnet");
        Console.WriteLine("  -msbuildverbosity      : sets MSBuild verbosity level (default: 'minimal')");
        Console.WriteLine();
        Console.WriteLine("Valid options (net4x frameworks only):");
        Console.WriteLine("  -noappdomain           : do not use app domains to run test code");
        Console.WriteLine("  -noshadow              : do not shadow copy assemblies");
        Console.WriteLine("  -x86                   : force tests to run in 32-bit mode");
        Console.WriteLine();
        Console.WriteLine("Valid options (netcoreapp frameworks only):");
        Console.WriteLine("  -fxversion version     : set the .NET Core framework version");
        Console.WriteLine();

        // TODO: Can't dynamically get the reporter list, hardcoded for now...
        Console.WriteLine("Reporters: (optional, choose only one)");
        Console.WriteLine("  -appveyor              : forces AppVeyor CI mode (normally auto-detected)");
        Console.WriteLine("  -json                  : show progress messages in JSON format");
        Console.WriteLine("  -quiet                 : do not show progress messages");
        Console.WriteLine("  -teamcity              : forces TeamCity mode (normally auto-detected)");
        Console.WriteLine("  -verbose               : show verbose progress messages");
        Console.WriteLine();

        // TODO: Can't dynamically get the transform factory list, hardcoded for now...
        Console.WriteLine("Result formats: (optional, choose one or more)");
        Console.WriteLine("  -xml <filename>        : output results to xUnit.net v2 XML file");
        Console.WriteLine("  -xmlv1 <filename>      : [net4x only] output results to xUnit.net v1 XML file");
        Console.WriteLine("  -nunit <filename>      : [net4x only] output results to NUnit v2.5 XML file");
        Console.WriteLine("  -html <filename>       : [net4x only] output results to HTML file");
    }

    int RunTargetFramework(string testProject, string targetFramework, bool amendOutputFileNames)
    {
        string extraArgs;

        if (amendOutputFileNames)
        {
            var amendedParsedArgs = ParsedArgs.ToDictionary(kvp => kvp.Key, kvp => new List<string>(kvp.Value));
            foreach (var key in OutputFileArgs)
                if (amendedParsedArgs.TryGetSingleValue(key, out var filePath))
                    amendedParsedArgs[key][0] = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}-{targetFramework}{Path.GetExtension(filePath)}");
            extraArgs = ToArgumentsString(amendedParsedArgs);
        }
        else
            extraArgs = ToArgumentsString(ParsedArgs);

        var tmpFile = Path.GetTempFileName();
        try
        {
            var target = default(string);

            if (NoBuild)
            {
                target = "_Xunit_GetTargetValues";
                WriteLine($"Locating binaries for framework {targetFramework}...");
            }
            else
            {
                target = "Build;_Xunit_GetTargetValues";
                WriteLine($"Building for framework {targetFramework}...");
            }

            var psi = GetMsBuildProcessStartInfo(testProject);
            psi.Arguments += $"/t:{target} \"/p:_XunitInfoFile={tmpFile}\" \"/p:TargetFramework={targetFramework}\"";
            WriteLineDiagnostics($"EXEC: \"{psi.FileName}\" {psi.Arguments}");

            var process = Process.Start(psi);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                WriteLineError("Build failed!");
                return process.ExitCode;
            }

            var lines = File.ReadAllLines(tmpFile);
            var outputPath = "";
            var assemblyName = "";
            var targetFileName = "";
            var targetFrameworkIdentifier = "";
            var targetFrameworkVersion = "";
            var runtimeFrameworkVersion = "";

            foreach (var line in lines)
            {
                var idx = line.IndexOf(':');
                if (idx <= 0) continue;
                var name = line.Substring(0, idx)?.Trim().ToLowerInvariant();
                var value = line.Substring(idx + 1)?.Trim();
                if (name == "outputpath")
                    outputPath = value;
                else if (name == "assemblyname")
                    assemblyName = value;
                else if (name == "targetfilename")
                    targetFileName = value;
                else if (name == "targetframeworkidentifier")
                    targetFrameworkIdentifier = value;
                else if (name == "targetframeworkversion")
                    targetFrameworkVersion = value;
                else if (name == "runtimeframeworkversion")
                    runtimeFrameworkVersion = value;
            }

            var version = string.IsNullOrWhiteSpace(targetFrameworkVersion) ? new Version("0.0.0.0") : new Version(targetFrameworkVersion.TrimStart('v'));

            if (targetFrameworkIdentifier == ".NETCoreApp")
            {
                var fxVersion = FxVersion ?? runtimeFrameworkVersion;
                WriteLine($"Running .NET Core {fxVersion} tests for framework {targetFramework}...");
                return RunDotNetCoreProject(outputPath, assemblyName, targetFileName, extraArgs, fxVersion, $"netcoreapp{version.Major}.0");
            }
            if (targetFrameworkIdentifier == ".NETFramework" && version >= Version452)
            {
                WriteLine($"Running desktop CLR tests for framework {targetFramework}...");
                return RunDesktopProject(outputPath, targetFileName, extraArgs);
            }

            WriteLineWarning($"Unsupported target framework '{targetFrameworkIdentifier} {version}' (only .NETCoreApp 1.x/2.x and .NETFramework 4.5.2+ are supported)");
            return 0;
        }
        finally
        {
            File.Delete(tmpFile);
        }
    }

    ProcessStartInfo CheckForMono(ProcessStartInfo psi)
    {
        // Depend on desktop CLR on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return psi;

        psi.Arguments = "\"" + psi.FileName + "\" " + psi.Arguments;

        // By default, OS X uses 32-bit Mono
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !Force32bit)
            psi.FileName = "mono64";
        else
            psi.FileName = "mono";

        return psi;
    }

    int RunDesktopProject(string outputPath, string targetFileName, string extraArgs)
    {
        var consoleFolder = Path.GetFullPath(Path.Combine(ThisAssemblyPath, "..", "..", "tools", "net452"));

        // Debug hack to be able to run from the compilation folder
        if (!Directory.Exists(consoleFolder))
            consoleFolder = Path.GetFullPath(Path.Combine(ThisAssemblyPath, "..", "..", "..", "..", "xunit.console", "bin", "Debug", "net452", "win7-x86"));

        var executableName = Force32bit ? "xunit.console.x86.exe" : "xunit.console.exe";
        var psi = CheckForMono(new ProcessStartInfo
        {
            FileName = Path.Combine(consoleFolder, executableName),
            Arguments = $@"""{targetFileName}"" {extraArgs}",
            WorkingDirectory = Path.GetFullPath(outputPath)
        });

        WriteLineDiagnostics($"EXEC: \"{psi.FileName}\" {psi.Arguments}");
        WriteLineDiagnostics($"  IN: {psi.WorkingDirectory}");

        var runTests = Process.Start(psi);
        runTests.WaitForExit();

        return runTests.ExitCode;
    }

    int RunDotNetCoreProject(string outputPath, string assemblyName, string targetFileName, string extraArgs, string fxVersion, string netCoreAppVersion)
    {
        var consoleFolder = Path.GetFullPath(Path.Combine(ThisAssemblyPath, "..", "..", "tools", netCoreAppVersion));

        // Debug hack to be able to run from the compilation folder
        if (!Directory.Exists(consoleFolder))
            consoleFolder = Path.GetFullPath(Path.Combine(ThisAssemblyPath, "..", "..", "..", "..", "xunit.console", "bin", "Debug", netCoreAppVersion));

        if (!Directory.Exists(consoleFolder))
        {
            WriteLineError($"Could not locate runner DLL for {netCoreAppVersion}; unsupported version of .NET Core");
            return 3;
        }

        var runner = Path.Combine(consoleFolder, "xunit.console.dll");
        var psi = new ProcessStartInfo
        {
            FileName = DotNetMuxer.MuxerPath,
            Arguments = $@"exec --fx-version {fxVersion} ""{runner}"" ""{targetFileName}"" {extraArgs}",
            WorkingDirectory = Path.GetFullPath(outputPath)
        };

        WriteLineDiagnostics($"EXEC: \"{psi.FileName}\" {psi.Arguments}");
        WriteLineDiagnostics($"  IN: {psi.WorkingDirectory}");

        var runTests = Process.Start(psi);
        runTests.WaitForExit();

        return runTests.ExitCode;
    }

    string ToArgumentsString(Dictionary<string, List<string>> parsedArgs)
        => string.Join(" ", parsedArgs.SelectMany(kvp => kvp.Value.Select(value => value == null ? kvp.Key : $"{kvp.Key} \"{value}\"")));

    void WriteLine(string message)
    {
        if (!Quiet)
            WriteLineWithColor(ConsoleColor.White, message);
    }

    void WriteLineDiagnostics(string message)
    {
        if (InternalDiagnostics)
            WriteLineWithColor(ConsoleColor.DarkGray, message);
    }

    void WriteLineError(string message)
        => WriteLineWithColor(ConsoleColor.Red, message, Console.Error);

    void WriteLineWarning(string message)
        => WriteLineWithColor(ConsoleColor.Yellow, message);

    void WriteLineWithColor(ConsoleColor color, string message, TextWriter writer = null)
    {
        if (!NoColor)
            ConsoleHelper.SetForegroundColor(color);

        (writer ?? Console.Out).WriteLine(message);

        if (!NoColor)
            ConsoleHelper.ResetColor();
    }
}
