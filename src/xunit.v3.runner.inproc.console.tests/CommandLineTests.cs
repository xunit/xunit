using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.InProc.SystemConsole;

public class CommandLineTests
{
	public class UnknownOption
	{
		[Fact]
		public static void UnknownOptionThrows()
		{
			var commandLine = new TestableCommandLine("-unknown");

			var exception = Record.Exception(() => commandLine.Parse());

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal("unknown option: -unknown", exception.Message);
		}
	}

	public class Project
	{
		[Fact]
		public static void DefaultValues()
		{
			var commandLine = new TestableCommandLine();

			var assembly = commandLine.Parse();

			Assert.Equal($"/full/path/{typeof(CommandLineTests).Assembly.Location}", assembly.AssemblyFileName);
			Assert.Null(assembly.ConfigFileName);
		}

		[Fact]
		public static void ConfigFileDoesNotExist_Throws()
		{
			var commandLine = new TestableCommandLine("badConfig.json");

			var exception = Record.Exception(() => commandLine.Parse());

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal("config file not found: badConfig.json", exception.Message);
		}

		[Fact]
		public static void ConfigFileUnsupportedFormat_Throws()
		{
			var commandLine = new TestableCommandLine("assembly1.config");

			var exception = Record.Exception(() => commandLine.Parse());

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal("unknown option: assembly1.config", exception.Message);
		}

		[Fact]
		public static void TwoConfigFiles_Throws()
		{
			var commandLine = new TestableCommandLine("assembly1.json", "assembly2.json");

			var exception = Record.Exception(() => commandLine.Parse());

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal("unknown option: assembly2.json", exception.Message);
		}

		[Fact]
		public static void WithConfigFile()
		{
			var commandLine = new TestableCommandLine("assembly1.json");

			var assembly = commandLine.Parse();

			Assert.Equal("/full/path/assembly1.json", assembly.ConfigFileName);
		}
	}

	[Collection("Switches Test Collection")]
	public sealed class Switches : IDisposable
	{
		readonly string? _originalNoColorValue;

		public Switches()
		{
			_originalNoColorValue = Environment.GetEnvironmentVariable(TestProjectConfiguration.EnvNameNoColor);
			Environment.SetEnvironmentVariable(TestProjectConfiguration.EnvNameNoColor, null);
		}

		public void Dispose() =>
			Environment.SetEnvironmentVariable(TestProjectConfiguration.EnvNameNoColor, _originalNoColorValue);

		static readonly (string Switch, Expression<Func<XunitProjectAssembly, bool>> Accessor)[] SwitchOptionsList =
		[
			("-debug", assembly => assembly.Project.Configuration.DebugOrDefault),
			("-diagnostics", assembly => assembly.Configuration.DiagnosticMessagesOrDefault),
			("-failskips", assembly => assembly.Configuration.FailSkipsOrDefault),
			("-ignorefailures", assembly => assembly.Project.Configuration.IgnoreFailuresOrDefault),
			("-internaldiagnostics", assembly => assembly.Configuration.InternalDiagnosticMessagesOrDefault),
			("-noautoreporters", assembly => assembly.Project.Configuration.NoAutoReportersOrDefault),
			("-nocolor", assembly => assembly.Project.Configuration.NoColorOrDefault),
			("-nologo", assembly => assembly.Project.Configuration.NoLogoOrDefault),
			("-pause", assembly => assembly.Project.Configuration.PauseOrDefault),
			("-preenumeratetheories", assembly => assembly.Configuration.PreEnumerateTheories ?? false),
			("-showliveoutput", assembly => assembly.Configuration.ShowLiveOutputOrDefault),
			("-stoponfail", assembly => assembly.Configuration.StopOnFailOrDefault),
			("-wait", assembly => assembly.Project.Configuration.WaitOrDefault),
		];

