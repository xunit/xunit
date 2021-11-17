using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Runner.Common;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public abstract class CommandLineParserBase
{
	readonly Dictionary<string, (CommandLineGroup Group, string? ArgumentDisplay, string[] Descriptions, Action<KeyValuePair<string, string?>> Handler)> parsers = new();
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
			"run tests under the given culture",
			"  default   - run with default operating system culture",
			"  invariant - run with the invariant culture",
			"  (string)  - run with the given culture (f.e., 'en-US')"
		);
		AddParser("debug", OnDebug, CommandLineGroup.General, null, "launch the debugger to debug the tests");
		AddParser("diagnostics", OnDiagnostics, CommandLineGroup.General, null, "enable diagnostics messages for all test assemblies");
		AddParser("failskips", OnFailSkips, CommandLineGroup.General, null, "convert skipped tests into failures");
		AddParser("ignorefailures", OnIgnoreFailures, CommandLineGroup.General, null, "if tests fail, do not return a failure exit code");
		AddParser("internaldiagnostics", OnInternalDiagnostics, CommandLineGroup.General, null, "enable internal diagnostics messages for all test assemblies");
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
			"maxthreads", OnMaxThreads, CommandLineGroup.General, "<option>",
			"maximum thread count for collection parallelization",
			"  default   - run with default (1 thread per CPU thread)",
			"  unlimited - run with unbounded thread count",
			"  (integer) - use exactly this many threads (f.e., '2' = 2 threads)",
			"  (float)x  - use a multiple of CPU threads (f.e., '2.0x' = 2.0 * the number of CPU threads)"
		);
		AddParser(
			"noautoreporters", OnNoAutoReporters, CommandLineGroup.General, "<option>",
			"do not allow reporters to be auto-enabled by environment",
			"(for example, auto-detecting TeamCity or AppVeyor)"
		);
		AddParser("nocolor", OnNoColor, CommandLineGroup.General, null, "do not output results with colors");
		AddParser("nologo", OnNoLogo, CommandLineGroup.General, null, "do not show the copyright message");
		AddParser("pause", OnPause, CommandLineGroup.General, null, "wait for input before running tests");
		AddParser("preenumeratetheories", OnPreEnumerateTheories, CommandLineGroup.General, null, "enable theory pre-enumeration (disabled by default)");
		AddParser("stoponfail", OnStopOnFail, CommandLineGroup.General, null, "stop on first test failure");
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

		// Deprecated options
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
			if (runnerReporters == null)
				runnerReporters = GetAvailableRunnerReporters();

			return runnerReporters;
		}
	}

	/// <summary/>
	protected void AddHiddenParser(
		string @switch,
		Action<KeyValuePair<string, string?>> handler,
		string? replacement = null) =>
			parsers[@switch] = (CommandLineGroup.Hidden, null, replacement == null ? Array.Empty<string>() : new[] { replacement }, handler);

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
		var result = new List<IRunnerReporter>();

		if (string.IsNullOrWhiteSpace(reporterFolder))
			return result;

		// TODO: We shouldn't just load every DLL, this could cause a lot of problems (not just performance,
		// but also the fact that some things may not be loadable from here). What's the best strategy? Demand
		// a filename match? Use Cecil to see if there's an assembly-level attribute indicating reporters are
		// present? Something besides this...
		foreach (var dllFile in Directory.GetFiles(reporterFolder, "*.dll").Select(f => Path.Combine(reporterFolder, f)))
		{
			Type?[] types;

			try
			{
				var assembly = LoadAssembly(dllFile);
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				types = ex.Types;
			}
			catch
			{
				continue;
			}

			foreach (var type in types)
			{
				if (type == null || type.IsAbstract || type.GetCustomAttribute<HiddenRunnerReporterAttribute>() != null || !type.GetInterfaces().Any(t => t == typeof(IRunnerReporter)))
					continue;

				var ctor = type.GetConstructor(new Type[0]);
				if (ctor == null)
				{
					if (!Project.Configuration.NoColorOrDefault)
						ConsoleHelper.SetForegroundColor(ConsoleColor.Yellow);

					Console.WriteLine($"Type {type.FullName} in assembly {dllFile} appears to be a runner reporter, but does not have an empty constructor.");

					if (!Project.Configuration.NoColorOrDefault)
						ConsoleHelper.ResetColor();

					continue;
				}

				result.Add((IRunnerReporter)ctor.Invoke(new object[0]));
			}
		}

		return result;
	}

	/// <summary/>
	[return: NotNullIfNotNull("fileName")]
	protected virtual string? GetFullPath(string? fileName) =>
		fileName == null ? null : Path.GetFullPath(fileName);

	/// <summary/>
	protected static void GuardNoOptionValue(KeyValuePair<string, string?> option)
	{
		if (option.Value != null)
			throw new ArgumentException($"error: unknown command line option: {option.Value}");
	}

	/// <summary/>
	protected static bool IsConfigFile(string fileName) =>
		fileName.EndsWith(".config", StringComparison.OrdinalIgnoreCase) ||
		fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);

	/// <summary/>
	protected abstract Assembly LoadAssembly(string dllFile);

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
			var optionName = option.Key.ToLowerInvariant();

			if (!optionName.StartsWith("-", StringComparison.Ordinal))
				throw new ArgumentException($"unknown option: {option.Key}");

			optionName = optionName.Substring(1);

			if (parsers.TryGetValue(optionName, out var parser))
				parser.Handler(option);
			else
			{
				// Might be a result output file...
				if (TransformFactory.AvailableTransforms.Any(t => t.ID.Equals(optionName, StringComparison.OrdinalIgnoreCase)))
				{
					if (option.Value == null)
						throw new ArgumentException($"missing filename for {option.Key}");

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
			var reporter = runnerReporters.FirstOrDefault(r => r.RunnerSwitch == unknownOption) ?? throw new ArgumentException($"unknown option: -{unknownOption}");

			if (runnerReporter != null)
				throw new ArgumentException("only one reporter is allowed");

			runnerReporter = reporter;
		}

		Project.RunnerReporter = autoReporter ?? runnerReporter ?? new DefaultRunnerReporter();

		return Project;
	}

	void OnClass(KeyValuePair<string, string?> option)
	{
		if (option.Value == null)
			throw new ArgumentException("missing argument for -class");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.IncludedClasses.Add(option.Value);
	}

	void OnClassMinus(KeyValuePair<string, string?> option)
	{
		if (option.Value == null)
			throw new ArgumentException("missing argument for -noclass");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.ExcludedClasses.Add(option.Value);
	}

	void OnCulture(KeyValuePair<string, string?> option)
	{
		if (option.Value == null)
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

	void OnFailSkips(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.FailSkips = true;
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
		if (option.Value == null)
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
		if (option.Value == null)
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
				if (match.Success && decimal.TryParse(match.Groups[1].Value, out var maxThreadMultiplier))
					maxParallelThreads = (int)(maxThreadMultiplier * Environment.ProcessorCount);
				else if (int.TryParse(option.Value, out var threadValue) && threadValue > 0)
					maxParallelThreads = threadValue;
				else
					throw new ArgumentException("incorrect argument value for -maxthreads (must be 'default', 'unlimited', a positive number, or a multiplier in the form of '0.0x')");

				break;
		}

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.MaxParallelThreads = maxParallelThreads;
	}

	void OnMethod(KeyValuePair<string, string?> option)
	{
		if (option.Value == null)
			throw new ArgumentException("missing argument for -method");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.IncludedMethods.Add(option.Value);
	}

	void OnMethodMinus(KeyValuePair<string, string?> option)
	{
		if (option.Value == null)
			throw new ArgumentException("missing argument for -nomethod");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.ExcludedMethods.Add(option.Value);
	}

	void OnNamespace(KeyValuePair<string, string?> option)
	{
		if (option.Value == null)
			throw new ArgumentException("missing argument for -namespace");

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.Filters.IncludedNamespaces.Add(option.Value);
	}

	void OnNamespaceMinus(KeyValuePair<string, string?> option)
	{
		if (option.Value == null)
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
	}

	void OnNoLogo(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		Project.Configuration.NoLogo = true;
	}

	/// <summary/>
	protected void OnParallel(KeyValuePair<string, string?> option)
	{
		if (option.Value == null)
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
		if (option.Value == null)
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
		if (option.Value == null)
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
				Console.WriteLine($"  -{switchableReporter.RunnerSwitch!.ToLowerInvariant().PadRight(longestSwitch)} : {switchableReporter.Description}");

			foreach (var environmentalReporter in RunnerReporters.Where(r => string.IsNullOrWhiteSpace(r.RunnerSwitch)).OrderBy(r => r.Description))
				Console.WriteLine($"   {"".PadRight(longestSwitch)} : {environmentalReporter.Description} [auto-enabled only]");
		}

		if (TransformFactory.AvailableTransforms.Count != 0)
		{
			Console.WriteLine();
			Console.WriteLine("Result formats (optional, choose one or more)");
			Console.WriteLine();

			var longestTransform = TransformFactory.AvailableTransforms.Max(t => t.ID.Length);
			foreach (var transform in TransformFactory.AvailableTransforms.OrderBy(t => t.ID))
				Console.WriteLine($"  -{$"{transform.ID} <filename>".PadRight(longestTransform + 11)} : {transform.Description}");
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
				.Select(p => (@switch: $"-{p.Key} {p.Value.ArgumentDisplay}".Trim(), descriptions: p.Value.Descriptions))
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
			Console.WriteLine($"  {@switch.PadRight(longestSwitch)} : {descriptions[0]}");
			for (int idx = 1; idx < descriptions.Length; ++idx)
				Console.WriteLine($"  {padding} : {descriptions[idx]}");
		}
	}
}
