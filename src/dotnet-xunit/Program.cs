using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

class Program
{
    static HashSet<string> HelpArgs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "-?", "/?", "-h", "--help" };
    static HashSet<string> OutputFileArgs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "-xml", "-xmlv1", "-nunit", "-html" };
    static Version Version452 = new Version("4.5.2");

    string Configuration;
    bool Force32bit;
    string FxVersion;
    bool InternalDiagnostics;
    bool NoColor;
    Dictionary<string, string> ParsedArgs;

    static int Main(string[] args)
        => new Program().Execute(args);

    int Execute(string[] args)
    {
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
                {
                    Force32bit = true;
                    ParsedArgs.Remove("-x86");
                }

                if (ParsedArgs.TryGetParameterWithoutValue("-internaldiagnostics"))
                    InternalDiagnostics = true;

                if (ParsedArgs.TryGetParameterWithoutValue("-nocolor"))
                    NoColor = true;

                requestedTargetFramework = ParsedArgs.GetAndRemoveParameterWithValue("-framework");
                Configuration = ParsedArgs.GetAndRemoveParameterWithValue("-configuration") ?? "Debug";
                FxVersion = ParsedArgs.GetAndRemoveParameterWithValue("-fxversion");

                // Need to amend the paths for the report output, since we are always running
                // in the context of the bin folder, not the project folder
                var currentDirectory = Directory.GetCurrentDirectory();
                foreach (var key in OutputFileArgs)
                    if (ParsedArgs.TryGetValue(key, out var fileName))
                        ParsedArgs[key] = Path.GetFullPath(Path.Combine(currentDirectory, fileName));
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
                WriteLineError("Could not find any project file in the current directory.");
                return 3;
            }

            if (testProjects.Count > 1)
            {
                WriteLineError($"Multiple project files were found; only a single project file is supported. Found: {string.Join(", ", testProjects.Select(x => Path.GetFileName(x)))}");
                return 3;
            }

            var testProject = testProjects[0];
            var testProjectFolder = Path.GetDirectoryName(testProject);
            var testProjectFile = Path.GetFileName(testProject);
            var objFolder = Path.Combine(testProjectFolder, "obj");

            var projectPropsFile = Path.Combine(objFolder, testProjectFile + ".dotnet-xunit.props");
            File.WriteAllText(projectPropsFile, @"
<Project>
  <PropertyGroup>
    <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
    <CopyNuGetImplementations>true</CopyNuGetImplementations>
    <DebugType Condition=""'$(TargetFrameworkIdentifier)' != '.NETCoreApp'"">Full</DebugType>
    <GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>
    <GenerateRuntimeConfigurationFiles>true</GenerateRuntimeConfigurationFiles>
    <GenerateDependencyFile>true</GenerateDependencyFile>
  </PropertyGroup>
</Project>");

            var projectTargetsFile = Path.Combine(objFolder, testProjectFile + ".dotnet-xunit.targets");
            File.WriteAllText(projectTargetsFile, @"
<Project>
   <Target Name=""_Xunit_GetTargetFrameworks"">
     <ItemGroup Condition="" '$(TargetFrameworks)' == '' "">
       <_XunitTargetFrameworksLines Include=""$(TargetFramework)"" />
     </ItemGroup>
     <ItemGroup Condition="" '$(TargetFrameworks)' != '' "">
       <_XunitTargetFrameworksLines Include=""$(TargetFrameworks)"" />
     </ItemGroup>
     <WriteLinesToFile File=""$(_XunitInfoFile)"" Lines=""@(_XunitTargetFrameworksLines)"" Overwrite=""true"" />
   </Target>
   <Target Name=""_Xunit_GetTargetValues"">
     <ItemGroup>
       <_XunitInfoLines Include=""OutputPath: $(OutputPath)""/>
       <_XunitInfoLines Include=""AssemblyName: $(AssemblyName)""/>
       <_XunitInfoLines Include=""TargetFileName: $(TargetFileName)""/>
       <_XunitInfoLines Include=""TargetFrameworkIdentifier: $(TargetFrameworkIdentifier)""/>
       <_XunitInfoLines Include=""TargetFrameworkVersion: $(TargetFrameworkVersion)""/>
     </ItemGroup>
     <WriteLinesToFile File=""$(_XunitInfoFile)"" Lines=""@(_XunitInfoLines)"" Overwrite=""true"" />
   </Target>
</Project>");

            var tmpFile = Path.GetTempFileName();
            var psi = new ProcessStartInfo
            {
                FileName = DotNetMuxer.MuxerPath,
                Arguments = $"msbuild \"{testProject}\" /t:_Xunit_GetTargetFrameworks /nologo \"/p:_XunitInfoFile={tmpFile}\""
            };

            WriteLine($"Detecting target frameworks in {testProjectFile}...");
            WriteLineDiagnostics($"EXEC: \"{psi.FileName}\" {psi.Arguments}");

            try
            {
                var process = Process.Start(psi);
                var returnValue = 0;

                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    WriteLineError("Detection failed!");
                    return 3;
                }

                var targetFrameworks = File.ReadAllLines(tmpFile);
                if (requestedTargetFramework != null)
                {
                    if (!targetFrameworks.Contains(requestedTargetFramework, StringComparer.OrdinalIgnoreCase))
                    {
                        WriteLineError($"Unknown target framework '{requestedTargetFramework}'; available frameworks: {string.Join(", ", targetFrameworks.Select(f => $"'{f}'"))}");
                        return 3;
                    }

                    returnValue = RunTargetFramework(testProject, requestedTargetFramework, amendOutputFileNames: false);
                }
                else
                {
                    foreach (var targetFramework in targetFrameworks)
                        returnValue = Math.Max(RunTargetFramework(testProject, targetFramework, amendOutputFileNames: targetFrameworks.Length > 1), returnValue);
                }

                return returnValue;
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }
        catch (Exception ex)
        {
            WriteLineError(ex.ToString());
            return 3;
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("xUnit.net .NET CLI Console Runner");
        Console.WriteLine("Copyright (C) .NET Foundation.");
        Console.WriteLine();
        Console.WriteLine("usage: dotnet xunit [configFile] [assemblyFile [configFile]...] [options] [reporter] [resultFormat filename [...]]");
        Console.WriteLine();
        Console.WriteLine("Note: Configuration files must end in .json (for JSON) or .config (for XML)");
        Console.WriteLine("      XML configuration files are only supported on net4x frameworks");
        Console.WriteLine();
        Console.WriteLine("Valid options (all frameworks):");
        Console.WriteLine("  -framework name        : set the framework (default: all targeted frameworks)");
        Console.WriteLine("  -configuration name    : set the build configuration (default: 'Debug')");
        Console.WriteLine("  -nologo                : do not show the copyright message");
        Console.WriteLine("  -nocolor               : do not output results with colors");
        Console.WriteLine("  -failskips             : convert skipped tests into failures");
        Console.WriteLine("  -parallel option       : set parallelization based on option");
        Console.WriteLine("                         :   none        - turn off all parallelization");
        Console.WriteLine("                         :   collections - only parallelize collections");
        Console.WriteLine("                         :   assemblies  - only parallelize assemblies");
        Console.WriteLine("                         :   all         - parallelize assemblies & collections");
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
        WriteLine($"Building for framework {targetFramework}...");

        string extraArgs;

        if (amendOutputFileNames)
        {
            var amendedParsedArgs = new Dictionary<string, string>(ParsedArgs);
            foreach (var key in OutputFileArgs)
                if (amendedParsedArgs.TryGetValue(key, out var filePath))
                    amendedParsedArgs[key] = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}-{targetFramework}{Path.GetExtension(filePath)}");
            extraArgs = ToArgumentsString(amendedParsedArgs);
        }
        else
            extraArgs = ToArgumentsString(ParsedArgs);

        var tmpFile = Path.GetTempFileName();
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = DotNetMuxer.MuxerPath,
                Arguments = $"msbuild \"{testProject}\" /t:Build;_Xunit_GetTargetValues /nologo \"/p:_XunitInfoFile={tmpFile}\" \"/p:TargetFramework={targetFramework}\" /p:Configuration={Configuration}"
            };

            WriteLineDiagnostics($"EXEC: \"{psi.FileName}\" {psi.Arguments}");

            var process = Process.Start(psi);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                WriteLineError("Build failed!");
                return 1;
            }

            var lines = File.ReadAllLines(tmpFile);
            var outputPath = "";
            var assemblyName = "";
            var targetFileName = "";
            var targetFrameworkIdentifier = "";
            var targetFrameworkVersion = "";

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
            }

            var version = string.IsNullOrWhiteSpace(targetFrameworkVersion) ? new Version("0.0.0.0") : new Version(targetFrameworkVersion.TrimStart('v'));

            if (targetFrameworkIdentifier == ".NETCoreApp")
            {
                WriteLine($"Running .NET Core tests for framework {targetFramework}...");
                return RunDotNetCoreProject(outputPath, assemblyName, targetFileName, extraArgs);
            }
            if (targetFrameworkIdentifier == ".NETFramework" && version >= Version452)
            {
                WriteLine($"Running desktop CLR tests for framework {targetFramework}...");
                return RunDesktopProject(outputPath, targetFileName, extraArgs);
            }

            WriteLineWarning($"Unsupported target framework '{targetFrameworkIdentifier} {version}' (only .NETCoreApp 1.0+ and .NETFramework 4.5.2+ are supported)");
            return 0;
        }
        finally
        {
            File.Delete(tmpFile);
        }
    }

    int RunDesktopProject(string outputPath, string targetFileName, string extraArgs)
    {
        var thisAssemblyPath = typeof(Program).GetTypeInfo().Assembly.Location;
        var consoleFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "..", "..", "tools", "net452"));

        // Debug hack to be able to run from the compilation folder
        if (!Directory.Exists(consoleFolder))
            consoleFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "..", "..", "..", "..", "xunit.console", "bin", "Debug", "net452", "win7-x86"));

        var executableName = Force32bit ? "xunit.console.x86.exe" : "xunit.console.exe";
        var psi = new ProcessStartInfo
        {
            FileName = Path.Combine(consoleFolder, executableName),
            Arguments = $@"""{targetFileName}"" {extraArgs}",
            WorkingDirectory = outputPath
        };

        WriteLineDiagnostics($"EXEC: \"{psi.FileName}\" {psi.Arguments}");

        var runTests = Process.Start(psi);
        runTests.WaitForExit();

        return runTests.ExitCode;
    }

    int RunDotNetCoreProject(string outputPath, string assemblyName, string targetFileName, string extraArgs)
    {
        var thisAssemblyPath = typeof(Program).GetTypeInfo().Assembly.Location;
        var consoleFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "..", "..", "tools", "netcoreapp1.0"));

        // Debug hack to be able to run from the compilation folder
        if (!Directory.Exists(consoleFolder))
            consoleFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "..", "..", "..", "..", "xunit.console", "bin", "Debug", "netcoreapp1.0"));

        foreach (var sourceFile in Directory.EnumerateFiles(consoleFolder))
        {
            var destinationFile = Path.Combine(outputPath, Path.GetFileName(sourceFile));
            File.Copy(sourceFile, destinationFile, true);
        }

        var dotnetArguments = "exec";
        if (File.Exists(Path.Combine(outputPath, assemblyName + ".deps.json")))
            dotnetArguments += $@" --depsfile ""{assemblyName}.deps.json""";
        if (File.Exists(Path.Combine(outputPath, assemblyName + ".runtimeconfig.json")))
            dotnetArguments += $@" --runtimeconfig ""{assemblyName}.runtimeconfig.json""";
        if (FxVersion != null)
            dotnetArguments += $" --fx-version {FxVersion}";

        var psi = new ProcessStartInfo
        {
            FileName = DotNetMuxer.MuxerPath,
            Arguments = $@"{dotnetArguments} xunit.console.dll ""{targetFileName}"" {extraArgs}",
            WorkingDirectory = outputPath
        };

        WriteLineDiagnostics($"EXEC: \"{psi.FileName}\" {psi.Arguments}");

        var runTests = Process.Start(psi);
        runTests.WaitForExit();

        return runTests.ExitCode;
    }

    string ToArgumentsString(Dictionary<string, string> parsedArgs)
        => string.Join(" ", parsedArgs.Select(kvp => kvp.Value == null ? kvp.Key : $"{kvp.Key} \"{kvp.Value}\""));

    void WriteLine(string message)
        => WriteLineWithColor(ConsoleColor.White, message);

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
            Console.ForegroundColor = color;

        (writer ?? Console.Out).WriteLine(message);

        if (!NoColor)
            Console.ResetColor();
    }
}