		public static readonly TheoryData<string, Expression<Func<XunitProjectAssembly, bool>>> SwitchesLowerCase =
			new(SwitchOptionsList);

		public static readonly TheoryData<string, Expression<Func<XunitProjectAssembly, bool>>> SwitchesUpperCase =
			new(SwitchOptionsList.Select(t => (t.Switch.ToUpperInvariant(), t.Accessor)));

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public void SwitchDefault(
			string _,
			Expression<Func<XunitProjectAssembly, bool>> accessor)
		{
			var commandLine = new TestableCommandLine("no-config.json");
			var assembly = commandLine.Parse();

			var result = accessor.Compile().Invoke(assembly);

			Assert.False(result);
		}

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public void SwitchOverride(
			string @switch,
			Expression<Func<XunitProjectAssembly, bool>> accessor)
		{
			var commandLine = new TestableCommandLine("no-config.json", @switch);
			var assembly = commandLine.Parse();

			var result = accessor.Compile().Invoke(assembly);

			Assert.True(result);
		}

		[Fact]
		public void NoColorSetsEnvironmentVariable()
		{
			Assert.Null(Environment.GetEnvironmentVariable(TestProjectConfiguration.EnvNameNoColor));

			new TestableCommandLine("no-config.json", "-nocolor").Parse();

			// Any set (non-null, non-empty) value is acceptable, see https://no-color.org/
			var envValue = Environment.GetEnvironmentVariable(TestProjectConfiguration.EnvNameNoColor);
			Assert.NotNull(envValue);
			Assert.NotEmpty(envValue);
		}
	}

	public class OptionsWithArguments
	{
		public class Automated
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine("no-config.json");

				var assembly = commandLine.Parse();

				Assert.Null(assembly.Configuration.SynchronousMessageReporting);
			}

			[Fact]
			public static void UnspecifiedValueIsNull()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-automated");

				var assembly = commandLine.Parse();

