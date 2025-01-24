using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public abstract class CommandLineParserBase
{
	readonly Dictionary<string, (CommandLineGroup Group, string? ArgumentDisplay, string[] Descriptions, Action<KeyValuePair<string, string?>> Handler)> parsers = new(StringComparer.OrdinalIgnoreCase);
	readonly string? reporterFolder;
	IReadOnlyList<IRunnerReporter>? runnerReporters;

	/// <summary/>
	protected CommandLineParserBase(
		ConsoleHelper consoleHelper,
		IReadOnlyList<IRunnerReporter>? runnerReporters,
		string? reporterFolder,
		string[] args)
	{
		this.runnerReporters = runnerReporters;
		this.reporterFolder = reporterFolder;

		ConsoleHelper = Guard.ArgumentNotNull(consoleHelper);
		Args = GetArguments(Guard.ArgumentNotNull(args));

		if (string.IsNullOrWhiteSpace(this.reporterFolder))
			this.reporterFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

		// General options
		AddParser(
			"culture", OnCulture, CommandLineGroup.General, "<option>",
			"run tests under the given culture (v3 assemblies only)",
			"note: when running a v1/v2 assembly, the culture option will be ignored",
			"  default   - run with default operating system culture",
			"  invariant - run with the invariant culture",
			"  (string)  - run with the given culture (i.e., 'en-US')"
		);
		AddParser("debug", OnDebug, CommandLineGroup.General, null, "launch the debugger to debug the tests");
		AddParser("diagnostics", OnDiagnostics, CommandLineGroup.General, null, "enable diagnostics messages for all test assemblies");
		AddParser(
			"explicit", OnExplicit, CommandLineGroup.General, "<option>",
			"change the way explicit tests are handled",
			"  on   - run both explicit and non-explicit tests",
			"  off  - run only non-explicit tests [default]",
			"  only - run only explicit tests"
		);
		AddParser("failSkips", OnFailSkips, CommandLineGroup.General, null, "treat skipped tests as failures");
		AddParser("failSkips-", OnFailSkipsMinus, CommandLineGroup.General, null, "treat skipped tests as skipped [default]");
		AddParser("failWarns", OnFailWarns, CommandLineGroup.General, null, "treat passing tests with warnings as failures");
		AddParser("failWarns-", OnFailWarnsMinus, CommandLineGroup.General, null, "treat passing tests with warnings as successful [default]");
		AddParser("ignoreFailures", OnIgnoreFailures, CommandLineGroup.General, null, "if tests fail, do not return a failure exit code");
		AddParser("internalDiagnostics", OnInternalDiagnostics, CommandLineGroup.General, null, "enable internal diagnostics messages for all test assemblies");
		AddParser(
			"list", OnList, CommandLineGroup.General, "<option>",
			"list information about the test assemblies rather than running tests (implies -noLogo)",
			"note: you can add '/json' to the end of any option to get the listing in JSON format",
			"  classes - list class names of every class which contains tests",
			"  full    - list complete discovery data",
			"  methods - list class+method names of every method which is a test",
			"  tests   - list just the display name of all tests",
			"  traits  - list the set of trait name/value pairs used in the test assemblies"
		);
		AddParser(
			"longRunning", OnLongRunning, CommandLineGroup.General, "<seconds>",
			"enable long running (hung) test detection (implies -diagnostics) by specifying",
			"the number of seconds (as a positive integer) to report a test as running",
			"too long (most effective with parallelAlgorithm 'conservative')"
		);
		AddParser(
			"maxThreads", OnMaxThreads, CommandLineGroup.General, "<option>",
			"maximum thread count for collection parallelization",
			"  default   - run with default (1 thread per CPU thread)",
			"  unlimited - run with unbounded thread count",
			"  (integer) - use exactly this many threads (e.g., '2' = 2 threads)",
			"  (float)x  - use a multiple of CPU threads (e.g., '2.0x' = 2.0 * the number of CPU threads)"
		);
		AddParser(
			"methodDisplay", OnMethodDisplay, CommandLineGroup.General, "<option>",
			"set default test display name",
			"  classAndMethod - Use a fully qualified name [default]",
			"  method         - Use just the method name"
		);
		AddParser(
			"methodDisplayOptions", OnMethodDisplayOptions, CommandLineGroup.General, "<option>",
			"alters the default test display name",
			"note: you can specify more than one flag by joining with commas",
			"  none                       - apply no alterations [default]",
			"  all                        - apply all alterations",
			"  replacePeriodWithComma     - replace periods in names with commas",
			"  replaceUnderscoreWithSpace - replace underscores in names with spaces",
			"  useOperatorMonikers        - replace operator names with operators",
			"                                 'lt' becomes '<'",
			"                                 'le' becomes '<='",
			"                                 'eq' becomes '='",
			"                                 'ne' becomes '!='",
			"                                 'gt' becomes '>'",
			"                                 'ge' becomes '>='",
			"  useEscapeSequences         - replace ASCII and Unicode escape sequences",
			"                                  X + 2 hex digits (i.e., 'X2C' becomes ',')",
			"                                  U + 4 hex digits (i.e., 'U0192' becomes '" + (char)0x0192 + "')"
		);
		AddParser(
			"noAutoReporters", OnNoAutoReporters, CommandLineGroup.General, null,
			"do not allow reporters to be auto-enabled by environment",
			"(for example, auto-detecting TeamCity or AppVeyor)"
		);
		AddParser("noColor", OnNoColor, CommandLineGroup.General, null, "do not output results with colors");
		AddParser("noLogo", OnNoLogo, CommandLineGroup.General, null, "do not show the copyright message");
		AddParser("parallelAlgorithm", OnParallelAlgorithm, CommandLineGroup.General, "<option>",
			"set the parallelization algoritm",
			"  conservative - start the minimum number of tests [default]",
			"  aggressive   - start as many tests as possible",
			"for more information, see https://xunit.net/docs/running-tests-in-parallel#algorithms"
		);
		AddParser("preEnumerateTheories", OnPreEnumerateTheories, CommandLineGroup.General, null, "enable theory pre-enumeration (disabled by default)");
		AddParser("showLiveOutput", OnShowLiveOutput, CommandLineGroup.General, null, "show output messages from tests live");
		AddParser("stopOnFail", OnStopOnFail, CommandLineGroup.General, null, "stop on first test failure");
		AddParser("useAnsiColor", OnUseAnsiColor, CommandLineGroup.General, null, "force using ANSI color output on Windows (non-Windows always uses ANSI colors)");

		// Query filtering
		AddParser(
			"filter", OnFilter, CommandLineGroup.FilterQuery, "\"query\"",
			"use a query filter to select tests (using the query filter language;",
			"in '/assemblyName/namespace/class/method[trait=value]' format)",
			"for more information, see https://xunit.net/docs/query-filter-language"
		);

		// Simple filtering
		AddParser(
			"class", OnClass, CommandLineGroup.FilterSimple, "\"name\"",
			"run all methods in a given test class (type names are fully qualified;",
			"i.e., 'MyNamespace.MyClass' or 'MyNamespace.MyClass+InnerClass'; wildcard '*'",
			"is supported at the beginning and/or end of the filter)",
			"  if specified more than once, acts as an OR operation"
		);
		AddParser(
			"class-", OnClassMinus, CommandLineGroup.FilterSimple, "\"name\"",
			"do not run any methods in a given test class (type names are fully qualified;",
			"i.e., 'MyNamespace.MyClass' or 'MyNamespace.MyClass+InnerClass'; wildcard '*'",
			"is supported at the beginning and/or end of the filter)",
			"  if specified more than once, acts as an AND operation"
		);
		AddParser(
			"method", OnMethod, CommandLineGroup.FilterSimple, "\"name\"",
			"run a given test method (including the fully qualified type name;",
			"i.e., 'MyNamespace.MyClass.MyTestMethod'; wildcard '*' is supported",
			"at the beginning and/or end of the filter)",
			"  if specified more than once, acts as an OR operation"
		);
		AddParser(
			"method-", OnMethodMinus, CommandLineGroup.FilterSimple, "\"name\"",
			"do not run a given test method (including the fully qualified type name;",
			"i.e., 'MyNamespace.MyClass.MyTestMethod'; wildcard '*' is supported",
			"at the beginning and/or end of the filter)",
			"  if specified more than once, acts as an AND operation"
		);
		AddParser(
			"namespace", OnNamespace, CommandLineGroup.FilterSimple, "\"name\"",
			"run all methods in a given namespace (i.e., 'MyNamespace.MySubNamespace';",
			"wildcard '*' is supported at the beginning and/or end of the filter)",
			"  if specified more than once, acts as an OR operation"
		);
		AddParser(
			"namespace-", OnNamespaceMinus, CommandLineGroup.FilterSimple, "\"name\"",
			"do not run any methods in a given namespace (i.e., 'MyNamespace.MySubNamespace';",
			"wildcard '*' is supported at the beginning and/or end of the filter)",
			"  if specified more than once, acts as an AND operation"
		);
		AddParser(
			"trait", OnTrait, CommandLineGroup.FilterSimple, "\"name=value\"",
			"only run tests with matching name/value traits (wildcard '*' is supported at the",
			"beginning and/or end of the trait name and/or value)",
			"  if specified more than once, acts as an OR operation"
		);
		AddParser(
			"trait-", OnTraitMinus, CommandLineGroup.FilterSimple, "\"name=value\"",
			"do not run tests with matching name/value traits (wildcard '*' is supported at the",
			"beginning and/or end of the trait name and/or value)",
			"  if specified more than once, acts as an AND operation"
		);

		// Argument display options
		AddParser("printMaxEnumerableLength", OnPrintMaxEnumerableLength, CommandLineGroup.ArgumentDisplay, "<option>",
			"override the maximum number of values to show when printing a collection",
			"set to 0 to always print the full collection",
			$"  any integer value >= 0 is valid (default value is {EnvironmentVariables.Defaults.PrintMaxEnumerableLength})"
		);
		AddParser("printMaxObjectDepth", OnPrintMaxObjectDepth, CommandLineGroup.ArgumentDisplay, "<option>",
			"override the maximum recursive depth when printing object values",
			"set to 0 to always print objects at all depths",
			"(warning: setting 0 or a very large value can cause stack overflows that may crash the test process)",
			$"  any integer value >= 0 is valid (default value is {EnvironmentVariables.Defaults.PrintMaxObjectDepth})"
		);
		AddParser("printMaxObjectMemberCount", OnPrintMaxObjectMemberCount, CommandLineGroup.ArgumentDisplay, "<option>",
			"override the maximum number of fields and properties to show when printing an object",
			"set to 0 to always print all members",
			$"  any integer value >= 0 is valid (default value is {EnvironmentVariables.Defaults.PrintMaxObjectMemberCount})"
		);
		AddParser("printMaxStringLength", OnPrintMaxStringLength, CommandLineGroup.ArgumentDisplay, "<option>",
			"override the maximum length to show when printing a string value",
			"set to 0 to always print the entire string",
			$"  any integer value >= 0 is valid (default value is {EnvironmentVariables.Defaults.PrintMaxStringLength})"
		);

		// Reporter is hidden because the available list is dynamic
		AddHiddenParser("reporter", OnReporter);

		// Deprecated reporter switches
		AddHiddenParser("json", kvp => OnReporter(new("-reporter", "json")));
		AddHiddenParser("quiet", kvp => OnReporter(new("-reporter", "quiet")));
		AddHiddenParser("silent", kvp => OnReporter(new("-reporter", "silent")));
		AddHiddenParser("teamcity", kvp => OnReporter(new("-reporter", "teamcity")));
		AddHiddenParser("verbose", kvp => OnReporter(new("-reporter", "verbose")));

		// Deprecated filters
		AddHiddenParser("noclass", OnClassMinus);
		AddHiddenParser("nomethod", OnMethodMinus);
		AddHiddenParser("nonamespace", OnNamespaceMinus);
		AddHiddenParser("notrait", OnTraitMinus);
	}

	/// <summary/>
	protected IReadOnlyList<string> Args { get; }

	/// <summary/>
	protected ConsoleHelper ConsoleHelper { get; }

	/// <summary/>
	public bool HelpRequested =>
		Args.Count > 0 && (Args[0] == "-?" || Args[0] == "/?" || Args[0] == "-h" || Args[0] == "--help");

	/// <summary/>
	public List<string> ParseWarnings { get; } = [];

	/// <summary/>
	protected XunitProject Project { get; } = new();

	/// <summary/>
	protected IReadOnlyList<IRunnerReporter> RunnerReporters
	{
		get
		{
			runnerReporters ??= GetAvailableRunnerReporters();
			return runnerReporters;
		}
	}

	/// <summary/>
	protected void AddHiddenParser(
		string @switch,
		Action<KeyValuePair<string, string?>> handler,
		string? replacement = null) =>
			parsers[@switch] = (CommandLineGroup.Hidden, null, replacement is null ? [] : [replacement], handler);

	/// <summary/>
	protected void AddParser(
		string @switch,
		Action<KeyValuePair<string, string?>> handler,
		CommandLineGroup group,
		string? argumentDisplay,
		params string[] descriptions) =>
			parsers[@switch] = (group, argumentDisplay, descriptions, handler);

	static void EnsurePathExists(string path)
	{
		var directory = Path.GetDirectoryName(path);

		if (string.IsNullOrEmpty(directory))
			return;

		Directory.CreateDirectory(directory);
	}

	/// <summary/>
	protected virtual bool FileExists(string? path) =>
		File.Exists(path);

	IReadOnlyList<string> GetArguments(string[] args)
	{
		if (args.Length == 2 && args[0] == "@@")
		{
			var responseFileName = args[1];
			if (!File.Exists(responseFileName))
			{
				ParseWarnings.Add("Response file not found: " + responseFileName);
				return ["-?"];
			}

			try
			{
				return File.ReadAllLines(responseFileName).Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
			}
			catch { }
		}

		return args;
	}

	/// <summary/>
	protected abstract IReadOnlyList<IRunnerReporter> GetAvailableRunnerReporters();

	/// <summary/>
	[return: NotNullIfNotNull(nameof(fileName))]
	protected virtual string? GetFullPath(string? fileName) =>
		fileName is null ? null : Path.GetFullPath(fileName);

	/// <summary/>
	protected static void GuardNoOptionValue(KeyValuePair<string, string?> option)
	{
		if (option.Value is not null)
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "error: unknown command line option: {0}", option.Value));
	}

	/// <summary/>
	protected static bool IsConfigFile(string fileName)
	{
		Guard.ArgumentNotNull(fileName);

		return
			fileName.EndsWith(".config", StringComparison.OrdinalIgnoreCase) ||
			fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary/>
	protected abstract Assembly LoadAssembly(string dllFile);

	/// <summary/>
	protected void OnAssertEquivalentMaxDepth(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -assertEquivalentMaxDepth");

		if (!int.TryParse(option.Value, out var maxDepth) || maxDepth < 1)
			throw new ArgumentException("invalid argument for -assertEquivalentMaxDepth");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.AssertEquivalentMaxDepth = maxDepth;
	}

	void OnClass(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -class");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.AddIncludedClassFilter(option.Value);
	}

	void OnClassMinus(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
#pragma warning disable CA1308  // This is for UI purposes, not normalization purposes
			throw new ArgumentException("missing argument for " + option.Key.ToLowerInvariant());
#pragma warning restore CA1308

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.AddExcludedClassFilter(option.Value);
	}

	void OnCulture(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -culture");

		var culture = option.Value switch
		{
			"default" => null,
			"invariant" => "",
			_ => option.Value
		};

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Culture = culture;
	}

	void OnDebug(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		Project.Configuration.Debug = true;
	}

	void OnDiagnostics(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.DiagnosticMessages = true;
	}

	void OnExplicit(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -explicit");

		var explicitOption = option.Value.ToUpperInvariant() switch
		{
			"OFF" => ExplicitOption.Off,
			"ON" => ExplicitOption.On,
			"ONLY" => ExplicitOption.Only,
			_ => throw new ArgumentException("invalid argument for -explicit"),
		};

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.ExplicitOption = explicitOption;
	}

	void OnFailSkips(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.FailSkips = true;
	}

	void OnFailSkipsMinus(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.FailSkips = false;
	}

	void OnFailWarns(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.FailTestsWithWarnings = true;
	}

	void OnFailWarnsMinus(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.FailTestsWithWarnings = false;
	}

	void OnFilter(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -filter");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.AddQueryFilter(option.Value);
	}

	void OnIgnoreFailures(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		Project.Configuration.IgnoreFailures = true;
	}

	void OnInternalDiagnostics(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.InternalDiagnosticMessages = true;
	}

	void OnList(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -list");

		var pieces = option.Value.Split('/');
		var list = default((ListOption Option, ListFormat Format)?);

		if (pieces.Length < 3 && Enum.TryParse<ListOption>(pieces[0], ignoreCase: true, out var listOption))
		{
			if (pieces.Length == 1)
				list = (listOption, ListFormat.Text);
			else if (Enum.TryParse<ListFormat>(pieces[1], ignoreCase: true, out var listFormat))
				list = (listOption, listFormat);
		}

		Project.Configuration.List = list ?? throw new ArgumentException("invalid argument for -list");
		Project.Configuration.NoLogo = list.Value.Format == ListFormat.Json;
	}

	void OnLongRunning(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -longRunning");

		if (!int.TryParse(option.Value, NumberStyles.None, NumberFormatInfo.CurrentInfo, out var longRunning))
			throw new ArgumentException("incorrect argument value for -longRunning (must be a positive integer)");

		foreach (var projectAssembly in Project.Assemblies)
		{
			projectAssembly.Configuration.DiagnosticMessages = true;
			projectAssembly.Configuration.LongRunningTestSeconds = longRunning;
		}
	}

	void OnMaxThreads(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -maxThreads");

		int? maxParallelThreads = null;

		switch (option.Value)
		{
			case "0":
			case "default":
				break;

			// Can't support "-1" here because it's interpreted as a new command line switch
			case "unlimited":
				maxParallelThreads = -1;
				break;

			default:
				var match = ConfigUtility.MultiplierStyleMaxParallelThreadsRegex.Match(option.Value);
				// Use invariant format and convert ',' to '.' so we can always support both formats, regardless of locale
				// If we stick to locale-only parsing, we could break people when moving from one locale to another (for example,
				// from people running tests on their desktop in a comma locale vs. running them in CI with a decimal locale).
				maxParallelThreads =
					match.Success && decimal.TryParse(match.Groups[1].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var maxThreadMultiplier)
					? (int)(maxThreadMultiplier * Environment.ProcessorCount)
					: int.TryParse(option.Value, out var threadValue) && threadValue > 0
						? threadValue
						: throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "incorrect argument value for -maxThreads (must be 'default', 'unlimited', a positive number, or a multiplier in the form of '{0}x')", 0.0m));

				break;
		}

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.MaxParallelThreads = maxParallelThreads;
	}

	void OnMethod(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -method");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.AddIncludedMethodFilter(option.Value);
	}

	void OnMethodDisplay(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -methodDisplay");

		if (!Enum.TryParse<TestMethodDisplay>(option.Value, ignoreCase: true, out var methodDisplay))
			throw new ArgumentException("incorrect argument value for -methodDisplay");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.MethodDisplay = methodDisplay;
	}

	void OnMethodDisplayOptions(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -methodDisplayOptions");

		if (!Enum.TryParse<TestMethodDisplayOptions>(option.Value, ignoreCase: true, out var methodDisplayOptions))
			throw new ArgumentException("incorrect argument value for -methodDisplay");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.MethodDisplayOptions = methodDisplayOptions;
	}

	void OnMethodMinus(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
#pragma warning disable CA1308  // This is for UI purposes, not normalization purposes
			throw new ArgumentException("missing argument for " + option.Key.ToLowerInvariant());
#pragma warning restore CA1308

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.AddExcludedMethodFilter(option.Value);
	}

	void OnNamespace(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -namespace");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.AddIncludedNamespaceFilter(option.Value);
	}

	void OnNamespaceMinus(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
#pragma warning disable CA1308  // This is for UI purposes, not normalization purposes
			throw new ArgumentException("missing argument for " + option.Key.ToLowerInvariant());
#pragma warning restore CA1308

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.AddExcludedNamespaceFilter(option.Value);
	}

	void OnNoAutoReporters(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		Project.Configuration.NoAutoReporters = true;
	}

	void OnNoColor(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		Project.Configuration.NoColor = true;

		// Set the environment variable so any plugins can also see the user requested -nocolor
		// For more information, see https://no-color.org/
		Environment.SetEnvironmentVariable(TestProjectConfiguration.EnvNameNoColor, "NO_COLOR");
	}

	void OnNoLogo(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		Project.Configuration.NoLogo = true;
	}

	/// <summary/>
	protected void OnParallel(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -parallel");

		if (!Enum.TryParse(option.Value, ignoreCase: true, out ParallelismOption parallelismOption))
			throw new ArgumentException("incorrect argument value for -parallel");

		var (parallelizeAssemblies, parallelizeTestCollections) = parallelismOption switch
		{
			ParallelismOption.all => (true, true),
			ParallelismOption.assemblies => (true, false),
			ParallelismOption.collections => (false, true),
			_ => (false, false)
		};

		foreach (var projectAssembly in Project.Assemblies)
		{
			projectAssembly.Configuration.ParallelizeAssembly = parallelizeAssemblies;
			projectAssembly.Configuration.ParallelizeTestCollections = parallelizeTestCollections;
		}
	}

	void OnParallelAlgorithm(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -parallelAlgorithm");

		if (!Enum.TryParse(option.Value, ignoreCase: true, out ParallelAlgorithm parallelAlgorithm))
			throw new ArgumentException("incorrect argument value for -parallelAlgorithm");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.ParallelAlgorithm = parallelAlgorithm;
	}

	/// <summary/>
	protected void OnPause(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		Project.Configuration.Pause = true;
	}

	void OnPreEnumerateTheories(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.PreEnumerateTheories = true;
	}

	void OnPrintMaxEnumerableLength(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -printMaxEnumerableLength");

		if (!int.TryParse(option.Value, out var maxValue) || maxValue < 0)
			throw new ArgumentException("incorrect argument value for -printMaxEnumerableLength");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.PrintMaxEnumerableLength = maxValue;
	}

	void OnPrintMaxObjectDepth(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -printMaxObjectDepth");

		if (!int.TryParse(option.Value, out var maxValue) || maxValue < 0)
			throw new ArgumentException("incorrect argument value for -printMaxObjectDepth");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.PrintMaxObjectDepth = maxValue;
	}

	void OnPrintMaxObjectMemberCount(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -printMaxObjectMemberCount");

		if (!int.TryParse(option.Value, out var maxValue) || maxValue < 0)
			throw new ArgumentException("incorrect argument value for -printMaxObjectMemberCount");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.PrintMaxObjectMemberCount = maxValue;
	}

	void OnPrintMaxStringLength(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -printMaxStringLength");

		if (!int.TryParse(option.Value, out var maxValue) || maxValue < 0)
			throw new ArgumentException("incorrect argument value for -printMaxStringLength");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.PrintMaxStringLength = maxValue;
	}

	void OnReporter(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -reporter");

		if (Project.HasRunnerReporter)
			throw new ArgumentException("cannot specify more than one reporter");

		var switchedReporters = RunnerReporters.Where(r => !string.IsNullOrWhiteSpace(r.RunnerSwitch)).ToList();
		if (switchedReporters.Count == 0)
			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"unknown reporter '{0}' (there are no registered reporters with switches)",
					option.Value
				)
			);

		Project.RunnerReporter =
			switchedReporters.FirstOrDefault(r => option.Value.Equals(r.RunnerSwitch, StringComparison.OrdinalIgnoreCase))
				?? throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						"unknown reporter '{0}' (must be one of: {1})",
						option.Value,
						string.Join(", ", switchedReporters.OrderBy(r => r.RunnerSwitch).Select(r => "'" + r.RunnerSwitch + "'"))
					)
				);
	}

	void OnShowLiveOutput(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.ShowLiveOutput = true;
	}

	void OnStopOnFail(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.StopOnFail = true;
	}

	void OnTrait(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -trait");

		var pieces = option.Value.Split('=');
		if (pieces.Length != 2 || string.IsNullOrEmpty(pieces[0]) || string.IsNullOrEmpty(pieces[1]))
			throw new ArgumentException("incorrect argument format for -trait (should be \"name=value\")");

		var name = pieces[0];
		var value = pieces[1];

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.AddIncludedTraitFilter(name, value);
	}

	void OnTraitMinus(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
#pragma warning disable CA1308  // This is for UI purposes, not normalization purposes
			throw new ArgumentException("missing argument for " + option.Key.ToLowerInvariant());
#pragma warning restore CA1308

		var pieces = option.Value.Split('=');
		if (pieces.Length != 2 || string.IsNullOrEmpty(pieces[0]) || string.IsNullOrEmpty(pieces[1]))
			throw new ArgumentException("incorrect argument format for -trait- (should be \"name=value\")");

		var name = pieces[0];
		var value = pieces[1];

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.AddExcludedTraitFilter(name, value);
	}

	void OnUseAnsiColor(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		Project.Configuration.UseAnsiColor = true;
	}

	/// <summary/>
	protected void OnWait(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		Project.Configuration.Wait = true;
	}

	/// <summary/>
	protected XunitProject ParseInternal(int argStartIndex)
	{
		var arguments = new Stack<string>();

		for (var i = Args.Count - 1; i >= argStartIndex; i--)
			arguments.Push(Args[i]);

		while (arguments.Count > 0)
		{
			var option = PopOption(arguments);
			var optionName = option.Key;

			if (!optionName.StartsWith("-", StringComparison.Ordinal))
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "unknown option: {0}", option.Key));

			optionName = optionName.Substring(1);

			if (parsers.TryGetValue(optionName, out var parser))
				parser.Handler(option);
			else
			{
				// Might be a result output file...
				if (TransformFactory.AvailableTransforms.Any(t => t.ID.Equals(optionName, StringComparison.OrdinalIgnoreCase)))
				{
					if (option.Value is null)
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "missing filename for {0}", option.Key));

					EnsurePathExists(option.Value);

					Project.Configuration.Output.Add(optionName, option.Value);
				}
				else
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "unknown option: {0}", option.Key));
			}
		}

		var autoReporter =
			Project.Configuration.NoAutoReportersOrDefault
				? null
				: RunnerReporters.FirstOrDefault(r => r.IsEnvironmentallyEnabled);

		if (autoReporter is not null)
			Project.RunnerReporter = autoReporter;

		if (!Project.HasRunnerReporter)
			Project.RunnerReporter =
				RunnerReporters.FirstOrDefault(r => "default".Equals(r.RunnerSwitch, StringComparison.OrdinalIgnoreCase))
					?? new DefaultRunnerReporter();

		return Project;
	}

	static KeyValuePair<string, string?> PopOption(Stack<string> arguments)
	{
		var option = arguments.Pop();
		string? value = null;

		if (arguments.Count > 0 && (option.Equals("-run", StringComparison.OrdinalIgnoreCase) || !arguments.Peek().StartsWith("-", StringComparison.Ordinal)))
			value = arguments.Pop();

		return new KeyValuePair<string, string?>(option, value);
	}

	/// <summary/>
	public void PrintUsage()
	{
		var isInProcessRunner = GetType().Namespace == "Xunit.Runner.InProc.SystemConsole";

		PrintUsageGroup(CommandLineGroup.General, "General options");
		PrintUsageGroup(CommandLineGroup.NetFramework, "Options for .NET Framework projects (v1 or v2 only)");
		PrintUsageGroup(CommandLineGroup.FilterQuery, "Query filtering (optional, choose one or more)", "If more than one query filter is specified, the filters act as an OR operation", "  Note: You cannot mix simple filtering and query filtering.");
		PrintUsageGroup(CommandLineGroup.FilterSimple, "Simple filtering (optional, choose one or more)", "If more than one simple filter type is specified, cross-filter type filters act as an AND operation", "  Note: You cannot mix simple filtering and query filtering.");
		PrintUsageGroup(CommandLineGroup.ArgumentDisplay, "Argument display overrides" + (isInProcessRunner ? string.Empty : " (v3 1.1.0+ only)"));

		if (RunnerReporters.Count > 0)
		{
			var switchableRunners = RunnerReporters.Where(r => !string.IsNullOrWhiteSpace(r.RunnerSwitch)).OrderBy(r => r.RunnerSwitch).ToList();

			ConsoleHelper.WriteLine();
			ConsoleHelper.WriteLine("Runner reporters ({0})", switchableRunners.Count != 0 ? "optional, choose only one" : "environmentally enabled");

			if (switchableRunners.Count != 0)
			{
				var longestSwitch = switchableRunners.Max(r => r.RunnerSwitch!.Length);

				ConsoleHelper.WriteLine();
				ConsoleHelper.WriteLine("  -reporter <option> : choose a reporter");

				foreach (var switchableReporter in switchableRunners)
					ConsoleHelper.WriteLine(
						"                     :   {0} - {1}{2}",
						switchableReporter.RunnerSwitch!.PadRight(longestSwitch),
						switchableReporter.Description,
						switchableReporter.ForceNoLogo ? " [implies '-noLogo']" : string.Empty
					);
			}

			var environmentalRunners = RunnerReporters.Where(r => r.CanBeEnvironmentallyEnabled).OrderBy(r => r.Description).ToList();
			if (environmentalRunners.Count != 0)
			{
				ConsoleHelper.WriteLine();
				ConsoleHelper.WriteLine("  The following reporters will be automatically enabled in the appropriate environment.");
				ConsoleHelper.WriteLine("  An automatically enabled reporter will override a manually selected reporter.");
				ConsoleHelper.WriteLine("    Note: You can disable auto-enabled reporters by specifying the '-noAutoReporters' switch");
				ConsoleHelper.WriteLine();

				foreach (var environmentalReporter in environmentalRunners)
					ConsoleHelper.WriteLine("    * {0}", environmentalReporter.Description);
			}
		}

		if (TransformFactory.AvailableTransforms.Count != 0)
		{
			ConsoleHelper.WriteLine();
			ConsoleHelper.WriteLine("Result formats (optional, choose one or more)");
			ConsoleHelper.WriteLine();

			var longestTransform = TransformFactory.AvailableTransforms.Max(t => t.ID.Length);
			foreach (var transform in TransformFactory.AvailableTransforms.OrderBy(t => t.ID))
				ConsoleHelper.WriteLine("  -{0} : {1}", string.Format(CultureInfo.CurrentCulture, "{0} <filename>", transform.ID).PadRight(longestTransform + 11), transform.Description);
		}
	}

	void PrintUsageGroup(
		CommandLineGroup group,
		params string[] headers)
	{
		var options =
			parsers
				.Where(p => p.Value.Group == group)
				.OrderBy(p => p.Key)
				.Select(p => (@switch: string.Format(CultureInfo.CurrentCulture, "-{0} {1}", p.Key, p.Value.ArgumentDisplay).Trim(), descriptions: p.Value.Descriptions))
				.ToList();

		if (options.Count == 0)
			return;

		var longestSwitch = options.Max(o => o.@switch.Length);
		var padding = "".PadRight(longestSwitch);

		ConsoleHelper.WriteLine();

		foreach (var header in headers)
			ConsoleHelper.WriteLine(header);

		ConsoleHelper.WriteLine();

		foreach (var (@switch, descriptions) in options)
		{
			ConsoleHelper.WriteLine("  {0} : {1}", @switch.PadRight(longestSwitch), descriptions[0]);
			for (var idx = 1; idx < descriptions.Length; ++idx)
				ConsoleHelper.WriteLine("  {0} : {1}", padding, descriptions[idx]);
		}
	}
}
