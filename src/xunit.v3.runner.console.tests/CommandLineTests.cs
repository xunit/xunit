using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.SystemConsole;

public class CommandLineTests
{
	public class UnknownOption
	{
		[Fact]
		public static void UnknownOptionThrows()
		{
			var exception = Record.Exception(() => TestableCommandLine.Parse("-unknown"));

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal("unknown option: -unknown", exception.Message);
		}
	}

	public class Project
	{
		[Fact]
		public static void AssemblyDoesNotExist_Throws()
		{
			var exception = Record.Exception(() => TestableCommandLine.Parse("badAssembly.dll"));

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal("file not found: badAssembly.dll", exception.Message);
		}

		[Theory]
		[InlineData("badConfig.config")]
		[InlineData("badConfig.json")]
		public static void AssemblyExists_ConfigFileDoesNotExist_Throws(string configFile)
		{
			var exception = Record.Exception(() => TestableCommandLine.Parse("assembly1.dll", configFile));

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal("config file not found: " + configFile, exception.Message);
		}

		[Fact]
		public static void SingleAssembly_NoConfigFile()
		{
			var commandLine = TestableCommandLine.Parse("assembly1.dll");

			var assembly = Assert.Single(commandLine.Project);
			Assert.Equal("/full/path/assembly1.dll", assembly.AssemblyFilename);
			Assert.Null(assembly.ConfigFilename);
		}

		[Fact]
		public static void SingleAssembly_WithConfigFile()
		{
			var commandLine = TestableCommandLine.Parse("assembly1.dll", "assembly1.json");

			var assembly = Assert.Single(commandLine.Project);
			Assert.Equal("/full/path/assembly1.dll", assembly.AssemblyFilename);
			Assert.Equal("/full/path/assembly1.json", assembly.ConfigFilename);
		}

		[Fact]
		public static void MultipleAssemblies_NoConfigFiles()
		{
			var arguments = new[] { "assemblyName.dll", "assemblyName2.dll" };

			var result = TestableCommandLine.Parse(arguments);

			Assert.Collection(
				result.Project,
				a =>
				{
					Assert.Equal("/full/path/assemblyName.dll", a.AssemblyFilename);
					Assert.Null(a.ConfigFilename);
				},
				a =>
				{
					Assert.Equal("/full/path/assemblyName2.dll", a.AssemblyFilename);
					Assert.Null(a.ConfigFilename);
				}
			);
		}

