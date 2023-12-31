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
		IReadOnlyList<IRunnerReporter>? runnerReporters,
		string? reporterFolder,
		string[] args)
	{
		this.runnerReporters = runnerReporters;
		this.reporterFolder = reporterFolder;
		Args = args;

		if (string.IsNullOrWhiteSpace(this.reporterFolder))
			this.reporterFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

		// General options
		AddParser(
			"culture", OnCulture, CommandLineGroup.General, "<option>",
			"run tests under the given culture (v3 assemblies only)",
			"note: when running a v1/v2 assembly, the culture option will be ignored",
			"  default   - run with default operating system culture",
			"  invariant - run with the invariant culture",
			"  (string)  - run with the given culture (f.e., 'en-US')"
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
		AddParser("failSkips-", OnFailSkipsMinus, CommandLineGroup.General, null, "treat skipped tests as skipped (default)");
		AddParser("failWarns", OnFailWarns, CommandLineGroup.General, null, "treat passing tests with warnings as failures");
		AddParser("failWarns-", OnFailWarnsMinus, CommandLineGroup.General, null, "treat passing tests with warnings as successful (default)");
		AddParser("ignoreFailures", OnIgnoreFailures, CommandLineGroup.General, null, "if tests fail, do not return a failure exit code");
		AddParser("internalDiagnostics", OnInternalDiagnostics, CommandLineGroup.General, null, "enable internal diagnostics messages for all test assemblies");
		AddParser(
			"list", OnList, CommandLineGroup.General, "<option>",
			"list information about the test assemblies rather than running tests (implies -nologo)",
			"note: you can add '/json' to the end of any option to get the listing in JSON format",
			"  classes - list class names of every class which contains tests",
			"  full    - list complete discovery data",
			"  methods - list class+method names of every method which is a test",
			"  tests   - list just the display name of all tests",
			"  traits  - list the set of trait name/value pairs used in the test assemblies"
		);
		AddParser(
			"maxThreads", OnMaxThreads, CommandLineGroup.General, "<option>",
			"maximum thread count for collection parallelization",
			"  default   - run with default (1 thread per CPU thread)",
			"  unlimited - run with unbounded thread count",
			"  (integer) - use exactly this many threads (f.e., '2' = 2 threads)",
			"  (float)x  - use a multiple of CPU threads (f.e., '2.0x' = 2.0 * the number of CPU threads)"
		);
		AddParser(
			"noAutoReporters", OnNoAutoReporters, CommandLineGroup.General, "<option>",
			"do not allow reporters to be auto-enabled by environment",
			"(for example, auto-detecting TeamCity or AppVeyor)"
		);
		AddParser("noColor", OnNoColor, CommandLineGroup.General, null, "do not output results with colors");
		AddParser("noLogo", OnNoLogo, CommandLineGroup.General, null, "do not show the copyright message");
		AddParser("pause", OnPause, CommandLineGroup.General, null, "wait for input before running tests");
		AddParser("preEnumerateTheories", OnPreEnumerateTheories, CommandLineGroup.General, null, "enable theory pre-enumeration (disabled by default)");
		AddParser("stopOnFail", OnStopOnFail, CommandLineGroup.General, null, "stop on first test failure");
		AddParser("wait", OnWait, CommandLineGroup.General, null, "wait for input after completion");

		// Filter options
		AddParser(
			"class", OnClass, CommandLineGroup.Filter, "\"name\"",
			"run all methods in a given test class (should be fully",
			"specified; i.e., 'MyNamespace.MyClass')",
			"  if specified more than once, acts as an OR operation"
		);
		AddParser(
			"class-", OnClassMinus, CommandLineGroup.Filter, "\"name\"",
			"do not run any methods in a given test class (should be fully",
			"specified; i.e., 'MyNamespace.MyClass')",
			"  if specified more than once, acts as an AND operation"
		);
		AddParser(
			"method", OnMethod, CommandLineGroup.Filter, "\"name\"",
			"run a given test method (can be fully specified or use a wildcard;",
			"i.e., 'MyNamespace.MyClass.MyTestMethod' or '*.MyTestMethod')",
			"  if specified more than once, acts as an OR operation"
		);
		AddParser(
			"method-", OnMethodMinus, CommandLineGroup.Filter, "\"name\"",
			"do not run a given test method (can be fully specified or use a wildcard;",
			"i.e., 'MyNamespace.MyClass.MyTestMethod' or '*.MyTestMethod')",
			"  if specified more than once, acts as an AND operation"
		);
		AddParser(
			"namespace", OnNamespace, CommandLineGroup.Filter, "\"name\"",
			"run all methods in a given namespace (i.e.,",
			"'MyNamespace.MySubNamespace')",
			"  if specified more than once, acts as an OR operation"
		);
		AddParser(
			"namespace-", OnNamespaceMinus, CommandLineGroup.Filter, "\"name\"",
			"do not run any methods in a given namespace (i.e.,",
			"'MyNamespace.MySubNamespace')",
			"  if specified more than once, acts as an AND operation"
		);
		AddParser(
			"trait", OnTrait, CommandLineGroup.Filter, "\"name=value\"",
			"only run tests with matching name/value traits",
			"  if specified more than once, acts as an OR operation"
		);
		AddParser(
			"trait-", OnTraitMinus, CommandLineGroup.Filter, "\"name=value\"",
			"do not run tests with matching name/value traits",
			"  if specified more than once, acts as an AND operation"
		);

		// Deprecated/hidden options
		AddHiddenParser("noclass", OnClassMinus);
		AddHiddenParser("nomethod", OnMethodMinus);
		AddHiddenParser("nonamespace", OnNamespaceMinus);
		AddHiddenParser("notrait", OnTraitMinus);
	}

	/// <summary/>
	protected string[] Args { get; }

	/// <summary/>
	public bool HelpRequested =>
		Args.Length > 0 && (Args[0] == "-?" || Args[0] == "/?" || Args[0] == "-h" || Args[0] == "--help");

	/// <summary/>
	protected XunitProject Project { get; } = new();

	/// <summary/>
	protected IReadOnlyList<IRunnerReporter> RunnerReporters
	{
		get
		{
			if (runnerReporters is null)
				runnerReporters = GetAvailableRunnerReporters();

			return runnerReporters;
		}
	}

	/// <summary/>
	protected void AddHiddenParser(
		string @switch,
		Action<KeyValuePair<string, string?>> handler,
		string? replacement = null) =>
			parsers[@switch] = (CommandLineGroup.Hidden, null, replacement is null ? Array.Empty<string>() : new[] { replacement }, handler);

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

	List<IRunnerReporter> GetAvailableRunnerReporters()
	{
		if (string.IsNullOrWhiteSpace(reporterFolder))
			return new List<IRunnerReporter>();

		var result = RunnerReporterUtility.GetAvailableRunnerReporters(reporterFolder, includeEmbeddedReporters: true, out var messages);

		if (messages.Count != 0)
		{
			if (!Project.Configuration.NoColorOrDefault)
				ConsoleHelper.SetForegroundColor(ConsoleColor.Yellow);

			foreach (var message in messages)
				Console.WriteLine(message);

			if (!Project.Configuration.NoColorOrDefault)
				ConsoleHelper.ResetColor();
		}

		return result;
	}

	/// <summary/>
	[return: NotNullIfNotNull("fileName")]
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

	void OnClass(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -class");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.IncludedClasses.Add(option.Value);
	}

	void OnClassMinus(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -noclass");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.ExcludedClasses.Add(option.Value);
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
			projectAssembly.Configuration.FailWarns = true;
	}

	void OnFailWarnsMinus(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.FailWarns = false;
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
		Project.Configuration.NoLogo = true;
	}

	void OnMaxThreads(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -maxthreads");

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
				if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var maxThreadMultiplier))
					maxParallelThreads = (int)(maxThreadMultiplier * Environment.ProcessorCount);
				else if (int.TryParse(option.Value, out var threadValue) && threadValue > 0)
					maxParallelThreads = threadValue;
				else
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "incorrect argument value for -maxthreads (must be 'default', 'unlimited', a positive number, or a multiplier in the form of '{0}x')", 0.0m));

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
			projectAssembly.Configuration.Filters.IncludedMethods.Add(option.Value);
	}

	void OnMethodMinus(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -nomethod");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.ExcludedMethods.Add(option.Value);
	}

	void OnNamespace(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -namespace");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.IncludedNamespaces.Add(option.Value);
	}

	void OnNamespaceMinus(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -nonamespace");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.ExcludedNamespaces.Add(option.Value);
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

	void OnPause(KeyValuePair<string, string?> option)
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
			projectAssembly.Configuration.Filters.IncludedTraits.Add(name, value);
	}

	void OnTraitMinus(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -notrait");

		var pieces = option.Value.Split('=');
		if (pieces.Length != 2 || string.IsNullOrEmpty(pieces[0]) || string.IsNullOrEmpty(pieces[1]))
			throw new ArgumentException("incorrect argument format for -notrait (should be \"name=value\")");

		var name = pieces[0];
		var value = pieces[1];

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.ExcludedTraits.Add(name, value);
	}

	void OnWait(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		Project.Configuration.Wait = true;
	}

	/// <summary/>
	protected XunitProject ParseInternal(int argStartIndex)
	{
		var arguments = new Stack<string>();
		var unknownOptions = new List<string>();

		for (var i = Args.Length - 1; i >= argStartIndex; i--)
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
				// ...or it might be a reporter (we won't know until later)
				else
				{
					GuardNoOptionValue(option);
					unknownOptions.Add(optionName);
				}
			}
		}

		// Determine the runner reporter while validating the unknown parsed options
		runnerReporters ??= GetAvailableRunnerReporters();

		var runnerReporter = default(IRunnerReporter);
		var autoReporter =
			Project.Configuration.NoAutoReportersOrDefault
				? null
				: runnerReporters.FirstOrDefault(r => r.IsEnvironmentallyEnabled);

		foreach (var unknownOption in unknownOptions)
		{
			var reporter = runnerReporters.FirstOrDefault(r => unknownOption.Equals(r.RunnerSwitch, StringComparison.OrdinalIgnoreCase)) ?? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "unknown option: -{0}", unknownOption));

			if (runnerReporter is not null)
				throw new ArgumentException("only one reporter is allowed");

			runnerReporter = reporter;
		}

		Project.RunnerReporter = autoReporter ?? runnerReporter ?? new DefaultRunnerReporter();

		return Project;
	}

	static KeyValuePair<string, string?> PopOption(Stack<string> arguments)
	{
		var option = arguments.Pop();
		string? value = null;

		if (arguments.Count > 0 && !arguments.Peek().StartsWith("-", StringComparison.Ordinal))
			value = arguments.Pop();

		return new KeyValuePair<string, string?>(option, value);
	}

	/// <summary/>
	public void PrintUsage()
	{
		PrintUsageGroup(CommandLineGroup.General, "General options");
		PrintUsageGroup(CommandLineGroup.NetFramework, "Options for .NET Framework projects (v1 or v2 only)");
		PrintUsageGroup(CommandLineGroup.Filter, "Filtering (optional, choose one or more)", "If more than one filter type is specified, cross-filter type filters act as an AND operation");

		if (RunnerReporters.Count > 0)
		{
			Console.WriteLine();
			Console.WriteLine("Reporters (optional, choose only one)");
			Console.WriteLine();

			var longestSwitch = RunnerReporters.Max(r => r.RunnerSwitch?.Length ?? 0);

			foreach (var switchableReporter in RunnerReporters.Where(r => !string.IsNullOrWhiteSpace(r.RunnerSwitch)).OrderBy(r => r.RunnerSwitch))
				Console.WriteLine("  -{0} : {1}", switchableReporter.RunnerSwitch!.PadRight(longestSwitch), switchableReporter.Description);

			foreach (var environmentalReporter in RunnerReporters.Where(r => string.IsNullOrWhiteSpace(r.RunnerSwitch)).OrderBy(r => r.Description))
				Console.WriteLine("   {0} : {1} [auto-enabled only]", "".PadRight(longestSwitch), environmentalReporter.Description);
		}

		if (TransformFactory.AvailableTransforms.Count != 0)
		{
			Console.WriteLine();
			Console.WriteLine("Result formats (optional, choose one or more)");
			Console.WriteLine();

			var longestTransform = TransformFactory.AvailableTransforms.Max(t => t.ID.Length);
			foreach (var transform in TransformFactory.AvailableTransforms.OrderBy(t => t.ID))
				Console.WriteLine("  -{0} : {1}", string.Format(CultureInfo.CurrentCulture, "{0} <filename>", transform.ID).PadRight(longestTransform + 11), transform.Description);
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

		Console.WriteLine();

		foreach (var header in headers)
			Console.WriteLine(header);

		Console.WriteLine();

		foreach (var (@switch, descriptions) in options)
		{
			Console.WriteLine("  {0} : {1}", @switch.PadRight(longestSwitch), descriptions[0]);
			for (int idx = 1; idx < descriptions.Length; ++idx)
				Console.WriteLine("  {0} : {1}", padding, descriptions[idx]);
		}
	}
}
