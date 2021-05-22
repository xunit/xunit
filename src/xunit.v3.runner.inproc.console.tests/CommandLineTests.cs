using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.InProc.SystemConsole;
using Xunit.v3;

public class CommandLineTests
{
	public class UnknownOption
	{
		[Fact]
		public static void UnknownOptionThrows()
		{
			var commandLine = TestableCommandLine.Parse("-unknown");

			Assert.IsType<ArgumentException>(commandLine.ParseFault);
			Assert.Equal("unknown option: -unknown", commandLine.ParseFault.Message);
		}
	}

	public class Project
	{
		[Fact]
		public static void DefaultValues()
		{
			var commandLine = TestableCommandLine.Parse();

			var assembly = Assert.Single(commandLine.Project);
			Assert.Null(assembly.ConfigFilename);
		}

		[Fact]
		public static void ConfigFileDoesNotExist_Throws()
		{
			var commandLine = TestableCommandLine.Parse("badConfig.json");

			Assert.IsType<ArgumentException>(commandLine.ParseFault);
			Assert.Equal("config file not found: badConfig.json", commandLine.ParseFault.Message);
		}

		[Fact]
		public static void ConfigFileUnsupportedFormat_Throws()
		{
			var commandLine = TestableCommandLine.Parse("assembly1.config");

			Assert.IsType<ArgumentException>(commandLine.ParseFault);
			Assert.Equal("expecting config file, got: assembly1.config", commandLine.ParseFault.Message);
		}

		[Fact]
		public static void TwoConfigFiles_Throws()
		{
			var commandLine = TestableCommandLine.Parse("assembly1.json", "assembly2.json");

			Assert.IsType<ArgumentException>(commandLine.ParseFault);
			Assert.Equal("expected option, instead got: assembly2.json", commandLine.ParseFault.Message);
		}

		[Fact]
		public static void WithConfigFile()
		{
			var commandLine = TestableCommandLine.Parse("assembly1.json");

			var assembly = Assert.Single(commandLine.Project);
			Assert.Equal("/full/path/assembly1.json", assembly.ConfigFilename);
		}
	}

	public class Switches
	{
		static readonly (string Switch, Expression<Func<CommandLine, bool>> Accessor)[] SwitchOptionsList = new (string, Expression<Func<CommandLine, bool>>)[]
		{
			("-debug", cmd => cmd.Project.Configuration.DebugOrDefault),
			("-diagnostics", cmd => cmd.Project.Assemblies.All(a => a.Configuration.DiagnosticMessagesOrDefault)),
			("-failskips", cmd => cmd.Project.Assemblies.All(a => a.Configuration.FailSkipsOrDefault)),
			("-internaldiagnostics", cmd => cmd.Project.Assemblies.All(a => a.Configuration.InternalDiagnosticMessagesOrDefault)),
			("-noautoreporters", cmd => cmd.Project.Configuration.NoAutoReportersOrDefault),
			("-nocolor", cmd => cmd.Project.Configuration.NoColorOrDefault),
			("-nologo", cmd => cmd.Project.Configuration.NoLogoOrDefault),
			("-pause", cmd => cmd.Project.Configuration.PauseOrDefault),
			("-preenumeratetheories", cmd => cmd.Project.Assemblies.All(a => a.Configuration.PreEnumerateTheories ?? false)),
			("-stoponfail", cmd => cmd.Project.Assemblies.All(a => a.Configuration.StopOnFailOrDefault)),
			("-wait", cmd => cmd.Project.Configuration.WaitOrDefault),
		};

		public static readonly TheoryData<string, Expression<Func<CommandLine, bool>>> SwitchesLowerCase =
			new TheoryData<string, Expression<Func<CommandLine, bool>>>(
				SwitchOptionsList
			);

		public static readonly TheoryData<string, Expression<Func<CommandLine, bool>>> SwitchesUpperCase =
			new TheoryData<string, Expression<Func<CommandLine, bool>>>(
				SwitchOptionsList.Select(t => (t.Switch.ToUpperInvariant(), t.Accessor))
			);

