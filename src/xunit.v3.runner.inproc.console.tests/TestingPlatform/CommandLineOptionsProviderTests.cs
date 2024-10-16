using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Configurations;
using NSubstitute;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.InProc.SystemConsole.TestingPlatform;
using Xunit.Sdk;

public class CommandLineOptionsProviderTests
{
	readonly IConfiguration configuration;
	readonly StubCommandLineOptions commandLineOptions;
	readonly XunitProjectAssembly projectAssembly;

	public CommandLineOptionsProviderTests()
	{
		configuration = Substitute.For<IConfiguration, InterfaceProxy<IConfiguration>>();
		commandLineOptions = new();
		projectAssembly = new(
			new XunitProject(),
			typeof(CommandLineOptionsProviderTests).Assembly.Location,
			new(3, TestData.DefaultTargetFramework)
		);
	}

	[Fact]
	public void GuardClauses()
	{
		Assert.Throws<ArgumentNullException>("configuration", () => CommandLineOptionsProvider.Parse(null!, commandLineOptions, projectAssembly));
		Assert.Throws<ArgumentNullException>("commandLineOptions", () => CommandLineOptionsProvider.Parse(configuration, null!, projectAssembly));
		Assert.Throws<ArgumentNullException>("projectAssembly", () => CommandLineOptionsProvider.Parse(configuration, commandLineOptions, null!));
	}

