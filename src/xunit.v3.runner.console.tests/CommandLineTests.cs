using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Runner.SystemConsole;

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
		public static void AssemblyDoesNotExist_Throws()
		{
			var commandLine = new TestableCommandLine("badAssembly.dll");

			var exception = Record.Exception(commandLine.Parse);

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal("assembly not found: badAssembly.dll", exception.Message);
		}

		[Theory]
		[InlineData("badConfig.config")]
		[InlineData("badConfig.json")]
		public static void AssemblyExists_ConfigFileDoesNotExist_Throws(string configFile)
		{
			var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, configFile);

			var exception = Record.Exception(() => commandLine.Parse());

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal("config file not found: " + configFile, exception.Message);
		}

		[Fact]
		public static void SingleAssembly_NoConfigFile()
		{
			var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location);

			var project = commandLine.Parse();

			var assembly = Assert.Single(project.Assemblies);
			Assert.Equal(typeof(CommandLineTests).Assembly.Location, assembly.AssemblyFileName);
			Assert.Null(assembly.ConfigFileName);
		}

		[Fact]
		public static void SingleAssembly_WithConfigFile()
		{
			var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "assembly1.json");

			var project = commandLine.Parse();

			var assembly = Assert.Single(project.Assemblies);
			Assert.Equal(typeof(CommandLineTests).Assembly.Location, assembly.AssemblyFileName);
			var assemblyFolder = Guard.NotNull("Assembly folder cannot be null", Path.GetDirectoryName(typeof(CommandLineTests).Assembly.Location));
			Assert.Equal(Path.Combine(assemblyFolder, "assembly1.json"), assembly.ConfigFileName);
		}

		[Fact]
		public static void MultipleAssemblies_NoConfigFiles()
		{
			var arguments = new[] { typeof(CommandLineTests).Assembly.Location, typeof(CommandLine).Assembly.Location };
			var commandLine = new TestableCommandLine(arguments);

			var project = commandLine.Parse();

			Assert.Collection(
				project.Assemblies,
				a =>
				{
					Assert.Equal(typeof(CommandLineTests).Assembly.Location, a.AssemblyFileName);
					Assert.Null(a.ConfigFileName);
				},
				a =>
				{
					Assert.Equal(typeof(CommandLine).Assembly.Location, a.AssemblyFileName);
					Assert.Null(a.ConfigFileName);
				}
			);
		}

		[Theory]
		[InlineData("assembly2.config")]
		[InlineData("assembly2.json")]
		public static void MultipleAssembliesOneWithConfig(string configFile)
		{
			var arguments = new[] { typeof(CommandLineTests).Assembly.Location, typeof(CommandLine).Assembly.Location, configFile };
			var commandLine = new TestableCommandLine(arguments);

			var project = commandLine.Parse();

			Assert.Collection(
				project.Assemblies,
				item =>
				{
					Assert.Equal(typeof(CommandLineTests).Assembly.Location, item.AssemblyFileName);
					Assert.Null(item.ConfigFileName);
				},
				item =>
				{
					Assert.Equal(typeof(CommandLine).Assembly.Location, item.AssemblyFileName);
					Assert.Equal($"{Path.GetDirectoryName(typeof(CommandLine).Assembly.Location)}{Path.DirectorySeparatorChar}{configFile}", item.ConfigFileName);
				}
			);
		}

		[Theory]
		[InlineData("assembly1.config", "assembly2.config")]
		[InlineData("assembly1.config", "assembly2.json")]
		[InlineData("assembly1.json", "assembly2.config")]
		[InlineData("assembly1.json", "assembly2.json")]
		public static void TwoConfigFiles_Throws(
			string configFile1,
			string configFile2)
		{
			var arguments = new[] { typeof(CommandLineTests).Assembly.Location, configFile1, configFile2 };
			var commandLine = new TestableCommandLine(arguments);

			var exception = Record.Exception(commandLine.Parse);

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal("expecting assembly, got config file: " + configFile2, exception.Message);
		}
	}

	[Collection("Switches Test Collection")]
	public class Switches : IDisposable
	{
		readonly string? _originalNoColorValue;

		public Switches()
		{
			_originalNoColorValue = Environment.GetEnvironmentVariable(TestProjectConfiguration.EnvNameNoColor);
			Environment.SetEnvironmentVariable(TestProjectConfiguration.EnvNameNoColor, null);
		}

		public void Dispose() =>
			Environment.SetEnvironmentVariable(TestProjectConfiguration.EnvNameNoColor, _originalNoColorValue);

		static readonly (string Switch, Expression<Func<XunitProject, bool>> Accessor)[] SwitchOptionsList =
		[
			("-debug", project => project.Configuration.DebugOrDefault),
			("-diagnostics", project => project.Assemblies.All(a => a.Configuration.DiagnosticMessagesOrDefault)),
			("-failskips", project => project.Assemblies.All(a => a.Configuration.FailSkipsOrDefault)),
			("-ignorefailures", project => project.Configuration.IgnoreFailuresOrDefault),
			("-internaldiagnostics", project => project.Assemblies.All(a => a.Configuration.InternalDiagnosticMessagesOrDefault)),
			("-noautoreporters", project => project.Configuration.NoAutoReportersOrDefault),
			("-nocolor", project => project.Configuration.NoColorOrDefault),
			("-nologo", project => project.Configuration.NoLogoOrDefault),
			("-noshadow", project => !project.Assemblies.Single().Configuration.ShadowCopyOrDefault),
			("-pause", project => project.Configuration.PauseOrDefault),
			("-preenumeratetheories", project => project.Assemblies.All(a => a.Configuration.PreEnumerateTheories ?? false)),
			("-showliveoutput", project => project.Assemblies.All(a => a.Configuration.ShowLiveOutputOrDefault)),
			("-stoponfail", project => project.Assemblies.All(a => a.Configuration.StopOnFailOrDefault)),
			("-wait", project => project.Configuration.WaitOrDefault),
		];

		public static readonly TheoryData<string, Expression<Func<XunitProject, bool>>> SwitchesLowerCase =
			new(SwitchOptionsList);

		public static readonly TheoryData<string, Expression<Func<XunitProject, bool>>> SwitchesUpperCase =
			new(SwitchOptionsList.Select(t => (t.Switch.ToUpperInvariant(), t.Accessor)));

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public void SwitchDefault(
			string _,
			Expression<Func<XunitProject, bool>> accessor)
		{
			var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json");
			var project = commandLine.Parse();

			var result = accessor.Compile().Invoke(project);

			Assert.False(result);
		}

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public void SwitchOverride(
			string @switch,
			Expression<Func<XunitProject, bool>> accessor)
		{
			var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", @switch);
			var project = commandLine.Parse();

			var result = accessor.Compile().Invoke(project);

			Assert.True(result);
		}

		[Fact]
		public void NoColorSetsEnvironmentVariable()
		{
			Assert.Null(Environment.GetEnvironmentVariable(TestProjectConfiguration.EnvNameNoColor));

			new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-nocolor").Parse();

			// Any set (non-null, non-empty) value is acceptable, see https://no-color.org/
			var envValue = Environment.GetEnvironmentVariable(TestProjectConfiguration.EnvNameNoColor);
			Assert.NotNull(envValue);
			Assert.NotEmpty(envValue);
		}
	}

	public class OptionsWithArguments
	{
		public class AppDomains
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json");

				var project = commandLine.Parse();

				foreach (var assembly in project.Assemblies)
					Assert.Null(assembly.Configuration.AppDomain);
			}

			[Fact]
			public static void MissingValue()
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-appdomains");

				var exception = Record.Exception(() => commandLine.Parse());

				Assert.IsType<ArgumentException>(exception);
				Assert.Equal("missing argument for -appdomains", exception.Message);
			}

			[Fact]
			public static void InvalidValue()
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-appdomains", "foo");

				var exception = Record.Exception(() => commandLine.Parse());

				Assert.IsType<ArgumentException>(exception);
				Assert.Equal("incorrect argument value for -appdomains (must be 'denied', 'required', or 'ifavailable')", exception.Message);
			}

			[Theory]
			[InlineData("required", AppDomainSupport.Required)]
			[InlineData("denied", AppDomainSupport.Denied)]
			[InlineData("ifavailable", AppDomainSupport.IfAvailable)]
			public static void ValidValues(
				string value,
				AppDomainSupport expected)
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-appdomains", value);

				var project = commandLine.Parse();

				foreach (var assembly in project.Assemblies)
					Assert.Equal(expected, assembly.Configuration.AppDomain);
			}
		}

		public class Culture
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json");

				var project = commandLine.Parse();

				foreach (var assembly in project.Assemblies)
					Assert.Null(assembly.Configuration.Culture);
			}

			[Fact]
			public static void ExplicitDefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-culture", "default");

				var project = commandLine.Parse();

				foreach (var assembly in project.Assemblies)
					Assert.Null(assembly.Configuration.Culture);
			}

			[Fact]
			public static void InvariantCultureIsEmptyString()
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-culture", "invariant");

				var project = commandLine.Parse();

				foreach (var assembly in project.Assemblies)
					Assert.Equal(string.Empty, assembly.Configuration.Culture);
			}

			[Fact]
			public static void ValueIsPreserved()
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-culture", "foo");

				var project = commandLine.Parse();

				foreach (var assembly in project.Assemblies)
					Assert.Equal("foo", assembly.Configuration.Culture);
			}
		}

		public class LongRunning
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json");

				var project = commandLine.Parse();

				foreach (var assembly in project.Assemblies)
				{
					Assert.Null(assembly.Configuration.DiagnosticMessages);
					Assert.Null(assembly.Configuration.LongRunningTestSeconds);
				}
			}

			[Fact]
			public static void MissingValue()
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-longrunning");

				var exception = Record.Exception(commandLine.Parse);

				Assert.IsType<ArgumentException>(exception);
				Assert.Equal("missing argument for -longRunning", exception.Message);
			}

			[Fact]
			public static void InvalidValue()
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-longrunning", "abc");

				var exception = Record.Exception(commandLine.Parse);

				Assert.IsType<ArgumentException>(exception);
				Assert.Equal("incorrect argument value for -longRunning (must be a positive integer)", exception.Message);
			}

			[Fact]
			public static void ValidValue()
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-LONGRUNNING", "123");

				var project = commandLine.Parse();

				foreach (var assembly in project.Assemblies)
				{
					Assert.True(assembly.Configuration.DiagnosticMessages);
					Assert.Equal(123, assembly.Configuration.LongRunningTestSeconds);
				}
			}
		}

		public class MaxThreads
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json");

				var project = commandLine.Parse();

				foreach (var assembly in project.Assemblies)
					Assert.Null(assembly.Configuration.MaxParallelThreads);
			}

			[Fact]
			public static void MissingValue()
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-maxthreads");

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
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-maxthreads", value);

				var exception = Record.Exception(() => commandLine.Parse());

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
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-maxthreads", value);

				var project = commandLine.Parse();

				foreach (var assembly in project.Assemblies)
					Assert.Equal(expected, assembly.Configuration.MaxParallelThreads);
			}

			[Theory]
			[InlineData("2x")]
			[InlineData("2.0x")]
			[InlineData("2,0x")]
			public static void MultiplierValue(string value)
			{
				var expected = Environment.ProcessorCount * 2;
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-maxthreads", value);

				var project = commandLine.Parse();

				foreach (var assembly in project.Assemblies)
					Assert.Equal(expected, assembly.Configuration.MaxParallelThreads);
			}
		}

		public class Parallelization
		{
			[Fact]
			public static void ParallelizationOptionsAreNullByDefault()
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json");

				var project = commandLine.Parse();

				foreach (var assembly in project.Assemblies)
				{
					Assert.Null(assembly.Configuration.ParallelizeAssembly);
					Assert.Null(assembly.Configuration.ParallelizeTestCollections);
				}
			}

			[Fact]
			public static void FailsWithoutOptionOrWithIncorrectOptions()
			{
				var commandLine1 = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-parallel");
				var exception1 = Record.Exception(() => commandLine1.Parse());
				Assert.IsType<ArgumentException>(exception1);
				Assert.Equal("missing argument for -parallel", exception1.Message);

				var commandLine2 = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-parallel", "nonsense");
				var exception2 = Record.Exception(() => commandLine2.Parse());
				Assert.IsType<ArgumentException>(exception2);
				Assert.Equal("incorrect argument value for -parallel", exception2.Message);
			}

			[Theory]
			[InlineData("none", false, false)]
			[InlineData("collections", false, true)]
			[InlineData("assemblies", true, false)]
			[InlineData("all", true, true)]
			public static void ParallelCanBeTurnedOn(
				string parallelOption,
				bool expectedAssembliesParallelization,
				bool expectedCollectionsParallelization)
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", "-parallel", parallelOption);

				var project = commandLine.Parse();

				foreach (var assembly in project.Assemblies)
				{
					Assert.Equal(expectedAssembliesParallelization, assembly.Configuration.ParallelizeAssembly);
					Assert.Equal(expectedCollectionsParallelization, assembly.Configuration.ParallelizeTestCollections);
				}
			}
		}
	}

	public class Filters
	{
		[Fact]
		public static void DefaultFilters()
		{
			var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json");

			var project = commandLine.Parse();

			var filters = project.Assemblies.Single().Configuration.Filters;
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
			var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", @switch);

			var exception = Record.Exception(commandLine.Parse);

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal($"missing argument for {@switch.ToLowerInvariant()}", exception.Message);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void SingleValidArgument(string @switch)
		{
			var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", @switch, "value1");

			var project = commandLine.Parse();

			Assert.Collection(
				project.Assemblies.Single().Configuration.Filters.ToXunit3Arguments(),
				arg => Assert.Equal(@switch.ToLowerInvariant(), arg),
				arg => Assert.Equal("value1", arg)
			);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void MultipleValidArguments(string @switch)
		{
			var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", @switch, "value2", @switch, "value1");

			var project = commandLine.Parse();

			Assert.Collection(
				project.Assemblies.Single().Configuration.Filters.ToXunit3Arguments(),
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
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", @switch, "foo=bar");

				var project = commandLine.Parse();

				Assert.Collection(
					project.Assemblies.Single().Configuration.Filters.ToXunit3Arguments(),
					arg => Assert.Equal(@switch.ToLowerInvariant(), arg),
					arg => Assert.Equal("foo=bar", arg)
				);
			}

			[Theory]
			[MemberData(nameof(SwitchesLowerCase))]
			[MemberData(nameof(SwitchesUpperCase))]
			public static void MultipleValidTraitArguments(string @switch)
			{
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", @switch, "foo=bar", @switch, "foo=baz");

				var project = commandLine.Parse();

				Assert.Collection(
					project.Assemblies.Single().Configuration.Filters.ToXunit3Arguments(),
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
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", @switch);

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
				var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", @switch, optionValue);

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
			var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", @switch);

			var exception = Record.Exception(() => commandLine.Parse());

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal($"missing filename for {@switch}", exception.Message);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void Output(string @switch)
		{
			var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json", @switch, "outputFile");

			var project = commandLine.Parse();

			var output = Assert.Single(project.Configuration.Output);
			Assert.Equal(@switch.Substring(1), output.Key, ignoreCase: true);
			Assert.Equal("outputFile", output.Value);
		}
	}

	[Collection(nameof(EnvironmentHelper.NullifyEnvironmentalReporters))]
	public class Reporters : IDisposable
	{
		readonly IDisposable environmentCleanup;

		public Reporters() =>
			environmentCleanup = EnvironmentHelper.NullifyEnvironmentalReporters();

		public void Dispose() =>
			environmentCleanup.Dispose();

		[Fact]
		public void NoReporters_UsesDefaultReporter()
		{
			var commandLine = new TestableCommandLine(typeof(CommandLineTests).Assembly.Location, "no-config.json");

			var project = commandLine.Parse();

			Assert.IsType<DefaultRunnerReporter>(project.RunnerReporter);
		}

		[Fact]
		public void NoExplicitReporter_NoEnvironmentallyEnabledReporters_UsesDefaultReporter()
		{
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: false);
			var commandLine = new TestableCommandLine([implicitReporter], typeof(CommandLineTests).Assembly.Location, "no-config.json");

			var project = commandLine.Parse();

			Assert.IsType<DefaultRunnerReporter>(project.RunnerReporter);
		}

		[Fact]
		public void ExplicitReporter_NoEnvironmentalOverride_UsesExplicitReporter()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");
			var commandLine = new TestableCommandLine([explicitReporter], typeof(CommandLineTests).Assembly.Location, "no-config.json", "-reporter", "switch");

			var project = commandLine.Parse();

			Assert.Same(explicitReporter, project.RunnerReporter);
		}

		[Fact]
		public void ExplicitReporter_WithEnvironmentalOverride_UsesEnvironmentalOverride()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
			var commandLine = new TestableCommandLine([explicitReporter, implicitReporter], typeof(CommandLineTests).Assembly.Location, "no-config.json", "-reporter", "switch");

			var project = commandLine.Parse();

			Assert.Same(implicitReporter, project.RunnerReporter);
		}

		[Fact]
		public void WithEnvironmentalOverride_WithEnvironmentalOverridesDisabled_UsesDefaultReporter()
		{
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
			var commandLine = new TestableCommandLine([implicitReporter], typeof(CommandLineTests).Assembly.Location, "no-config.json", "-noautoreporters");

			var project = commandLine.Parse();

			Assert.IsType<DefaultRunnerReporter>(project.RunnerReporter);
		}

		[Fact]
		public void NoExplicitReporter_SelectsFirstEnvironmentallyEnabledReporter()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");
			var implicitReporter1 = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
			var implicitReporter2 = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
			var commandLine = new TestableCommandLine([explicitReporter, implicitReporter1, implicitReporter2], typeof(CommandLineTests).Assembly.Location, "no-config.json");

			var project = commandLine.Parse();

			Assert.Same(implicitReporter1, project.RunnerReporter);
		}
	}

	class TestableCommandLine : CommandLine
	{
		public TestableCommandLine(params string[] args)
			: base(new ConsoleHelper(TextReader.Null, TextWriter.Null), [], args)
		{ }

		public TestableCommandLine(
			IReadOnlyList<IRunnerReporter> reporters,
			params string[] args)
				: base(new ConsoleHelper(TextReader.Null, TextWriter.Null), reporters, args)
		{ }

		protected override bool FileExists(string? path) =>
			path?.StartsWith("bad") != true && path != "fileName";
	}
}