		[Theory]
		[MemberData(nameof(SwitchesLowerCase), DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(SwitchesUpperCase), DisableDiscoveryEnumeration = true)]
		public static void SwitchDefault(
			string _,
			Expression<Func<CommandLine, bool>> accessor)
		{
			var commandLine = TestableCommandLine.Parse("no-config.json");

			var result = accessor.Compile().Invoke(commandLine);

			Assert.False(result);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase), DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(SwitchesUpperCase), DisableDiscoveryEnumeration = true)]
		public static void SwitchOverride(
			string @switch,
			Expression<Func<CommandLine, bool>> accessor)
		{
			var commandLine = TestableCommandLine.Parse("no-config.json", @switch);

			var result = accessor.Compile().Invoke(commandLine);

			Assert.True(result);
		}
	}

	public class OptionsWithArguments
	{
		public class MaxThreads
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = TestableCommandLine.Parse("no-config.json");

				foreach (var assembly in commandLine.Project.Assemblies)
					Assert.Null(assembly.Configuration.MaxParallelThreads);
			}

			[Fact]
			public static void MissingValue()
			{
				var commandLine = TestableCommandLine.Parse("no-config.json", "-maxthreads");

				Assert.IsType<ArgumentException>(commandLine.ParseFault);
				Assert.Equal("missing argument for -maxthreads", commandLine.ParseFault.Message);
			}

			[Theory]
			[InlineData("0")]
			[InlineData("abc")]
			public static void InvalidValues(string value)
			{
				var commandLine = TestableCommandLine.Parse("no-config.json", "-maxthreads", value);

				Assert.IsType<ArgumentException>(commandLine.ParseFault);
				Assert.Equal("incorrect argument value for -maxthreads (must be 'default', 'unlimited', or a positive number)", commandLine.ParseFault.Message);
			}

			[Theory]
			[InlineData("default", 0)]
			[InlineData("unlimited", -1)]
			[InlineData("16", 16)]
			public static void ValidValues(
				string value,
				int expected)
			{
				var commandLine = TestableCommandLine.Parse("no-config.json", "-maxthreads", value);

				foreach (var assembly in commandLine.Project.Assemblies)
					Assert.Equal(expected, assembly.Configuration.MaxParallelThreads);
			}
		}

		public class Parallelization
		{
			[Fact]
			public static void ParallelizationOptionsAreNullByDefault()
			{
				var commandLine = TestableCommandLine.Parse("no-config.json");

				foreach (var assembly in commandLine.Project.Assemblies)
					Assert.Null(assembly.Configuration.ParallelizeTestCollections);
			}

			[Fact]
			public static void FailsWithoutOptionOrWithIncorrectOptions()
			{
				var commandLine1 = TestableCommandLine.Parse("no-config.json", "-parallel");
				Assert.IsType<ArgumentException>(commandLine1.ParseFault);
				Assert.Equal("missing argument for -parallel", commandLine1.ParseFault.Message);

				var commandLine2 = TestableCommandLine.Parse("no-config.json", "-parallel", "nonsense");
				Assert.IsType<ArgumentException>(commandLine2.ParseFault);
				Assert.Equal("incorrect argument value for -parallel", commandLine2.ParseFault.Message);
			}

			[Theory]
			[InlineData("none", false)]
			[InlineData("collections", true)]
			public static void ParallelCanBeTurnedOn(
				string parallelOption,
				bool expectedCollectionsParallelization)
			{
				var commandLine = TestableCommandLine.Parse("no-config.json", "-parallel", parallelOption);

				foreach (var assembly in commandLine.Project.Assemblies)
					Assert.Equal(expectedCollectionsParallelization, assembly.Configuration.ParallelizeTestCollections);
			}
		}
	}

	public class Filters
	{
		[Fact]
		public static void DefaultFilters()
		{
			var commandLine = TestableCommandLine.Parse("no-config.json");

			var filters = commandLine.Project.Assemblies.Single().Configuration.Filters;
			Assert.Equal(0, filters.IncludedTraits.Count);
			Assert.Equal(0, filters.ExcludedTraits.Count);
			Assert.Equal(0, filters.IncludedNamespaces.Count);
			Assert.Equal(0, filters.ExcludedNamespaces.Count);
			Assert.Equal(0, filters.IncludedClasses.Count);
			Assert.Equal(0, filters.ExcludedClasses.Count);
			Assert.Equal(0, filters.IncludedMethods.Count);
			Assert.Equal(0, filters.ExcludedMethods.Count);
		}

		static readonly (string Switch, Expression<Func<CommandLine, ICollection<string>>> Accessor)[] SwitchOptionsList =
			new (string, Expression<Func<CommandLine, ICollection<string>>>)[]
			{
				("-namespace", cmd => cmd.Project.Assemblies.Single().Configuration.Filters.IncludedNamespaces),
				("-nonamespace", cmd => cmd.Project.Assemblies.Single().Configuration.Filters.ExcludedNamespaces),
				("-class", cmd => cmd.Project.Assemblies.Single().Configuration.Filters.IncludedClasses),
				("-noclass", cmd => cmd.Project.Assemblies.Single().Configuration.Filters.ExcludedClasses),
				("-method", cmd => cmd.Project.Assemblies.Single().Configuration.Filters.IncludedMethods),
				("-nomethod", cmd => cmd.Project.Assemblies.Single().Configuration.Filters.ExcludedMethods),
			};

		public static readonly TheoryData<string, Expression<Func<CommandLine, ICollection<string>>>> SwitchesLowerCase =
			new TheoryData<string, Expression<Func<CommandLine, ICollection<string>>>>(
				SwitchOptionsList
			);

		public static readonly TheoryData<string, Expression<Func<CommandLine, ICollection<string>>>> SwitchesUpperCase =
			new TheoryData<string, Expression<Func<CommandLine, ICollection<string>>>>(
				SwitchOptionsList.Select(t => (t.Switch.ToUpperInvariant(), t.Accessor))
			);

		[Theory]
		[MemberData(nameof(SwitchesLowerCase), DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(SwitchesUpperCase), DisableDiscoveryEnumeration = true)]
		public static void MissingOptionValue(
			string @switch,
			Expression<Func<CommandLine, ICollection<string>>> _)
		{
			var commandLine = TestableCommandLine.Parse("no-config.json", @switch);

			Assert.IsType<ArgumentException>(commandLine.ParseFault);
			Assert.Equal($"missing argument for {@switch.ToLowerInvariant()}", commandLine.ParseFault.Message);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase), DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(SwitchesUpperCase), DisableDiscoveryEnumeration = true)]
		public static void SingleValidArgument(
			string @switch,
			Expression<Func<CommandLine, ICollection<string>>> accessor)
		{
			var commandLine = TestableCommandLine.Parse("no-config.json", @switch, "value1");

			var results = accessor.Compile().Invoke(commandLine);

			var item = Assert.Single(results.OrderBy(x => x));
			Assert.Equal("value1", item);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase), DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(SwitchesUpperCase), DisableDiscoveryEnumeration = true)]
		public static void MultipleValidArguments(
			string @switch,
			Expression<Func<CommandLine, ICollection<string>>> accessor)
		{
			var commandLine = TestableCommandLine.Parse("no-config.json", @switch, "value2", @switch, "value1");

			var results = accessor.Compile().Invoke(commandLine);

			Assert.Collection(results.OrderBy(x => x),
				item => Assert.Equal("value1", item),
				item => Assert.Equal("value2", item)
			);
		}

		public class Traits
		{
			static readonly (string Switch, Expression<Func<CommandLine, Dictionary<string, List<string>>>> Accessor)[] SwitchOptionsList =
				new (string Switch, Expression<Func<CommandLine, Dictionary<string, List<string>>>> Accessor)[]
				{
					("-trait", cmd => cmd.Project.Assemblies.Single().Configuration.Filters.IncludedTraits),
					("-notrait", cmd => cmd.Project.Assemblies.Single().Configuration.Filters.ExcludedTraits),
				};

			static readonly string[] BadFormatValues =
				new string[]
				{
					// Missing equals
					"foobar",
					// Missing value
					"foo=",
					// Missing name
					"=bar",
					// Double equal signs
					"foo=bar=baz",
				};

			public static readonly TheoryData<string, Expression<Func<CommandLine, Dictionary<string, List<string>>>>> SwitchesLowerCase =
				new TheoryData<string, Expression<Func<CommandLine, Dictionary<string, List<string>>>>>(
					SwitchOptionsList
				);

			public static readonly TheoryData<string, Expression<Func<CommandLine, Dictionary<string, List<string>>>>> SwitchesUpperCase =
				new TheoryData<string, Expression<Func<CommandLine, Dictionary<string, List<string>>>>>(
					SwitchOptionsList.Select(x => (x.Switch.ToUpperInvariant(), x.Accessor))
				);

			public static readonly TheoryData<string, string> SwitchesWithOptionsLowerCase =
				new TheoryData<string, string>(
					SwitchOptionsList.SelectMany(
						tuple => BadFormatValues.Select(value => (tuple.Switch, value))
					)
				);

			public static readonly TheoryData<string, string> SwitchesWithOptionsUpperCase =
				new TheoryData<string, string>(
					SwitchOptionsList.SelectMany(
						tuple => BadFormatValues.Select(value => (tuple.Switch.ToUpperInvariant(), value))
					)
				);

			[Theory]
			[MemberData(nameof(SwitchesLowerCase), DisableDiscoveryEnumeration = true)]
			[MemberData(nameof(SwitchesUpperCase), DisableDiscoveryEnumeration = true)]
			public static void SingleValidTraitArgument(
				string @switch,
				Expression<Func<CommandLine, Dictionary<string, List<string>>>> accessor)
			{
				var commandLine = TestableCommandLine.Parse("no-config.json", @switch, "foo=bar");

				var traits = accessor.Compile().Invoke(commandLine);
				Assert.Equal(1, traits.Count);
				Assert.Equal(1, traits["foo"].Count());
				Assert.Contains("bar", traits["foo"]);
			}

			[Theory]
			[MemberData(nameof(SwitchesLowerCase), DisableDiscoveryEnumeration = true)]
			[MemberData(nameof(SwitchesUpperCase), DisableDiscoveryEnumeration = true)]
			public static void MultipleValidTraitArguments_SameName(
				string @switch,
				Expression<Func<CommandLine, Dictionary<string, List<string>>>> accessor)
			{
				var commandLine = TestableCommandLine.Parse("no-config.json", @switch, "foo=bar", @switch, "foo=baz");

				var traits = accessor.Compile().Invoke(commandLine);
				Assert.Equal(1, traits.Count);
				Assert.Equal(2, traits["foo"].Count());
				Assert.Contains("bar", traits["foo"]);
				Assert.Contains("baz", traits["foo"]);
			}

			[Theory]
			[MemberData(nameof(SwitchesLowerCase), DisableDiscoveryEnumeration = true)]
			[MemberData(nameof(SwitchesUpperCase), DisableDiscoveryEnumeration = true)]
			public static void MultipleValidTraitArguments_DifferentName(
				string @switch,
				Expression<Func<CommandLine, Dictionary<string, List<string>>>> accessor)
			{
				var commandLine = TestableCommandLine.Parse("no-config.json", @switch, "foo=bar", @switch, "baz=biff");

				var traits = accessor.Compile().Invoke(commandLine);
				Assert.Equal(2, traits.Count);
				Assert.Equal(1, traits["foo"].Count());
				Assert.Contains("bar", traits["foo"]);
				Assert.Equal(1, traits["baz"].Count());
				Assert.Contains("biff", traits["baz"]);
			}

			[Theory]
			[MemberData(nameof(SwitchesLowerCase), DisableDiscoveryEnumeration = true)]
			[MemberData(nameof(SwitchesUpperCase), DisableDiscoveryEnumeration = true)]
			public static void MissingOptionValue(
				string @switch,
				Expression<Func<CommandLine, Dictionary<string, List<string>>>> _)
			{
				var commandLine = TestableCommandLine.Parse("no-config.json", @switch);

				Assert.IsType<ArgumentException>(commandLine.ParseFault);
				Assert.Equal($"missing argument for {@switch.ToLowerInvariant()}", commandLine.ParseFault.Message);
			}

			[Theory]
			[MemberData(nameof(SwitchesWithOptionsLowerCase))]
			[MemberData(nameof(SwitchesWithOptionsUpperCase))]
			public static void ImproperlyFormattedOptionValue(
				string @switch,
				string optionValue)
			{
				var commandLine = TestableCommandLine.Parse("no-config.json", @switch, optionValue);

				Assert.IsType<ArgumentException>(commandLine.ParseFault);
				Assert.Equal($"incorrect argument format for {@switch.ToLowerInvariant()} (should be \"name=value\")", commandLine.ParseFault.Message);
			}
		}
	}

	public class Transforms
	{
		public static readonly TheoryData<string> SwitchesLowerCase =
			new TheoryData<string>(TransformFactory.AvailableTransforms.Select(x => $"-{x.ID}"));

		public static readonly TheoryData<string> SwitchesUpperCase =
			new TheoryData<string>(TransformFactory.AvailableTransforms.Select(x => $"-{x.ID.ToUpperInvariant()}"));

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void OutputMissingFilename(string @switch)
		{
			var commandLine = TestableCommandLine.Parse("no-config.json", @switch);

			Assert.IsType<ArgumentException>(commandLine.ParseFault);
			Assert.Equal($"missing filename for {@switch}", commandLine.ParseFault.Message);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void Output(string @switch)
		{
			var commandLine = TestableCommandLine.Parse("no-config.json", @switch, "outputFile");

			var output = Assert.Single(commandLine.Project.Configuration.Output);
			Assert.Equal(@switch.Substring(1).ToLowerInvariant(), output.Key);
			Assert.Equal("outputFile", output.Value);
		}
	}

	public class Reporters
	{
		[Fact]
		public void NoReporters_UsesDefaultReporter()
		{
			var commandLine = TestableCommandLine.Parse("no-config.json");

			Assert.IsType<DefaultRunnerReporter>(commandLine.Reporter);
		}

		[Fact]
		public void NoExplicitReporter_NoEnvironmentallyEnabledReporters_UsesDefaultReporter()
		{
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: false);

			var commandLine = TestableCommandLine.Parse(new[] { implicitReporter }, "no-config.json");

			Assert.IsType<DefaultRunnerReporter>(commandLine.Reporter);
		}

		[Fact]
		public void ExplicitReporter_NoEnvironmentalOverride_UsesExplicitReporter()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");

			var commandLine = TestableCommandLine.Parse(new[] { explicitReporter }, "no-config.json", "-switch");

			Assert.Same(explicitReporter, commandLine.Reporter);
		}

		[Fact]
		public void ExplicitReporter_WithEnvironmentalOverride_UsesEnvironmentalOverride()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);

			var commandLine = TestableCommandLine.Parse(new[] { explicitReporter, implicitReporter }, "no-config.json", "-switch");

			Assert.Same(implicitReporter, commandLine.Reporter);
		}

		[Fact]
		public void WithEnvironmentalOverride_WithEnvironmentalOverridesDisabled_UsesDefaultReporter()
		{
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);

			var commandLine = TestableCommandLine.Parse(new[] { implicitReporter }, "no-config.json", "-noautoreporters");

			Assert.IsType<DefaultRunnerReporter>(commandLine.Reporter);
		}

		[Fact]
		public void NoExplicitReporter_SelectsFirstEnvironmentallyEnabledReporter()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");
			var implicitReporter1 = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
			var implicitReporter2 = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);

			var commandLine = TestableCommandLine.Parse(new[] { explicitReporter, implicitReporter1, implicitReporter2 }, "no-config.json");

			Assert.Same(implicitReporter1, commandLine.Reporter);
		}
	}

	class TestableCommandLine : CommandLine
	{
		public readonly IRunnerReporter? Reporter;

		private TestableCommandLine(
			IReadOnlyList<IRunnerReporter> reporters,
			params string[] arguments)
				: base(Assembly.GetExecutingAssembly(), "assemblyName.dll", arguments, filename => !filename.StartsWith("badConfig.") && filename != "fileName")
		{
			if (ParseFault == null)
			{
				try
				{
					Reporter = ChooseReporter(reporters);
				}
				catch (Exception ex)
				{
					ParseFault = ex;
				}
			}
		}

		protected override string GetFullPath(string fileName) => $"/full/path/{fileName}";

		public static TestableCommandLine Parse(params string[] arguments) =>
			new TestableCommandLine(new IRunnerReporter[0], arguments);

		public static TestableCommandLine Parse(
			IReadOnlyList<IRunnerReporter> reporters,
			params string[] arguments) =>
				new TestableCommandLine(reporters, arguments);
	}
}