		[Theory]
		[InlineData("assembly2.config")]
		[InlineData("assembly2.json")]
		public static void MultipleAssembliesOneWithConfig(string configFile)
		{
			var arguments = new[] { "assemblyName.dll", "assemblyName2.dll", configFile };

			var result = TestableCommandLine.Parse(arguments);

			Assert.Collection(
				result.Project,
				item =>
				{
					Assert.Equal("/full/path/assemblyName.dll", item.AssemblyFilename);
					Assert.Null(item.ConfigFilename);
				},
				item =>
				{
					Assert.Equal("/full/path/assemblyName2.dll", item.AssemblyFilename);
					Assert.Equal($"/full/path/{configFile}", item.ConfigFilename);
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
			var arguments = new[] { "assemblyName.dll", configFile1, configFile2 };

			var exception = Record.Exception(() => TestableCommandLine.Parse(arguments));

			Assert.IsType<ArgumentException>(exception);
			Assert.Equal("expecting assembly, got config file: " + configFile2, exception.Message);
		}
	}

	public class Switches
	{
		static readonly (string Switch, Expression<Func<CommandLine, bool>> Accessor)[] SwitchOptionsList = new (string, Expression<Func<CommandLine, bool>>)[]
		{
			("-debug", cmd => cmd.Debug),
			("-diagnostics", cmd => cmd.DiagnosticMessages),
			("-failskips", cmd => cmd.FailSkips),
			("-internaldiagnostics", cmd => cmd.InternalDiagnosticMessages),
			("-noautoreporters", cmd => cmd.NoAutoReporters),
			("-nocolor", cmd => cmd.NoColor),
			("-nologo", cmd => cmd.NoLogo),
			("-noshadow", cmd => !cmd.Project.Assemblies.Single().Configuration.ShadowCopyOrDefault),
			("-pause", cmd => cmd.Pause),
			("-serialize", cmd => cmd.Serialize),
			("-stoponfail", cmd => cmd.StopOnFail),
			("-wait", cmd => cmd.Wait),
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
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void SwitchOptionIsFalseByDefault(
			string _,
			Expression<Func<CommandLine, bool>> accessor)
		{
			var commandLine = TestableCommandLine.Parse("assembly1.dll");

			var result = accessor.Compile().Invoke(commandLine);

			Assert.False(result);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void SwitchOptionIsTrueWhenSpecified(
			string @switch,
			Expression<Func<CommandLine, bool>> accessor)
		{
			var commandLine = TestableCommandLine.Parse("assembly1.dll", @switch);

			var result = accessor.Compile().Invoke(commandLine);

			Assert.True(result);
		}
	}

	public class OptionsWithArguments
	{
		public class AppDomains
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = TestableCommandLine.Parse("assemblyName.dll");

				Assert.Null(commandLine.AppDomains);
			}

			[Fact]
			public static void MissingValue()
			{
				var ex = Assert.Throws<ArgumentException>(() => TestableCommandLine.Parse("assemblyName.dll", "-appdomains"));

				Assert.Equal("missing argument for -appdomains", ex.Message);
			}

			[Fact]
			public static void InvalidValue()
			{
				var ex = Assert.Throws<ArgumentException>(() => TestableCommandLine.Parse("assemblyName.dll", "-appdomains", "foo"));

				Assert.Equal("incorrect argument value for -appdomains (must be 'ifavailable', 'required', or 'denied')", ex.Message);
			}

			[Theory]
			[InlineData("ifavailable", AppDomainSupport.IfAvailable)]
#if NETFRAMEWORK
			[InlineData("required", AppDomainSupport.Required)]
#endif
			[InlineData("denied", AppDomainSupport.Denied)]
			public static void ValidValues(
				string value,
				AppDomainSupport expected)
			{
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "-appdomains", value);

				Assert.Equal(expected, commandLine.AppDomains);
			}
		}

		public class MaxThreads
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = TestableCommandLine.Parse();

				Assert.Null(commandLine.MaxParallelThreads);
			}

			[Fact]
			public static void MissingValue()
			{
				var ex = Assert.Throws<ArgumentException>(() => TestableCommandLine.Parse("-maxthreads"));

				Assert.Equal("missing argument for -maxthreads", ex.Message);
			}

			[Theory]
			[InlineData("0")]
			[InlineData("abc")]
			public static void InvalidValues(string value)
			{
				var ex = Assert.Throws<ArgumentException>(() => TestableCommandLine.Parse("-maxthreads", value));

				Assert.Equal("incorrect argument value for -maxthreads (must be 'default', 'unlimited', or a positive number)", ex.Message);
			}

			[Theory]
			[InlineData("default", 0)]
			[InlineData("unlimited", -1)]
			[InlineData("16", 16)]
			public static void ValidValues(
				string value,
				int expected)
			{
				var commandLine = TestableCommandLine.Parse("-maxthreads", value);

				Assert.Equal(expected, commandLine.MaxParallelThreads);
			}
		}

		public class Parallelization
		{
			[Fact]
			public static void ParallelizationOptionsAreNullByDefault()
			{
				var commandLine = TestableCommandLine.Parse();

				Assert.Null(commandLine.ParallelizeAssemblies);
				Assert.Null(commandLine.ParallelizeTestCollections);
			}

