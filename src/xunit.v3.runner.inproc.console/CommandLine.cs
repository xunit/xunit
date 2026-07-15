using System.Reflection;
using Xunit.Runner.Common;

namespace Xunit.Runner.InProc.SystemConsole;

/// <summary/>
public class CommandLine : CommandLineParserBase
{
	readonly Assembly assembly;
	readonly string assemblyFileName;

	/// <summary/>
	public CommandLine(
		ConsoleHelper consoleHelper,
		Assembly assembly,
		string[] args,
		IReadOnlyList<IRunnerReporter> runnerReporters)
			: base(consoleHelper, runnerReporters, args)
	{
		this.assembly = assembly;
		assemblyFileName = assembly.GetSafeLocation() ?? throw new ArgumentException("Test assembly must have an on-disk representation");

		// General options
		AddParser("assemblyInfo", OnAssemblyInfo, CommandLineGroup.General, null, "return test assembly information; does not find or run tests (implies -noColor and -noLogo)");
		AddParser("assertEquivalentMaxDepth", OnAssertEquivalentMaxDepth, CommandLineGroup.General, "<option>",
			"override the maximum recursive depth when comparing objects with Assert.Equivalent",
			$"  any integer value >= 1 is valid (default value is {EnvironmentVariables.Defaults.AssertEquivalentMaxDepth})"
		);
		AddParser(
			"automated", OnAutomated, CommandLineGroup.General, "[option]",
			"enables automated mode (ensures all output is machine parseable)",
			"  <unset> - use synchronous reporting requested by the configuration",
			"  async   - asynchronously report messages (and don't wait)",
			"  sync    - synchronously report messages (and wait for a carriage return after each)"
		);
		AddParser("id", OnID, CommandLineGroup.General, "<id>", "run a test case (by unique ID)");
		AddParser("pause", OnPause, CommandLineGroup.General, null, "wait for input before running tests (ignored with -automated)");
#if !XUNIT_AOT
		AddParser("run", OnRun, CommandLineGroup.General, "<serialization>", "run a test case (by serialization)");
#endif
		AddParser("wait", OnWait, CommandLineGroup.General, null, "wait for input after completion (ignored with -automated)");
		AddParser("waitForDebugger", OnWaitForDebugger, CommandLineGroup.General, null, "pauses execution until a debugger has been attached");

		// Simple filtering
		AddParser(
			"displayName", OnDisplayName, CommandLineGroup.FilterSimple, "\"name\"",
			"run all tests with a matching test case display name (wildcard '*' is supported",
			"at the beginning and/or end of the filter)",
			"  if specified more than once, acts as an OR operation"
		);
		AddParser(
			"displayName-", OnDisplayNameMinus, CommandLineGroup.FilterSimple, "\"name\"",
			"do not run tests with a matching test case display name (wildcard '*' is supported",
			"at the beginning and/or end of the filter)",
			"  if specified more than once, acts as an AND operation"
		);

		// VSTest filtering
		AddParser(
			"filterVSTest", OnFilterVSTest, CommandLineGroup.FilterVSTest, "\"query\"",
			"use a VSTest filter to select tests",
			"for more information about the filter syntax, see",
			"https://learn.microsoft.com/dotnet/core/testing/selective-unit-tests?pivots=xunit"
		);
	}

	/// <summary/>
	public bool AutomatedRequested =>
		Args.Any(a => a.Equals("-automated", StringComparison.OrdinalIgnoreCase));

	void AddAssembly(
		Assembly assembly,
		string assemblyFileName,
		string? configFileName,
		int? seed)
	{
		if (!FileExists(assemblyFileName))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "assembly not found: {0}", assemblyFileName));
		if (configFileName is not null && !FileExists(configFileName))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "config file not found: {0}", configFileName));

		var targetFramework = assembly.GetTargetFramework();
		var projectAssembly = new XunitProjectAssembly(Project, GetFullPath(assemblyFileName), new(3, targetFramework))
		{
			Assembly = assembly,
			ConfigFileName = GetFullPath(configFileName),
		};

		ConfigReader_Json.Load(projectAssembly.Configuration, projectAssembly.AssemblyFileName, projectAssembly.ConfigFileName, ParseWarnings);
		projectAssembly.Configuration.Seed = seed ?? projectAssembly.Configuration.Seed;

		Project.Add(projectAssembly);
	}

	XunitProjectAssembly GetAssembly() =>
		Project.Assemblies.FirstOrDefault()
			?? throw new ArgumentException("no assembly in the project");

	/// <summary/>
	public XunitProjectAssembly Parse()
	{
		if (Project.Assemblies.Count > 0)
			throw new InvalidOperationException("Parse may only be called once");

		var argsStartIndex = 0;

		int? seed = null;
		if (Args.Count > argsStartIndex && Args[argsStartIndex].StartsWith(':'))
		{
			var seedValueText = Args[argsStartIndex].Substring(1);
			if (!int.TryParse(seedValueText, NumberStyles.None, NumberFormatInfo.CurrentInfo, out var parsedValue) || parsedValue < 0)
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "invalid seed value '{0}' (must be an integer in the range of 0 - 2147483647)", seedValueText));

			seed = parsedValue;
			++argsStartIndex;
		}

		string? configFileName = null;
		if (Args.Count > argsStartIndex && !Args[argsStartIndex].StartsWith('-') && Args[argsStartIndex].EndsWith(".json", StringComparison.OrdinalIgnoreCase))
		{
			configFileName = Args[argsStartIndex];
			++argsStartIndex;
		}

		AddAssembly(assembly, assemblyFileName, configFileName, seed);
		ParseInternal(argsStartIndex);

		return Project.Assemblies.Single();
	}

	void OnAssemblyInfo(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		Project.Configuration.AssemblyInfo = true;
	}

	void OnAutomated(KeyValuePair<string, string?> option)
	{
		if (option.Value is not null)
			GetAssembly().Configuration.SynchronousMessageReporting = option.Value.ToUpperInvariant() switch
			{
				"ASYNC" => false,
				"SYNC" => true,
				_ => throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "invalid automated option '{0}'", option.Value)),
			};
	}

	void OnFilterVSTest(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -filterVSTest");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.SetVSTestFilter(option.Value);
	}

	void OnID(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -id");

		GetAssembly().TestCaseIDsToRun.Add(option.Value);
	}

#if !XUNIT_AOT

	void OnRun(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -run");

		GetAssembly().TestCasesToRun.Add(option.Value);
	}

#endif

	void OnWaitForDebugger(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		Project.Configuration.WaitForDebugger = true;
	}
}