				Assert.Null(assembly.Configuration.SynchronousMessageReporting);
			}

			[Fact]
			public static void AsyncIsFalse()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-automated", "async");

				var assembly = commandLine.Parse();

				Assert.False(assembly.Configuration.SynchronousMessageReporting);
			}

			[Fact]
			public static void SyncIsTrue()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-automated", "sync");

				var assembly = commandLine.Parse();

				Assert.True(assembly.Configuration.SynchronousMessageReporting);
			}
		}

		public class AssertEquivalentMaxDepth
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine("no-config.json");

				var assembly = commandLine.Parse();

				Assert.Null(assembly.Configuration.AssertEquivalentMaxDepth);
			}

			[Fact]
			public static void ValueIsPreserved()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-assertEquivalentMaxDepth", "42");

				var assembly = commandLine.Parse();

				Assert.Equal(42, assembly.Configuration.AssertEquivalentMaxDepth);
			}
		}

		public class Culture
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine("no-config.json");

				var assembly = commandLine.Parse();

				Assert.Null(assembly.Configuration.Culture);
			}

			[Fact]
			public static void ExplicitDefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-culture", "default");

				var assembly = commandLine.Parse();

				Assert.Null(assembly.Configuration.Culture);
			}

			[Fact]
			public static void InvariantCultureIsEmptyString()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-culture", "invariant");

				var assembly = commandLine.Parse();

				Assert.Equal(string.Empty, assembly.Configuration.Culture);
			}

			[Fact]
			public static void ValueIsPreserved()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-culture", "foo");

				var assembly = commandLine.Parse();

				Assert.Equal("foo", assembly.Configuration.Culture);
			}
		}

		public class LongRunning
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine("no-config.json");

				var assembly = commandLine.Parse();

				Assert.Null(assembly.Configuration.DiagnosticMessages);
				Assert.Null(assembly.Configuration.LongRunningTestSeconds);
			}

			[Fact]
			public static void MissingValue()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-longrunning");

				var exception = Record.Exception(commandLine.Parse);

				Assert.IsType<ArgumentException>(exception);
				Assert.Equal("missing argument for -longRunning", exception.Message);
			}

			[Fact]
			public static void InvalidValue()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-longrunning", "abc");

				var exception = Record.Exception(commandLine.Parse);

				Assert.IsType<ArgumentException>(exception);
				Assert.Equal("incorrect argument value for -longRunning (must be a positive integer)", exception.Message);
			}

			[Fact]
			public static void ValidValue()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-LONGRUNNING", "123");

				var assembly = commandLine.Parse();

				Assert.True(assembly.Configuration.DiagnosticMessages);
				Assert.Equal(123, assembly.Configuration.LongRunningTestSeconds);
			}
		}

		public class MaxThreads
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine("no-config.json");

				var assembly = commandLine.Parse();

				Assert.Null(assembly.Configuration.MaxParallelThreads);
			}

			[Fact]
			public static void MissingValue()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-maxthreads");

				var exception = Record.Exception(() => commandLine.Parse());

				Assert.IsType<ArgumentException>(exception);
				Assert.Equal("missing argument for -maxThreads", exception.Message);
			}

			[Theory]
			[InlineData("abc")]
			// Non digit
			[InlineData("0.ax")]
			[InlineData("0,ax")]
			// Missing leading digit
			[InlineData(".0x")]
			[InlineData(",0x")]
			public static void InvalidValues(string value)
			{
				var commandLine = new TestableCommandLine("no-config.json", "-maxthreads", value);

				var exception = Record.Exception(commandLine.Parse);

				Assert.IsType<ArgumentException>(exception);
				Assert.Equal($"incorrect argument value for -maxThreads (must be 'default', 'unlimited', a positive number, or a multiplier in the form of '{0.0m}x')", exception.Message);
			}

			[Theory]
			[InlineData("default", null)]
			[InlineData("0", null)]
			[InlineData("unlimited", -1)]
			[InlineData("16", 16)]
			public static void ValidValues(
				string value,
				int? expected)
			{
				var commandLine = new TestableCommandLine("no-config.json", "-maxthreads", value);

				var assembly = commandLine.Parse();

				Assert.Equal(expected, assembly.Configuration.MaxParallelThreads);
			}

			[Theory]
			[InlineData("2x")]
			[InlineData("2.0x")]
			[InlineData("2,0x")]
			public static void MultiplierValue(string value)
			{
				var expected = Environment.ProcessorCount * 2;
				var commandLine = new TestableCommandLine("no-config.json", "-maxthreads", value);

				var assembly = commandLine.Parse();

				Assert.Equal(expected, assembly.Configuration.MaxParallelThreads);
			}
		}

		public class Parallelization
		{
			[Fact]
			public static void ParallelizationOptionsAreNullByDefault()
			{
				var commandLine = new TestableCommandLine("no-config.json");

				var assembly = commandLine.Parse();

				Assert.Null(assembly.Configuration.ParallelizeTestCollections);
			}

			[Fact]
			public static void FailsWithoutOptionOrWithIncorrectOptions()
			{
				var commandLine1 = new TestableCommandLine("no-config.json", "-parallel");
				var exception1 = Record.Exception(commandLine1.Parse);
				Assert.IsType<ArgumentException>(exception1);
				Assert.Equal("missing argument for -parallel", exception1.Message);

				var commandLine2 = new TestableCommandLine("no-config.json", "-parallel", "nonsense");
				var exception2 = Record.Exception(commandLine2.Parse);
				Assert.IsType<ArgumentException>(exception2);
				Assert.Equal("incorrect argument value for -parallel", exception2.Message);
			}

			[Theory]
			[InlineData("none", false)]
			[InlineData("collections", true)]
			public static void ParallelCanBeTurnedOn(
				string parallelOption,
				bool expectedCollectionsParallelization)
			{
				var commandLine = new TestableCommandLine("no-config.json", "-parallel", parallelOption);

				var assembly = commandLine.Parse();

				Assert.Equal(expectedCollectionsParallelization, assembly.Configuration.ParallelizeTestCollections);
			}
		}

		public class PrintMaxEnumerableLength
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine("no-config.json");

				var assembly = commandLine.Parse();

				Assert.Null(assembly.Configuration.PrintMaxEnumerableLength);
			}

			[Fact]
			public static void ValueIsPreserved()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-printMaxEnumerableLength", "42");

				var assembly = commandLine.Parse();

				Assert.Equal(42, assembly.Configuration.PrintMaxEnumerableLength);
			}
		}

		public class PrintMaxObjectDepth
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine("no-config.json");

				var assembly = commandLine.Parse();

				Assert.Null(assembly.Configuration.PrintMaxObjectDepth);
			}

			[Fact]
			public static void ValueIsPreserved()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-printMaxObjectDepth", "42");

				var assembly = commandLine.Parse();

				Assert.Equal(42, assembly.Configuration.PrintMaxObjectDepth);
			}
		}

		public class PrintMaxObjectMemberCount()
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine("no-config.json");

				var assembly = commandLine.Parse();

				Assert.Null(assembly.Configuration.PrintMaxObjectMemberCount);
			}

			[Fact]
			public static void ValueIsPreserved()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-printMaxObjectMemberCount", "42");

				var assembly = commandLine.Parse();

				Assert.Equal(42, assembly.Configuration.PrintMaxObjectMemberCount);
			}
		}

		public class PrintMaxStringLength
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine("no-config.json");

				var assembly = commandLine.Parse();

				Assert.Null(assembly.Configuration.PrintMaxStringLength);
			}

			[Fact]
			public static void ValueIsPreserved()
			{
				var commandLine = new TestableCommandLine("no-config.json", "-printMaxStringLength", "42");

				var assembly = commandLine.Parse();

				Assert.Equal(42, assembly.Configuration.PrintMaxStringLength);
			}
		}
	}

	public class Filters
	{
		[Fact]
		public static void DefaultFilters()
		{
			var commandLine = new TestableCommandLine("no-config.json");

			var assembly = commandLine.Parse();

			var filters = assembly.Configuration.Filters;
			Assert.True(filters.Empty);
		}

		static readonly string[] SwitchOptionsList =
		[
			"-namespace",
			"-namespace-",
			"-class",
			"-class-",
			"-method",
			"-method-",
		];

		public static readonly TheoryData<string> SwitchesLowerCase =
			new(SwitchOptionsList);

		public static readonly TheoryData<string> SwitchesUpperCase =
			new(SwitchOptionsList.Select(t => t.ToUpperInvariant()));

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void MissingOptionValue(string @switch)
		{
			var commandLine = new TestableCommandLine("no-config.json", @switch);

			var exception = Record.Exception(commandLine.Parse);

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal($"missing argument for {@switch.ToLowerInvariant()}", exception.Message);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void SingleValidArgument(string @switch)
		{
			var commandLine = new TestableCommandLine("no-config.json", @switch, "value1");

			var assembly = commandLine.Parse();

			Assert.Collection(
				assembly.Configuration.Filters.ToXunit3Arguments(),
				arg => Assert.Equal(@switch.ToLowerInvariant(), arg),
				arg => Assert.Equal("value1", arg)
			);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void MultipleValidArguments(string @switch)
		{
			var commandLine = new TestableCommandLine("no-config.json", @switch, "value2", @switch, "value1");

			var assembly = commandLine.Parse();

			Assert.Collection(
				assembly.Configuration.Filters.ToXunit3Arguments(),
				arg => Assert.Equal(@switch.ToLowerInvariant(), arg),
				arg => Assert.Equal("value2", arg),
				arg => Assert.Equal(@switch.ToLowerInvariant(), arg),
				arg => Assert.Equal("value1", arg)
			);
		}

		public class Traits
		{
			static readonly string[] SwitchOptionsList =
			[
				"-trait",
				"-trait-",
			];

			static readonly string[] BadFormatValues =
			[
				// Missing equals
				"foobar",
				// Missing value
				"foo=",
				// Missing name
				"=bar",
				// Double equal signs
				"foo=bar=baz",
			];

			public static readonly TheoryData<string> SwitchesLowerCase =
				new(SwitchOptionsList);

			public static readonly TheoryData<string> SwitchesUpperCase =
				new(SwitchOptionsList.Select(x => x.ToUpperInvariant()));

			public static readonly TheoryData<string, string> SwitchesWithOptionsLowerCase =
				new(SwitchOptionsList.SelectMany(@switch => BadFormatValues.Select(value => (@switch, value))));

			public static readonly TheoryData<string, string> SwitchesWithOptionsUpperCase =
				new(SwitchOptionsList.SelectMany(@switch => BadFormatValues.Select(value => (@switch.ToUpperInvariant(), value))));

			[Theory]
			[MemberData(nameof(SwitchesLowerCase))]
			[MemberData(nameof(SwitchesUpperCase))]
			public static void SingleValidTraitArgument(string @switch)
			{
				var commandLine = new TestableCommandLine("no-config.json", @switch, "foo=bar");

				var assembly = commandLine.Parse();

				Assert.Collection(
					assembly.Configuration.Filters.ToXunit3Arguments(),
					arg => Assert.Equal(@switch.ToLowerInvariant(), arg),
					arg => Assert.Equal("foo=bar", arg)
				);
			}

			[Theory]
			[MemberData(nameof(SwitchesLowerCase))]
			[MemberData(nameof(SwitchesUpperCase))]
			public static void MultipleValidTraitArguments(string @switch)
			{
				var commandLine = new TestableCommandLine("no-config.json", @switch, "foo=bar", @switch, "foo=baz");

				var assembly = commandLine.Parse();

				Assert.Collection(
					assembly.Configuration.Filters.ToXunit3Arguments(),
					arg => Assert.Equal(@switch.ToLowerInvariant(), arg),
					arg => Assert.Equal("foo=bar", arg),
					arg => Assert.Equal(@switch.ToLowerInvariant(), arg),
					arg => Assert.Equal("foo=baz", arg)
				);
			}

			[Theory]
			[MemberData(nameof(SwitchesLowerCase))]
			[MemberData(nameof(SwitchesUpperCase))]
			public static void MissingOptionValue(string @switch)
			{
				var commandLine = new TestableCommandLine("no-config.json", @switch);

				var exception = Record.Exception(commandLine.Parse);

				Assert.IsType<ArgumentException>(exception);
				Assert.Equal($"missing argument for {@switch.ToLowerInvariant()}", exception.Message);
			}

			[Theory]
			[MemberData(nameof(SwitchesWithOptionsLowerCase))]
			[MemberData(nameof(SwitchesWithOptionsUpperCase))]
			public static void ImproperlyFormattedOptionValue(
				string @switch,
				string optionValue)
			{
				var commandLine = new TestableCommandLine("no-config.json", @switch, optionValue);

				var exception = Record.Exception(commandLine.Parse);

				Assert.IsType<ArgumentException>(exception);
				Assert.Equal($"incorrect argument format for {@switch.ToLowerInvariant()} (should be \"name=value\")", exception.Message);
			}
		}
	}

	public class Transforms
	{
		public static readonly TheoryData<string> SwitchesLowerCase =
			new(TransformFactory.AvailableTransforms.Select(x => $"-{x.ID}"));

		public static readonly TheoryData<string> SwitchesUpperCase =
			new(TransformFactory.AvailableTransforms.Select(x => $"-{x.ID.ToUpperInvariant()}"));

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void OutputMissingFilename(string @switch)
		{
			var commandLine = new TestableCommandLine("no-config.json", @switch);

			var exception = Record.Exception(commandLine.Parse);

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal($"missing filename for {@switch}", exception.Message);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void Output(string @switch)
		{
			var commandLine = new TestableCommandLine("no-config.json", @switch, "outputFile");

			var assembly = commandLine.Parse();

			var output = Assert.Single(assembly.Project.Configuration.Output);
			Assert.Equal(@switch.Substring(1), output.Key, ignoreCase: true);
			Assert.Equal("outputFile", output.Value);
		}
	}

	[Collection(nameof(EnvironmentHelper.NullifyEnvironmentalReporters))]
	public sealed class Reporters : IDisposable
	{
		readonly IDisposable environmentCleanup;

		public Reporters() =>
			environmentCleanup = EnvironmentHelper.NullifyEnvironmentalReporters();

		public void Dispose() =>
			environmentCleanup.Dispose();

		[Fact]
		public void NoReporters_UsesDefaultReporter()
		{
			var commandLine = new TestableCommandLine("no-config.json");

			var assembly = commandLine.Parse();

			Assert.IsType<DefaultRunnerReporter>(assembly.Project.RunnerReporter);
		}

		[Fact]
		public void NoExplicitReporter_NoEnvironmentallyEnabledReporters_UsesDefaultReporter()
		{
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: false);
			var commandLine = new TestableCommandLine([implicitReporter], "no-config.json");

			var assembly = commandLine.Parse();

			Assert.IsType<DefaultRunnerReporter>(assembly.Project.RunnerReporter);
		}

		[Fact]
		public void ExplicitReporter_NoEnvironmentalOverride_UsesExplicitReporter()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");
			var commandLine = new TestableCommandLine([explicitReporter], "no-config.json", "-reporter", "switch");

			var assembly = commandLine.Parse();

			Assert.Same(explicitReporter, assembly.Project.RunnerReporter);
		}

		[Fact]
		public void ExplicitReporter_WithEnvironmentalOverride_UsesEnvironmentalOverride()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
			var commandLine = new TestableCommandLine([explicitReporter, implicitReporter], "no-config.json", "-reporter", "switch");

			var assembly = commandLine.Parse();

			Assert.Same(implicitReporter, assembly.Project.RunnerReporter);
		}

		[Fact]
		public void WithEnvironmentalOverride_WithEnvironmentalOverridesDisabled_UsesDefaultReporter()
		{
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
			var commandLine = new TestableCommandLine([implicitReporter], "no-config.json", "-noautoreporters");

			var assembly = commandLine.Parse();

			Assert.IsType<DefaultRunnerReporter>(assembly.Project.RunnerReporter);
		}

		[Fact]
		public void NoExplicitReporter_SelectsFirstEnvironmentallyEnabledReporter()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");
			var implicitReporter1 = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
			var implicitReporter2 = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
			var commandLine = new TestableCommandLine([explicitReporter, implicitReporter1, implicitReporter2], "no-config.json");

			var assembly = commandLine.Parse();

			Assert.Same(implicitReporter1, assembly.Project.RunnerReporter);
		}
	}

	class TestableCommandLine : CommandLine
	{
		public TestableCommandLine(params string[] arguments)
			: base(new ConsoleHelper(TextReader.Null, TextWriter.Null), Assembly.GetExecutingAssembly(), arguments)
		{ }

		public TestableCommandLine(
			IReadOnlyList<IRunnerReporter> reporters,
			params string[] arguments)
				: base(new ConsoleHelper(TextReader.Null, TextWriter.Null), Assembly.GetExecutingAssembly(), arguments, reporters)
		{ }

		protected override bool FileExists(string? path) =>
			path?.StartsWith("badConfig.") != true && path != "fileName";

		protected override string? GetFullPath(string? fileName) =>
			fileName is null ? null : $"/full/path/{fileName}";
	}
}