			[Fact]
			public static void FailsWithoutOptionOrWithIncorrectOptions()
			{
				var aex1 = Assert.Throws<ArgumentException>(() => TestableCommandLine.Parse("-parallel"));
				Assert.Equal("missing argument for -parallel", aex1.Message);

				var aex2 = Assert.Throws<ArgumentException>(() => TestableCommandLine.Parse("-parallel", "nonsense"));
				Assert.Equal("incorrect argument value for -parallel", aex2.Message);
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
				var commandLine = TestableCommandLine.Parse("-parallel", parallelOption);

				Assert.Equal(expectedAssembliesParallelization, commandLine.ParallelizeAssemblies);
				Assert.Equal(expectedCollectionsParallelization, commandLine.ParallelizeTestCollections);
			}
		}
	}

	public class Filters
	{
		[Fact]
		public static void DefaultFilters()
		{
			var commandLine = TestableCommandLine.Parse();

			Assert.Equal(0, commandLine.Project.Filters.IncludedTraits.Count);
			Assert.Equal(0, commandLine.Project.Filters.ExcludedTraits.Count);
			Assert.Equal(0, commandLine.Project.Filters.IncludedNamespaces.Count);
			Assert.Equal(0, commandLine.Project.Filters.ExcludedNamespaces.Count);
			Assert.Equal(0, commandLine.Project.Filters.IncludedClasses.Count);
			Assert.Equal(0, commandLine.Project.Filters.ExcludedClasses.Count);
			Assert.Equal(0, commandLine.Project.Filters.IncludedMethods.Count);
			Assert.Equal(0, commandLine.Project.Filters.ExcludedMethods.Count);
		}

		static readonly (string Switch, Expression<Func<CommandLine, ICollection<string>>> Accessor)[] SwitchOptionsList =
			new (string, Expression<Func<CommandLine, ICollection<string>>>)[]
			{
				("-namespace", cmd => cmd.Project.Filters.IncludedNamespaces),
				("-nonamespace", cmd => cmd.Project.Filters.ExcludedNamespaces),
				("-class", cmd => cmd.Project.Filters.IncludedClasses),
				("-noclass", cmd => cmd.Project.Filters.ExcludedClasses),
				("-method", cmd => cmd.Project.Filters.IncludedMethods),
				("-nomethod", cmd => cmd.Project.Filters.ExcludedMethods),
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
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void MissingOptionValue(
			string @switch,
			Expression<Func<CommandLine, ICollection<string>>> _)
		{
			var ex = Record.Exception(() => TestableCommandLine.Parse(@switch));

			Assert.IsType<ArgumentException>(ex);
			Assert.Equal($"missing argument for {@switch.ToLowerInvariant()}", ex.Message);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void SingleValidArgument(
			string @switch,
			Expression<Func<CommandLine, ICollection<string>>> accessor)
		{
			var commandLine = TestableCommandLine.Parse(@switch, "value1");

			var results = accessor.Compile().Invoke(commandLine);

			Assert.Collection(results.OrderBy(x => x),
				item => Assert.Equal("value1", item)
			);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void MultipleValidArguments(
			string @switch,
			Expression<Func<CommandLine, ICollection<string>>> accessor)
		{
			var commandLine = TestableCommandLine.Parse(@switch, "value2", @switch, "value1");

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
					("-trait", cmd => cmd.Project.Filters.IncludedTraits),
					("-notrait", cmd => cmd.Project.Filters.ExcludedTraits),
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
			[MemberData(nameof(SwitchesLowerCase))]
			[MemberData(nameof(SwitchesUpperCase))]
			public static void SingleValidTraitArgument(
				string @switch,
				Expression<Func<CommandLine, Dictionary<string, List<string>>>> accessor)
			{
				var commandLine = TestableCommandLine.Parse(@switch, "foo=bar");

				var traits = accessor.Compile().Invoke(commandLine);
				Assert.Equal(1, traits.Count);
				Assert.Equal(1, traits["foo"].Count());
				Assert.Contains("bar", traits["foo"]);
			}

			[Theory]
			[MemberData(nameof(SwitchesLowerCase))]
			[MemberData(nameof(SwitchesUpperCase))]
			public static void MultipleValidTraitArguments_SameName(
				string @switch,
				Expression<Func<CommandLine, Dictionary<string, List<string>>>> accessor)
			{
				var commandLine = TestableCommandLine.Parse(@switch, "foo=bar", @switch, "foo=baz");

				var traits = accessor.Compile().Invoke(commandLine);
				Assert.Equal(1, traits.Count);
				Assert.Equal(2, traits["foo"].Count());
				Assert.Contains("bar", traits["foo"]);
				Assert.Contains("baz", traits["foo"]);
			}

			[Theory]
			[MemberData(nameof(SwitchesLowerCase))]
			[MemberData(nameof(SwitchesUpperCase))]
			public static void MultipleValidTraitArguments_DifferentName(
				string @switch,
				Expression<Func<CommandLine, Dictionary<string, List<string>>>> accessor)
			{
				var commandLine = TestableCommandLine.Parse(@switch, "foo=bar", @switch, "baz=biff");

				var traits = accessor.Compile().Invoke(commandLine);
				Assert.Equal(2, traits.Count);
				Assert.Equal(1, traits["foo"].Count());
				Assert.Contains("bar", traits["foo"]);
				Assert.Equal(1, traits["baz"].Count());
				Assert.Contains("biff", traits["baz"]);
			}

			[Theory]
			[MemberData(nameof(SwitchesLowerCase))]
			[MemberData(nameof(SwitchesUpperCase))]
			public static void MissingOptionValue(
				string @switch,
				Expression<Func<CommandLine, Dictionary<string, List<string>>>> _)
			{
				var ex = Record.Exception(() => TestableCommandLine.Parse(@switch));

				Assert.IsType<ArgumentException>(ex);
				Assert.Equal($"missing argument for {@switch.ToLowerInvariant()}", ex.Message);
			}

			[Theory]
			[MemberData(nameof(SwitchesWithOptionsLowerCase))]
			[MemberData(nameof(SwitchesWithOptionsUpperCase))]
			public static void ImproperlyFormattedOptionValue(
				string @switch,
				string optionValue)
			{
				var ex = Record.Exception(() => TestableCommandLine.Parse(@switch, optionValue));

				Assert.IsType<ArgumentException>(ex);
				Assert.Equal($"incorrect argument format for {@switch.ToLowerInvariant()} (should be \"name=value\")", ex.Message);
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
			var ex = Record.Exception(() => TestableCommandLine.Parse(@switch));

			Assert.IsType<ArgumentException>(ex);
			Assert.Equal($"missing filename for {@switch}", ex.Message);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void Output(string @switch)
		{
			var commandLine = TestableCommandLine.Parse(@switch, "outputFile");

			var output = Assert.Single(commandLine.Project.Output);
			Assert.Equal(@switch.Substring(1).ToLowerInvariant(), output.Key);
			Assert.Equal("outputFile", output.Value);
		}
	}

	public class Reporters
	{
		[Fact]
		public void NoReporters_UsesDefaultReporter()
		{
			var commandLine = TestableCommandLine.Parse();

			Assert.IsType<DefaultRunnerReporterWithTypes>(commandLine.Reporter);
		}

		[Fact]
		public void NoExplicitReporter_NoEnvironmentallyEnabledReporters_UsesDefaultReporter()
		{
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: false);

			var commandLine = TestableCommandLine.Parse(new[] { implicitReporter });

			Assert.IsType<DefaultRunnerReporterWithTypes>(commandLine.Reporter);
		}

		[Fact]
		public void ExplicitReporter_NoEnvironmentalOverride_UsesExplicitReporter()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");

			var commandLine = TestableCommandLine.Parse(new[] { explicitReporter }, "-switch");

			Assert.Same(explicitReporter, commandLine.Reporter);
		}

		[Fact]
		public void ExplicitReporter_WithEnvironmentalOverride_UsesEnvironmentalOverride()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);

			var commandLine = TestableCommandLine.Parse(new[] { explicitReporter, implicitReporter }, "-switch");

			Assert.Same(implicitReporter, commandLine.Reporter);
		}

		[Fact]
		public void WithEnvironmentalOverride_WithEnvironmentalOverridesDisabled_UsesDefaultReporter()
		{
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);

			var commandLine = TestableCommandLine.Parse(new[] { implicitReporter }, "-noautoreporters");

			Assert.IsType<DefaultRunnerReporterWithTypes>(commandLine.Reporter);
		}

		[Fact]
		public void NoExplicitReporter_SelectsFirstEnvironmentallyEnabledReporter()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");
			var implicitReporter1 = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
			var implicitReporter2 = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);

			var commandLine = TestableCommandLine.Parse(new[] { explicitReporter, implicitReporter1, implicitReporter2 });

			Assert.Same(implicitReporter1, commandLine.Reporter);
		}
	}

	class TestableCommandLine : CommandLine
	{
		public readonly IRunnerReporter Reporter;

		private TestableCommandLine(
			IReadOnlyList<IRunnerReporter> reporters,
			params string[] arguments)
				: base(arguments, filename => !filename.StartsWith("bad") && filename != "fileName")
		{
			Reporter = ChooseReporter(reporters);
		}

		protected override string GetFullPath(string fileName) =>
			$"/full/path/{fileName}";

		public static new TestableCommandLine Parse(params string[] arguments) =>
			new TestableCommandLine(new IRunnerReporter[0], arguments);

		public static TestableCommandLine Parse(
			IReadOnlyList<IRunnerReporter> reporters,
			params string[] arguments) =>
				new TestableCommandLine(reporters, arguments);
	}
}