	public class OnOffSwitches : CommandLineOptionsProviderTests
	{
		public static readonly TheoryData<string, Expression<Func<XunitProjectAssembly, bool?>>> SwitchOptionsList =
		[
			("fail-skips", assembly => assembly.Configuration.FailSkips),
			("fail-warns", assembly => assembly.Configuration.FailTestsWithWarnings),
			("pre-enumerate-theories", assembly => assembly.Configuration.PreEnumerateTheories),
			("show-live-output", assembly => assembly.Configuration.ShowLiveOutput),
			("stop-on-fail", assembly => assembly.Configuration.StopOnFail),
			("xunit-diagnostics", assembly => assembly.Configuration.DiagnosticMessages),
			("xunit-internal-diagnostics", assembly => assembly.Configuration.InternalDiagnosticMessages),
		];

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(SwitchOptionsList))]
		public void SwitchNotPresent(
			string _,
			Expression<Func<XunitProjectAssembly, bool?>> accessor)
		{
			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.Null(accessor.Compile().Invoke(projectAssembly));
		}

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(SwitchOptionsList))]
		public void SwitchOff(
			string @switch,
			Expression<Func<XunitProjectAssembly, bool?>> accessor)
		{
			commandLineOptions.Set(@switch, ["off"]);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.False(accessor.Compile().Invoke(projectAssembly));
		}

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(SwitchOptionsList))]
		public void SwitchOn(
			string @switch,
			Expression<Func<XunitProjectAssembly, bool?>> accessor)
		{
			commandLineOptions.Set(@switch, ["on"]);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.True(accessor.Compile().Invoke(projectAssembly));
		}

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(SwitchOptionsList))]
		public void SwitchInvalid(
			string @switch,
			Expression<Func<XunitProjectAssembly, bool?>> _)
		{
			commandLineOptions.Set(@switch, ["foo"]);

			var ex = Record.Exception(() => CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly));

			Assert.IsType<ArgumentException>(ex);
			Assert.Equal("Invalid value 'foo' (must be one of: 'on', 'off')", ex.Message);
		}

		[Theory]
		[InlineData("on")]
		[InlineData("off")]
		public void AutoReporters_Valid(string value)
		{
			commandLineOptions.Set("auto-reporters", [value]);

			var ex = Record.Exception(() => CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly));

			// There's nothing else to observe here other than there being no exception
			Assert.Null(ex);
		}

		[Fact]
		public void AutoReporters_Invalid()
		{
			commandLineOptions.Set("auto-reporters", ["foo"]);

			var ex = Record.Exception(() => CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly));

			Assert.IsType<ArgumentException>(ex);
			Assert.Equal("Invalid value 'foo' (must be one of: 'on', 'off')", ex.Message);
		}
	}

	public class ArgumentSwitches : CommandLineOptionsProviderTests
	{
		[Theory]
		[InlineData("Invalid value '123'", "culture", "123")]
		[InlineData("Invalid value 'abc' (must be one of: 'off', 'on', 'only')", "explicit", "abc")]
		[InlineData("Invalid value 'abc' (must be a positive integer)", "long-running", "abc")]
		[InlineData("Invalid value 'abc' (must be one of: 'default', 'unlimited', a positive number, a multiplier in the form of '0.0x')", "max-threads", "abc")]
		[InlineData("Invalid value 'abc' (must be one of: 'classAndMethod', 'method')", "method-display", "abc")]
		[InlineData("Invalid value 'abc' (must be one of: 'none', 'replaceUnderscoreWithSpace', 'useOperatorMonikers', 'useEscapeSequences', 'replacePeriodWithComma', 'all')", "method-display-options", "abc")]
		[InlineData("Cannot specify 'all' with any other values", "method-display-options", "all", "replacePeriodWithComma")]
		[InlineData("Cannot specify 'none' with any other values", "method-display-options", "replacePeriodWithComma", "none")]
		[InlineData("Invalid value 'abc' (must be one of: 'none', 'collections')", "parallel", "abc")]
		[InlineData("Invalid value 'abc' (must be one of: 'conservative', 'aggressive')", "parallel-algorithm", "abc")]
		[InlineData("Invalid value 'abc' (must be an integer in the range of 0 - 2147483647)", "seed", "abc")]
		public void Validation(
			string expectedMessage,
			string @switch,
			params string[] argValues)
		{
			if (@switch == "culture")
				Assert.SkipUnless(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), "This value only throws on Windows");

			commandLineOptions.Set(@switch, argValues);

			var ex = Record.Exception(() => CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly));

			Assert.IsType<ArgumentException>(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}

		[Fact]
		public void ConfigFile()
		{
			commandLineOptions.Set("xunit-config-filename", ["foo-bar-baz.json"]);

			var ex = Record.Exception(() => CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly));

			Assert.IsType<ArgumentException>(ex);
			Assert.Equal($"Config file '{Path.GetFullPath("foo-bar-baz.json")}' was not found", ex.Message);
		}

		[Theory]
		[InlineData("default", null)]
		[InlineData("invariant", "")]
		[InlineData("en-US", "en-US")]
		public void Culture(
			string argValue,
			string? expected)
		{
			commandLineOptions.Set("culture", [argValue]);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.Equal(expected, projectAssembly.Configuration.Culture);
		}

		[Theory]
		[InlineData("on", ExplicitOption.On)]
		[InlineData("off", ExplicitOption.Off)]
		[InlineData("only", ExplicitOption.Only)]
		public void Explicit(
			string argValue,
			ExplicitOption expected)
		{
			commandLineOptions.Set("explicit", [argValue]);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.Equal(expected, projectAssembly.Configuration.ExplicitOption);
		}

		[Fact]
		public void LongRunning()
		{
			commandLineOptions.Set("long-running", ["123"]);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.Equal(123, projectAssembly.Configuration.LongRunningTestSeconds);
		}

		public static TheoryData<string, int?> MaxThreadsData =
		[
			("default", null),
			("unlimited", -1),
			("42", 42),
			("2.5x", (int)(Environment.ProcessorCount * 2.5)),
		];

		[Theory]
		[MemberData(nameof(MaxThreadsData))]
		public void MaxThreads(
			string argValue,
			int? expected)
		{
			commandLineOptions.Set("max-threads", [argValue]);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.Equal(expected, projectAssembly.Configuration.MaxParallelThreads);
		}

		[Theory]
		[InlineData("classAndMethod", TestMethodDisplay.ClassAndMethod)]
		[InlineData("method", TestMethodDisplay.Method)]
		public void MethodDisplay(
			string argValue,
			TestMethodDisplay expected)
		{
			commandLineOptions.Set("method-display", [argValue]);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.Equal(expected, projectAssembly.Configuration.MethodDisplay);
		}

		[Theory]
		[InlineData(TestMethodDisplayOptions.All, "all")]
		[InlineData(TestMethodDisplayOptions.None, "none")]
		[InlineData(TestMethodDisplayOptions.ReplacePeriodWithComma, "replacePeriodWithComma")]
		[InlineData(TestMethodDisplayOptions.ReplaceUnderscoreWithSpace, "replaceUnderscoreWithSpace")]
		[InlineData(TestMethodDisplayOptions.UseEscapeSequences, "useEscapeSequences")]
		[InlineData(TestMethodDisplayOptions.UseOperatorMonikers, "useOperatorMonikers")]
		[InlineData(TestMethodDisplayOptions.ReplacePeriodWithComma | TestMethodDisplayOptions.ReplaceUnderscoreWithSpace, "replacePeriodWithComma", "replaceUnderscoreWithSpace")]
		public void MethodDisplayOptions(
			TestMethodDisplayOptions expected,
			params string[] argValues)
		{
			commandLineOptions.Set("method-display-options", argValues);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.Equal(expected, projectAssembly.Configuration.MethodDisplayOptions);
		}

		[Theory]
		[InlineData("none", false)]
		[InlineData("collections", true)]
		public void Parallel(
			string argValue,
			bool expected)
		{
			commandLineOptions.Set("parallel", [argValue]);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.Equal(expected, projectAssembly.Configuration.ParallelizeTestCollections);
		}

		[Theory]
		[InlineData("conservative", ParallelAlgorithm.Conservative)]
		[InlineData("aggressive", ParallelAlgorithm.Aggressive)]
		public void Parallel_Algorithm(
			string argValue,
			ParallelAlgorithm expected)
		{
			commandLineOptions.Set("parallel-algorithm", [argValue]);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.Equal(expected, projectAssembly.Configuration.ParallelAlgorithm);
		}

		[Fact]
		public void Seed()
		{
			commandLineOptions.Set("seed", ["42"]);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.Equal(42, projectAssembly.Configuration.Seed);
		}
	}

	public class Filters : CommandLineOptionsProviderTests
	{
		public static readonly TheoryData<string, string> FilterOptionList =
		[
			("filter-class", "-class"),
			("filter-not-class", "-class-"),
			("filter-method", "-method"),
			("filter-not-method", "-method-"),
			("filter-namespace", "-namespace"),
			("filter-not-namespace", "-namespace-"),
		];

		[Theory]
		[MemberData(nameof(FilterOptionList))]
		public void Filter_SingleValue(
			string mtpSwitch,
			string xunit3Switch)
		{
			commandLineOptions.Set(mtpSwitch, ["foo"]);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.Collection(
				projectAssembly.Configuration.Filters.ToXunit3Arguments(),
				arg => Assert.Equal(xunit3Switch, arg),
				arg => Assert.Equal("foo", arg)
			);
		}

		[Theory]
		[MemberData(nameof(FilterOptionList))]
		public void Filter_MultiValue(
			string mtpSwitch,
			string xunit3Switch)
		{
			commandLineOptions.Set(mtpSwitch, ["foo", "bar"]);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.Collection(
				projectAssembly.Configuration.Filters.ToXunit3Arguments(),
				arg => Assert.Equal(xunit3Switch, arg),
				arg => Assert.Equal("foo", arg),
				arg => Assert.Equal(xunit3Switch, arg),
				arg => Assert.Equal("bar", arg)
			);
		}

		[Fact]
		public void FilterTraits()
		{
			commandLineOptions.Set("filter-trait", ["foo=bar"]);
			commandLineOptions.Set("filter-not-trait", ["baz=biff"]);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			Assert.Collection(
				projectAssembly.Configuration.Filters.ToXunit3Arguments(),
				arg => Assert.Equal("-trait", arg),
				arg => Assert.Equal("foo=bar", arg),
				arg => Assert.Equal("-trait-", arg),
				arg => Assert.Equal("baz=biff", arg)
			);
		}

		[Theory]
		[InlineData("filter-trait")]
		[InlineData("filter-not-trait")]
		public void FilterTraitsValidation(string @switch)
		{
			commandLineOptions.Set(@switch, ["foo"]);

			var ex = Record.Exception(() => CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly));

			Assert.IsType<ArgumentException>(ex);
			Assert.Equal("Invalid trait format (must be \"name=value\")", ex.Message);
		}
	}

	public class Reports : CommandLineOptionsProviderTests
	{
		public static TheoryData<string> ReportOptions = ["ctrf", "junit", "nunit", "xunit", "xunit-html", "xunit-trx"];

		[Theory]
		[MemberData(nameof(ReportOptions))]
		public void PathValidation(string option)
		{
			commandLineOptions.Set($"report-{option}-filename", ["/path/to/report-file"]);

			var ex = Record.Exception(() => CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly));

			Assert.IsType<ArgumentException>(ex);
			Assert.Equal("Report file name may not contain a path (use --results-directory to set the report output path)", ex.Message);
		}

		[Theory]
		[MemberData(nameof(ReportOptions))]
		public async Task FileNameRequiresEnablementOption(string option)
		{
			commandLineOptions.Set($"report-{option}-filename", ["report-file"]);

			var result = await new CommandLineOptionsProvider().ValidateCommandLineOptionsAsync(commandLineOptions);

			Assert.False(result.IsValid);
			Assert.Equal($"'--report-{option}-filename' requires '--report-{option}' to be enabled", result.ErrorMessage);
		}

		public static TheoryData<string, string, string> ReportOptionsKeyExtension =
		[
			("ctrf", "ctrf", "ctrf"),
			("junit", "junit", "junit"),
			("nunit", "nunit", "nunit"),
			("xunit", "xml", "xunit"),
			("xunit-html", "html", "html"),
			("xunit-trx", "trx", "trx"),
		];

		[Theory]
		[MemberData(nameof(ReportOptionsKeyExtension))]
		public void OptionWithoutFilename(
			string option,
			string outputKey,
			string extension)
		{
			commandLineOptions.Set($"report-{option}", []);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			var output = Assert.Single(projectAssembly.Project.Configuration.Output);
			Assert.Equal(outputKey, output.Key);
			// Format: <user>_<machine>_yyyy-MM-dd_hh_mm_ss.fff.<extension>
			Assert.Matches($"{Environment.UserName}_{Environment.MachineName}_(\\d){{4}}-(\\d){{2}}-(\\d){{2}}_(\\d){{2}}_(\\d){{2}}_(\\d){{2}}\\.(\\d){{3}}\\.{extension}", output.Value);
		}

		[Theory]
		[MemberData(nameof(ReportOptionsKeyExtension))]
		public void OptionWithFilename(
			string option,
			string outputKey,
			string _)
		{
			configuration.GetTestResultDirectory().Returns("/path/to/results");
			commandLineOptions.Set($"report-{option}", []);
			commandLineOptions.Set($"report-{option}-filename", ["report-file"]);

			CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

			var output = Assert.Single(projectAssembly.Project.Configuration.Output);
			Assert.Equal(outputKey, output.Key);
			Assert.Equal(Path.Combine("/path/to/results", "report-file"), output.Value);
		}
	}

	class StubCommandLineOptions : ICommandLineOptions
	{
		readonly Dictionary<string, string[]> options = new(StringComparer.OrdinalIgnoreCase);

		public bool IsOptionSet(string optionName) =>
			options.ContainsKey(optionName);

		public void Set(
			string optionName,
			string[] arguments) =>
				options[optionName] = arguments;

		public bool TryGetOptionArgumentList(
			string optionName,
			[NotNullWhen(true)] out string[]? arguments) =>
				options.TryGetValue(optionName, out arguments);
	}
}
