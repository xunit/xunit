using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Configurations;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.CommandLine;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

/// <summary>
/// Implementation of <see cref="ICommandLineOptionsProvider"/> for xUnit.net v3.
/// </summary>
/// <remarks>
/// This class is an implementation detail for Microsoft.Testing.Platform that is public for testing purposes.
/// Use this class at your own risk, as breaking changes may occur as needed.
/// </remarks>
public sealed class CommandLineOptionsProvider() :
	ExtensionBase("command line options provider", "7e0a6fd0-3615-48b0-859c-6bb4f51c3095"), ICommandLineOptionsProvider
{
	static readonly Dictionary<string, (string Description, ArgumentArity Arity, Action<ParseOptions> Parse)> options = new(StringComparer.OrdinalIgnoreCase)
	{
		// General options
		{ "assert-equivalent-max-depth", ($"""
			Set the maximum recursive depth when comparing objects with Assert.Equivalent. Default value is {EnvironmentVariables.Defaults.AssertEquivalentMaxDepth}.
			    (integer) - maximum depth to compare; exceeding this fails the assertion
			""", ArgumentArity.ExactlyOne, options => OnIntValueWithMinimum(options, 1, value => options.AssemblyConfig.AssertEquivalentMaxDepth = value)) },
		{ "culture", ("""
			Run tests under the given culture.
			    default   - run with the default operating system culture [default]
			    invariant - run with the invariant culture
			    (string)  - run with the given culture (i.e., 'en-US')
			""", ArgumentArity.ExactlyOne, OnCulture) },
		{ "explicit", ("""
			Change the way explicit tests are handled.
			    on   - run both explicit and non-explicit tests
			    off  - run only non-explicit tests [default]
			    only - run only explicit tests
			""", ArgumentArity.ExactlyOne, OnExplicit) },
		{ "fail-skips", ("""
			Change the way skipped tests are handled.
			    on  - treat skipped tests as failed
			    off - treat skipped tests as skipped [default]
			""", ArgumentArity.ExactlyOne, OnFailSkips) },
		{ "fail-warns", ("""
			Change the way passing tests with warnings are handled.
			    on  - treat passing tests with warnings as failed
			    off - treat passing tests with warnings as passed [default]
			""", ArgumentArity.ExactlyOne, OnFailWarns) },
		{ "long-running", ("""
			Enable long running (hung) test detection.
			    (integer) - number of seconds a test runs to be considered 'long running'
			""", ArgumentArity.ExactlyOne, options => OnIntValueWithMinimum(options, 0, value => options.AssemblyConfig.LongRunningTestSeconds = value)) },
		{ "max-threads", ("""
			Set maximum thread count for collection parallelization.
			    default   - run with default (1 thread per CPU thread)
			    unlimited - run with unbounded thread count
			    (integer) - use exactly this many threads (e.g., '2' = 2 threads)
			    (float)x  - use a multiple of CPU threads (e.g., '2.0x' = 2.0 * the number of CPU threads)
			""", ArgumentArity.ExactlyOne, OnMaxThreads) },
		{ "method-display", ("""
			Set default test display name.
			    classAndMethod - use a fully qualified name [default]
			    method         - use just the method name
			""", ArgumentArity.ExactlyOne, OnMethodDisplay) },
		{ "method-display-options", ("""
			Alters the default test display name.
			    none - apply no alterations [default]
			    all  - apply all alterations
			    Or one or more of:
			        replacePeriodWithComma     - replace periods in names with commas
			        replaceUnderscoreWithSpace - replace underscores in names with spaces
			        useOperatorMonikers        - replace operator names with operators
			                                         'lt' becomes '<'
			                                         'le' becomes '<='
			                                         'eq' becomes '='
			                                         'ne' becomes '!='
			                                         'gt' becomes '>'
			                                         'ge' becomes '>='
			        useEscapeSequences         - replace ASCII and Unicode escape sequences
			                                         X + 2 hex digits (i.e., 'X2C' becomes ',')
			                                         U + 4 hex digits (i.e., 'U0192' becomes 'ƒ')
			""", ArgumentArity.OneOrMore, OnMethodDisplayOptions) },
		{ "parallel", ("""
			Change test parallelization.
			    none        - turn off parallelization
			    collections - parallelize by collections [default]
			""", ArgumentArity.ExactlyOne, OnParallel) },
		{ "parallel-algorithm", ("""
			Change the parallelization algorithm.
			    conservative - start the minimum number of tests [default]
			    aggressive   - start as many tests as possible
			""", ArgumentArity.ExactlyOne, OnParallelAlgorithm) },
		{ "pre-enumerate-theories", ("""
			Change theory pre-enumeration during discovery.
			    on  - turns on theory pre-enumeration [default]
			    off - turns off theory pre-enumeration
			""", ArgumentArity.ExactlyOne, OnPreEnumerateTheories) },
		{ "seed", ("""
			Set the randomization seed.
			    (integer) - use this as the randomization seed
			""", ArgumentArity.ExactlyOne, options => OnIntValueWithMinimum(options, 0, value => options.AssemblyConfig.Seed = value)) },
		{ "show-live-output", ("""
			Determine whether to show test output (from ITestOutputHelper) live during test execution.
			    on  - turn on live reporting of test output
			    off - turn off live reporting of test output [default]
			""", ArgumentArity.ExactlyOne, OnShowLiveOutput) },
		{ "stop-on-fail", ("""
			Stop running tests after the first test failure.
			    on  - stop running tests after the first test failure
			    off - run all tests regardless of failures [default]
			""", ArgumentArity.ExactlyOne, OnStopOnFail) },
		{ "xunit-diagnostics", ("""
			Determine whether to show diagnostic messages.
			    on  - display diagnostic messages
			    off - hide diagnostic messages [default]
			""", ArgumentArity.ExactlyOne, OnDiagnostics) },
		{ "xunit-internal-diagnostics", ("""
			Determine whether to show internal diagnostic messages.
			    on  - display internal diagnostic messages
			    off - hide internal diagnostic messages [default]
			""", ArgumentArity.ExactlyOne, OnInternalDiagnostics) },

		// Query filtering
		{ "filter-query", ("""
			Filter based on the filter query lanaugage. Pass one or more filter queries (in the
			'/assemblyName/namespace/type/method[trait=value]' format. For more information, see
			https://xunit.net/docs/query-filter-language
			    Note: Specifying more than one is an OR operation.
			          This is categorized as a query filter. You cannot use both query filters and simple filters.
			""", ArgumentArity.OneOrMore, OnFilterQuery) },

		// Simple filtering
		{ "filter-class", ("""
			Run all methods in a given test class. Pass one or more fully qualified type names (i.e.,
			'MyNamespace.MyClass' or 'MyNamespace.MyClass+InnerClass'). Wildcard '*' is supported at
			the beginning and/or end of each filter.
			    Note: Specifying more than one is an OR operation.
			          This is categorized as a simple filter. You cannot use both simple filters and query filters.
			""", ArgumentArity.OneOrMore, options => OnFilter(options.Arguments, options.AssemblyConfig.Filters.AddIncludedClassFilter)) },
		{ "filter-not-class", ("""
			Do not run any methods in the given test class. Pass one or more fully qualified type names
			(i.e., 'MyNamspace.MyClass', or 'MyNamspace.MyClass+InnerClass'). Wildcard '*' is supported at
			the beginning and/or end of each filter.
			    Note: Specifying more than one is an AND operation.
			          This is categorized as a simple filter. You cannot use both simple filters and query filters.
			""", ArgumentArity.OneOrMore, options => OnFilter(options.Arguments, options.AssemblyConfig.Filters.AddExcludedClassFilter)) },
		{ "filter-method", ("""
			Run a given test method. Pass one or more fully qualified method names (i.e.,
			'MyNamespace.MyClass.MyTestMethod'). Wildcard '*' is supported at the beginning and/or end
			of each filter.
			    Note: Specifying more than one is an OR operation.
			          This is categorized as a simple filter. You cannot use both simple filters and query filters.
			""", ArgumentArity.OneOrMore, options => OnFilter(options.Arguments, options.AssemblyConfig.Filters.AddIncludedMethodFilter)) },
		{ "filter-not-method", ("""
			Do not run a given test method. Pass one or more fully qualified method names (i.e.,
			'MyNamspace.MyClass.MyTestMethod'). Wildcard '*' is supported at the beginning and/or end
			of each filter.
			    Note: Specifying more than one is an AND operation.
			          This is categorized as a simple filter. You cannot use both simple filters and query filters.
			""", ArgumentArity.OneOrMore, options => OnFilter(options.Arguments, options.AssemblyConfig.Filters.AddExcludedMethodFilter)) },
		{ "filter-namespace", ("""
			Run all methods in the given namespace. Pass one or more namespaces (i.e., 'MyNamespace' or
			'MyNamespace.MySubNamespace'). Wildcard '*' is supported at the beginning and/or end of
			each filter.
			    Note: Specifying more than one is an OR operation.
			          This is categorized as a simple filter. You cannot use both simple filters and query filters.
			""", ArgumentArity.OneOrMore, options => OnFilter(options.Arguments, options.AssemblyConfig.Filters.AddIncludedNamespaceFilter)) },
		{ "filter-not-namespace", ("""
			Do not run any methods in the given namespace. Pass one or more namespaces (i.e., 'MyNamespace'
			or 'MyNamespace.MySubNamespace'). Wildcard '*' is supported at the beginning and/or end of
			each filter.
			    Note: Specifying more than one is an AND operation.
			          This is categorized as a simple filter. You cannot use both simple filters and query filters.
			""", ArgumentArity.OneOrMore, options => OnFilter(options.Arguments, options.AssemblyConfig.Filters.AddExcludedNamespaceFilter)) },
		{ "filter-trait", ("""
			Run all methods with a given trait value. Pass one or more name/value pairs (i.e.,
			'name=value'). Wildcard '*' is supported at the beginning and/or end of the trait name
			and/or value.
			    Note: Specifying more than one is an OR operation.
			          This is categorized as a simple filter. You cannot use both simple filters and query filters.
			""", ArgumentArity.OneOrMore, options => OnFilterTrait(options.Arguments, options.AssemblyConfig.Filters.AddIncludedTraitFilter)) },
		{ "filter-not-trait", ("""
			Do not run any methods with a given trait value. Pass one or more name/value pairs (i.e.,
			'name=value'). Wildcard '*' is supported at the beginning and/or end of the trait name
			and/or value.
			    Note: Specifying more than one is an AND operation.
			          This is categorized as a simple filter. You cannot use both simple filters and query filters.
			""", ArgumentArity.OneOrMore, options => OnFilterTrait(options.Arguments, options.AssemblyConfig.Filters.AddExcludedTraitFilter)) },

		// Argument display options
		{ "print-max-enumerable-length", ($"""
			Set the maximum number of values to show when printing a collection. Default value is {EnvironmentVariables.Defaults.PrintMaxEnumerableLength}.
			    0         - always print the full collection
			    (integer) - maximum values to print, followed by an ellipsis
			""", ArgumentArity.ExactlyOne, options => OnIntValueWithMinimum(options, 0, value => options.AssemblyConfig.PrintMaxEnumerableLength = value)) },
		{ "print-max-object-depth", ($"""
			Set the maximum recursive depth when printing object values. Default value is {EnvironmentVariables.Defaults.PrintMaxObjectDepth}.
			    0         - print objects at all depths
			    (integer) - maximum depth to print, followed by an ellipsis
			Warning: Setting '0' or a very large value can cause stack overflows that may crash the test process
			""", ArgumentArity.ExactlyOne, options => OnIntValueWithMinimum(options, 0, value => options.AssemblyConfig.PrintMaxObjectDepth = value)) },
		{ "print-max-object-member-count", ($"""
			Set the maximum number of fields and properties to show when printing an object. Default value is {EnvironmentVariables.Defaults.PrintMaxObjectMemberCount}.
			    0         - always print the full collection
			    (integer) - maximum members to print, followed by an ellipsis
			""", ArgumentArity.ExactlyOne, options => OnIntValueWithMinimum(options, 0, value => options.AssemblyConfig.PrintMaxObjectMemberCount = value)) },
		{ "print-max-string-length", ($"""
			Set the maximum length when printing a string. Default value is {EnvironmentVariables.Defaults.PrintMaxStringLength}.
			    0         - always print the full collection
			    (integer) - maximum string length to print, followed by an ellipsis
			""", ArgumentArity.ExactlyOne, options => OnIntValueWithMinimum(options, 0, value => options.AssemblyConfig.PrintMaxStringLength = value)) },

		// Reports
		{ "report-ctrf", ("Enable generating CTRF (JSON) report", ArgumentArity.Zero, options => OnReport(options.Configuration, options.CommandLineOptions, "ctrf", "report-ctrf-filename", "ctrf", options.ProjectConfig)) },
		{ "report-ctrf-filename", ("The name of the generated CTRF report", ArgumentArity.ExactlyOne, OnReportFilename) },
		{ "report-junit", ("Enable generating JUnit (XML) report", ArgumentArity.Zero, options => OnReport(options.Configuration, options.CommandLineOptions, "junit", "report-junit-filename", "junit", options.ProjectConfig)) },
		{ "report-junit-filename", ("The name of the generated JUnit report", ArgumentArity.ExactlyOne, OnReportFilename) },
		{ "report-nunit", ("Enable generating NUnit (v2.5 XML) report", ArgumentArity.Zero, options => OnReport(options.Configuration, options.CommandLineOptions, "nunit", "report-nunit-filename", "nunit", options.ProjectConfig)) },
		{ "report-nunit-filename", ("The name of the generated NUnit report", ArgumentArity.ExactlyOne, OnReportFilename) },
		{ "report-xunit", ("Enable generating xUnit.net (v2+ XML) report", ArgumentArity.Zero, options => OnReport(options.Configuration, options.CommandLineOptions, "xml", "report-xunit-filename", "xunit", options.ProjectConfig)) },
		{ "report-xunit-filename", ("The name of the generated xUnit.net report", ArgumentArity.ExactlyOne, OnReportFilename) },
		{ "report-xunit-html", ("Enable generating xUnit.net HTML report", ArgumentArity.Zero, options => OnReport(options.Configuration, options.CommandLineOptions, "html", "report-xunit-html-filename", "html", options.ProjectConfig)) },
		{ "report-xunit-html-filename", ("The name of the generated xUnit.net HTML report", ArgumentArity.ExactlyOne, OnReportFilename) },
		{ "report-xunit-trx", ("Enable generating xUnit.net TRX report", ArgumentArity.Zero, options => OnReport(options.Configuration, options.CommandLineOptions, "trx", "report-xunit-trx-filename", "trx", options.ProjectConfig)) },
		{ "report-xunit-trx-filename", ("The name of the generated xUnit.net TRX report", ArgumentArity.ExactlyOne, OnReportFilename) },

		// Non-configuration options (read externally)
		{ "auto-reporters", (
			"""
			Change whether reporters can be auto-enabled.
			    on  - allow reporters to be auto-enabled by the environment [default]
			    off - do not allow reporters to be auto-enabled by the environment
			""", ArgumentArity.ExactlyOne, OnAutoReporters) },
		{ "xunit-config-filename", (
			"""
			Sets the configuration file. By default, this is 'xunit.runner.json' in the bin directory
			alongside the compiled output.
			""", ArgumentArity.ExactlyOne, OnConfigFilename) },
		{ "xunit-info", ("Show xUnit.net headers and information", ArgumentArity.Zero, NoOp) },
	};
	static readonly Dictionary<string, string> optionDependencies = new()
	{
		{ "report-ctrf-filename", "report-ctrf" },
		{ "report-junit-filename", "report-junit" },
		{ "report-nunit-filename", "report-nunit" },
		{ "report-xunit-filename", "report-xunit" },
		{ "report-xunit-html-filename", "report-xunit-html" },
		{ "report-xunit-trx-filename", "report-xunit-trx" },
	};
	// Match the format used by Microsoft.Testing.Extensions.TrxReport
	static readonly string reportFileNameRoot = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2:yyyy-MM-dd_HH_mm_ss.fff}.", Environment.UserName, Environment.MachineName, DateTimeOffset.UtcNow);

	/// <inheritdoc/>
	public IReadOnlyCollection<CommandLineOption> GetCommandLineOptions() =>
		options.Select(option => new CommandLineOption(option.Key, option.Value.Description, option.Value.Arity, isHidden: false)).ToArray();

	static void NoOp(ParseOptions options)
	{ }

	// We read the value outside of config, this is just here to validate
	static void OnAutoReporters(ParseOptions options) =>
		ParseOnOff(options.Arguments[0]);

	// We read the value outside of config, this is just here to validate
	static void OnConfigFilename(ParseOptions options)
	{
		var configFilename = Path.GetFullPath(options.Arguments[0]);

		if (!File.Exists(configFilename))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Config file '{0}' was not found", configFilename));
	}

	static void OnCulture(ParseOptions options)
	{
		var culture = options.Arguments[0];

		options.AssemblyConfig.Culture = culture.ToUpperInvariant() switch
		{
			"DEFAULT" => null,
			"INVARIANT" => string.Empty,
			_ => culture,
		};

		// Validate the provided culture; this isn't foolproof, since the system will accept random names, but it
		// will catch some simple cases like trying to pass a number as the culture
		if (!string.IsNullOrWhiteSpace(options.AssemblyConfig.Culture))
		{
			try
			{
				CultureInfo.GetCultureInfo(culture);
			}
			catch (CultureNotFoundException)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Invalid value '{0}'", culture));
			}
		}
	}

	static void OnDiagnostics(ParseOptions options) =>
		options.AssemblyConfig.DiagnosticMessages = ParseOnOff(options.Arguments[0]);

	static void OnExplicit(ParseOptions options) =>
		options.AssemblyConfig.ExplicitOption = ParseEnum<ExplicitOption>(options.Arguments[0]);

	static void OnFailSkips(ParseOptions options) =>
		options.AssemblyConfig.FailSkips = ParseOnOff(options.Arguments[0]);

	static void OnFailWarns(ParseOptions options) =>
		options.AssemblyConfig.FailTestsWithWarnings = ParseOnOff(options.Arguments[0]);

	static void OnFilter(
		string[] arguments,
		Action<string> addFunction) =>
			arguments.ForEach(addFunction);

	static void OnFilterQuery(ParseOptions options) =>
		options.Arguments.ForEach(options.AssemblyConfig.Filters.AddQueryFilter);

	static void OnFilterTrait(
		string[] arguments,
		Action<string, string> addFunction) =>
			arguments.ForEach(argument =>
			{
				var pieces = argument.Split('=');
				if (pieces.Length != 2 || string.IsNullOrEmpty(pieces[0]) || string.IsNullOrEmpty(pieces[1]))
					throw new ArgumentException("Invalid trait format (must be \"name=value\")");

				addFunction(pieces[0], pieces[1]);
			});

	static void OnInternalDiagnostics(ParseOptions options) =>
		options.AssemblyConfig.InternalDiagnosticMessages = ParseOnOff(options.Arguments[0]);

	static void OnIntValueWithMinimum(
		ParseOptions options,
		int minValue,
		Action<int> setter)
	{
		var stringValue = options.Arguments[0];

		if (!int.TryParse(options.Arguments[0], NumberStyles.None, NumberFormatInfo.CurrentInfo, out var intValue) || intValue < minValue)
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Invalid value '{0}' (must be an integer between {1} and {2})", stringValue, minValue, int.MaxValue));

		setter(intValue);
	}

	static void OnMaxThreads(ParseOptions options) =>
		options.AssemblyConfig.MaxParallelThreads = options.Arguments[0].ToUpperInvariant() switch
		{
			"0" => null,
			"DEFAULT" => null,
			"UNLIMITED" => -1,
			_ => ParseMaxThreadsValue(options.Arguments[0]),
		};

	static void OnMethodDisplay(ParseOptions options) =>
		options.AssemblyConfig.MethodDisplay = ParseEnum<TestMethodDisplay>(options.Arguments[0]);

	static void OnMethodDisplayOptions(ParseOptions options)
	{
		if (options.Arguments.Any(a => a.Equals("all", StringComparison.OrdinalIgnoreCase)))
		{
			if (options.Arguments.Length > 1)
				throw new ArgumentException("Cannot specify 'all' with any other values");

			options.AssemblyConfig.MethodDisplayOptions = TestMethodDisplayOptions.All;
		}
		else if (options.Arguments.Any(a => a.Equals("none", StringComparison.OrdinalIgnoreCase)))
		{
			if (options.Arguments.Length > 1)
				throw new ArgumentException("Cannot specify 'none' with any other values");

			options.AssemblyConfig.MethodDisplayOptions = TestMethodDisplayOptions.None;
		}
		else
		{
			options.AssemblyConfig.MethodDisplayOptions = TestMethodDisplayOptions.None;

			foreach (var argument in options.Arguments)
				options.AssemblyConfig.MethodDisplayOptions |= ParseEnum<TestMethodDisplayOptions>(argument);
		}
	}

	static void OnParallel(ParseOptions options) =>
		options.AssemblyConfig.ParallelizeTestCollections = options.Arguments[0].ToUpperInvariant() switch
		{
			"NONE" => false,
			"COLLECTIONS" => true,
			_ => throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Invalid value '{0}' (must be one of: 'none', 'collections')", options.Arguments[0])),
		};

	static void OnParallelAlgorithm(ParseOptions options) =>
		options.AssemblyConfig.ParallelAlgorithm = ParseEnum<ParallelAlgorithm>(options.Arguments[0]);

	static void OnPreEnumerateTheories(ParseOptions options) =>
		options.AssemblyConfig.PreEnumerateTheories = ParseOnOff(options.Arguments[0]);

	static void OnReport(
		IConfiguration? configuration,
		ICommandLineOptions? commandLineOptions,
		string transform,
		string optionName,
		string extension,
		TestProjectConfiguration projectConfig)
	{
		// If this is the validation from ValidateOptionArgumentsAsync, there's nothing to validate
		if (configuration is null || commandLineOptions is null)
			return;

		var outputFileName = Path.Combine(
			configuration.GetTestResultDirectory(),
			commandLineOptions.TryGetOptionArgumentList(optionName, out var filenameArguments)
				? filenameArguments[0]
				: reportFileNameRoot + extension
		);

		projectConfig.Output.Add(transform, outputFileName);
	}

	static void OnReportFilename(ParseOptions options)
	{
		// Pure validation only, actual setting of configuration value is done in OnReport
		if (!string.IsNullOrWhiteSpace(Path.GetDirectoryName(options.Arguments[0])))
			throw new ArgumentException("Report file name may not contain a path (use --results-directory to set the report output path)");
	}

	static void OnShowLiveOutput(ParseOptions options) =>
		options.AssemblyConfig.ShowLiveOutput = ParseOnOff(options.Arguments[0]);

	static void OnStopOnFail(ParseOptions options) =>
		options.AssemblyConfig.StopOnFail = ParseOnOff(options.Arguments[0]);

	/// <summary>
	/// Parse the command line options, placing them into the project and project assembly configuration.
	/// </summary>
	/// <param name="configuration">The Microsoft.Testing.Platform <see cref="IConfiguration"/></param>
	/// <param name="commandLineOptions">The Microsoft.Testing.Platform <see cref="ICommandLineOptions"/></param>
	/// <param name="projectAssembly">The project assembly to place the parsed values into</param>
	public static void Parse(
		IConfiguration configuration,
		ICommandLineOptions commandLineOptions,
		XunitProjectAssembly projectAssembly)
	{
		Guard.ArgumentNotNull(configuration);
		Guard.ArgumentNotNull(commandLineOptions);
		Guard.ArgumentNotNull(projectAssembly);

		foreach (var option in options)
			if (commandLineOptions.TryGetOptionArgumentList(option.Key, out var arguments))
				option.Value.Parse(new ParseOptions(arguments, projectAssembly.Configuration, projectAssembly.Project.Configuration, configuration, commandLineOptions));
	}

	static TEnum ParseEnum<TEnum>(string value)
		where TEnum : struct =>
			Enum.TryParse<TEnum>(value, ignoreCase: true, out var result)
				? result
				: throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Invalid value '{0}' (must be one of: {1})", value, string.Join(", ", Enum.GetValues(typeof(TEnum)).OfType<object>().Select(e => "'" + ToCamelCaseString(e) + "'"))));

	static int ParseMaxThreadsValue(string value)
	{
		var match = ConfigUtility.MultiplierStyleMaxParallelThreadsRegex.Match(value);

		// Use invariant format and convert ',' to '.' so we can always support both formats, regardless of locale
		// If we stick to locale-only parsing, we could break people when moving from one locale to another (for example,
		// from people running tests on their desktop in a comma locale vs. running them in CI with a decimal locale).
		return match.Success && decimal.TryParse(match.Groups[1].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var maxThreadMultiplier)
			? (int)(maxThreadMultiplier * Environment.ProcessorCount)
			: int.TryParse(value, out var threadValue) && threadValue > 0
				? threadValue
				: throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Invalid value '{0}' (must be one of: 'default', 'unlimited', a positive number, a multiplier in the form of '{1}x')", value, 0.0m));
	}

	static bool ParseOnOff(string value) =>
		value.ToUpperInvariant() switch
		{
			"ON" => true,
			"OFF" => false,
			_ => throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Invalid value '{0}' (must be one of: 'on', 'off')", value)),
		};

	static string ToCamelCaseString(object? obj)
	{
		var result = obj?.ToString();

		return
			result is not null && result.Length != 0
				? !char.IsLetter(result[0]) ? result : char.ToLowerInvariant(result[0]) + result.Substring(1)
				: string.Empty;
	}

	/// <inheritdoc/>
	public Task<ValidationResult> ValidateCommandLineOptionsAsync(ICommandLineOptions commandLineOptions)
	{
		Guard.ArgumentNotNull(commandLineOptions);

		foreach (var optionDependency in optionDependencies)
			if (commandLineOptions.IsOptionSet(optionDependency.Key) && !commandLineOptions.IsOptionSet(optionDependency.Value))
				return ValidationResult.InvalidTask(string.Format(CultureInfo.CurrentCulture, "'--{0}' requires '--{1}' to be enabled", optionDependency.Key, optionDependency.Value));

		return ValidationResult.ValidTask;
	}

	/// <inheritdoc/>
	public Task<ValidationResult> ValidateOptionArgumentsAsync(
		CommandLineOption commandOption,
		string[] arguments)
	{
		Guard.ArgumentNotNull(commandOption);
		Guard.ArgumentNotNull(arguments);

		if (options.TryGetValue(commandOption.Name, out var option))
		{
			try
			{
				var projectConfig = new TestProjectConfiguration();
				var assemblyConfig = new TestAssemblyConfiguration();
				option.Parse(new ParseOptions(arguments, assemblyConfig, projectConfig));
			}
			catch (ArgumentException argEx)
			{
				return ValidationResult.InvalidTask(argEx.Message);
			}
		}

		return ValidationResult.ValidTask;
	}

	sealed class ParseOptions(
		string[] arguments,
		TestAssemblyConfiguration assemblyConfiguration,
		TestProjectConfiguration projectConfiguration,
		IConfiguration? configuration = null,
		ICommandLineOptions? commandLineOptions = null)
	{
		public string[] Arguments { get; } = arguments;

		public TestAssemblyConfiguration AssemblyConfig { get; } = assemblyConfiguration;

		public ICommandLineOptions? CommandLineOptions { get; } = commandLineOptions;

		public IConfiguration? Configuration { get; } = configuration;

		public TestProjectConfiguration ProjectConfig { get; } = projectConfiguration;
	}
}
