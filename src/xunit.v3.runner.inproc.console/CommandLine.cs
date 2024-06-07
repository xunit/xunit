using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.Runner.Common;

namespace Xunit.Runner.InProc.SystemConsole;

/// <summary/>
public class CommandLine : CommandLineParserBase
{
	readonly Assembly assembly;
	readonly string? assemblyFileName;

	/// <summary/>
	public CommandLine(
		TextWriter consoleWriter,
		Assembly assembly,
		string[] args,
		IReadOnlyList<IRunnerReporter>? runnerReporters = null,
		string? reporterFolder = null)
			: base(consoleWriter, runnerReporters, reporterFolder ?? Path.GetDirectoryName(assembly.GetSafeLocation()), args)
	{
		this.assembly = assembly;
		assemblyFileName = assembly.GetSafeLocation();

		// General options
		AddParser("automated", OnAutomated, CommandLineGroup.General, null, "enables automated mode; ensures all output is machine parseable");
		AddParser(
			"parallel", OnParallel, CommandLineGroup.General, "<option>",
			"set parallelization based on option",
			"  none        - turn off parallelization",
			"  collections - parallelize by collections [default]"
		);
		AddParser("run", OnRun, CommandLineGroup.General, "<serialization>", "Run a test case");
		AddParser("waitForDebugger", OnWaitForDebugger, CommandLineGroup.General, null, "pauses execution until a debugger has been attached");
	}

	/// <summary/>
	public bool AutomatedRequested =>
		Args.Any(a => a.Equals("-automated", StringComparison.OrdinalIgnoreCase));

	void AddAssembly(
		Assembly assembly,
		string? assemblyFileName,
		string? configFileName,
		int? seed)
	{
		if (assemblyFileName is not null && !FileExists(assemblyFileName))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "assembly not found: {0}", assemblyFileName));
		if (configFileName is not null && !FileExists(configFileName))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "config file not found: {0}", configFileName));

		var targetFramework = assembly.GetTargetFramework();
		var projectAssembly = new XunitProjectAssembly(Project)
		{
			Assembly = assembly,
			AssemblyFileName = GetFullPath(assemblyFileName),
			AssemblyMetadata = new(3, targetFramework),
			ConfigFileName = GetFullPath(configFileName),
		};

		ConfigReader_Json.Load(projectAssembly.Configuration, projectAssembly.AssemblyFileName, projectAssembly.ConfigFileName, ParseWarnings);
		projectAssembly.Configuration.Seed = seed ?? projectAssembly.Configuration.Seed;

		Project.Add(projectAssembly);
	}

	/// <summary/>
	protected override Assembly LoadAssembly(string dllFile) =>
#if NETFRAMEWORK
		Assembly.LoadFile(dllFile);
#else
		Assembly.Load(new AssemblyName(Path.GetFileNameWithoutExtension(dllFile)));
#endif

	/// <summary/>
	public XunitProject Parse()
	{
		if (Project.Assemblies.Count > 0)
			throw new InvalidOperationException("Parse may only be called once");

		var argsStartIndex = 0;

		int? seed = null;
#if NETFRAMEWORK
		if (Args.Count > argsStartIndex && Args[argsStartIndex].StartsWith(":", StringComparison.OrdinalIgnoreCase))
#else
		if (Args.Count > argsStartIndex && Args[argsStartIndex].StartsWith(':'))
#endif
		{
			var seedValueText = Args[argsStartIndex].Substring(1);
			if (!int.TryParse(seedValueText, NumberStyles.None, NumberFormatInfo.CurrentInfo, out int parsedValue) || parsedValue < 0)
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "invalid seed value '{0}' (must be an integer in the range of 0 - 2147483647)", seedValueText));

			seed = parsedValue;
			++argsStartIndex;
		}

		string? configFileName = null;
#if NETFRAMEWORK
		if (Args.Count > argsStartIndex && !Args[argsStartIndex].StartsWith("-", StringComparison.OrdinalIgnoreCase) && Args[argsStartIndex].EndsWith(".json", StringComparison.OrdinalIgnoreCase))
#else
		if (Args.Count > argsStartIndex && !Args[argsStartIndex].StartsWith('-') && Args[argsStartIndex].EndsWith(".json", StringComparison.OrdinalIgnoreCase))
#endif
		{
			configFileName = Args[argsStartIndex];
			++argsStartIndex;
		}

		AddAssembly(assembly, assemblyFileName, configFileName, seed);

		return ParseInternal(argsStartIndex);
	}

	// Don't need to store anything, because we rely on AutomatedRequested instead; just want to validate and
	// ignore during parsing, which happens later than is normally useful for us.
	void OnAutomated(KeyValuePair<string, string?> option) =>
		GuardNoOptionValue(option);

	void OnRun(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -run");

		var assembly = Project.Assemblies.FirstOrDefault();
		if (assembly is null)
			throw new ArgumentException("no assembly in the project");

		assembly.TestCasesToRun.Add(option.Value);
	}

	void OnWaitForDebugger(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		Project.Configuration.WaitForDebugger = true;
	}
}
