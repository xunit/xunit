using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.SystemConsole;
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
		public static void AssemblyDoesNotExist_Throws()
		{
			var commandLine = TestableCommandLine.Parse("badAssembly.dll");

			Assert.IsType<ArgumentException>(commandLine.ParseFault);
			Assert.Equal("file not found: badAssembly.dll", commandLine.ParseFault.Message);
		}

		[Theory]
		[InlineData("badConfig.config")]
		[InlineData("badConfig.json")]
		public static void AssemblyExists_ConfigFileDoesNotExist_Throws(string configFile)
		{
			var commandLine = TestableCommandLine.Parse("assembly1.dll", configFile);

			Assert.IsType<ArgumentException>(commandLine.ParseFault);
			Assert.Equal("config file not found: " + configFile, commandLine.ParseFault.Message);
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

			var commandLine = TestableCommandLine.Parse(arguments);

			Assert.IsType<ArgumentException>(commandLine.ParseFault);
			Assert.Equal("expecting assembly, got config file: " + configFile2, commandLine.ParseFault.Message);
		}
	}

	public class Switches
	{
		static readonly (string Switch, Expression<Func<CommandLine, bool>> Accessor)[] SwitchOptionsList = new (string, Expression<Func<CommandLine, bool>>)[]
		{
			("-debug", cmd => cmd.Project.Configuration.DebugOrDefault),
			("-diagnostics", cmd => cmd.Project.Assemblies.All(a => a.Configuration.DiagnosticMessagesOrDefault)),
			("-failskips", cmd => cmd.Project.Assemblies.All(a => a.Configuration.FailSkipsOrDefault)),
			("-ignorefailures", cmd => cmd.Project.Configuration.IgnoreFailuresOrDefault),
			("-internaldiagnostics", cmd => cmd.Project.Assemblies.All(a => a.Configuration.InternalDiagnosticMessagesOrDefault)),
			("-noautoreporters", cmd => cmd.Project.Configuration.NoAutoReportersOrDefault),
			("-nocolor", cmd => cmd.Project.Configuration.NoColorOrDefault),
			("-nologo", cmd => cmd.Project.Configuration.NoLogoOrDefault),
			("-noshadow", cmd => !cmd.Project.Assemblies.Single().Configuration.ShadowCopyOrDefault),
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
			var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json");

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
			var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", @switch);

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
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json");

				foreach (var assembly in commandLine.Project.Assemblies)
					Assert.Null(assembly.Configuration.AppDomain);
			}

			[Fact]
			public static void MissingValue()
			{
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", "-appdomains");

				Assert.IsType<ArgumentException>(commandLine.ParseFault);
				Assert.Equal("missing argument for -appdomains", commandLine.ParseFault.Message);
			}

			[Fact]
			public static void InvalidValue()
			{
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", "-appdomains", "foo");

				Assert.IsType<ArgumentException>(commandLine.ParseFault);
				Assert.Equal("incorrect argument value for -appdomains (must be 'denied', 'required', or 'ifavailable')", commandLine.ParseFault.Message);
			}

			[Theory]
			[InlineData("required", AppDomainSupport.Required)]
			[InlineData("denied", AppDomainSupport.Denied)]
			[InlineData("ifavailable", AppDomainSupport.IfAvailable)]
			public static void ValidValues(
				string value,
				AppDomainSupport expected)
			{
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", "-appdomains", value);

				foreach (var assembly in commandLine.Project.Assemblies)
					Assert.Equal(expected, assembly.Configuration.AppDomain);
			}
		}

		public class MaxThreads
		{
			[Fact]
			public static void DefaultValueIsNull()
			{
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json");

				foreach (var assembly in commandLine.Project.Assemblies)
					Assert.Null(assembly.Configuration.MaxParallelThreads);
			}

			[Fact]
			public static void MissingValue()
			{
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", "-maxthreads");

				Assert.IsType<ArgumentException>(commandLine.ParseFault);
				Assert.Equal("missing argument for -maxthreads", commandLine.ParseFault.Message);
			}

			[Theory]
			[InlineData("abc")]
			[InlineData("0.ax")]  // Non-digit
			[InlineData(".0x")]   // Missing leading digit
			public static void InvalidValues(string value)
			{
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", "-maxthreads", value);

				Assert.IsType<ArgumentException>(commandLine.ParseFault);
				Assert.Equal("incorrect argument value for -maxthreads (must be 'default', 'unlimited', a positive number, or a multiplier in the form of '0.0x')", commandLine.ParseFault.Message);
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
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", "-maxthreads", value);

				foreach (var assembly in commandLine.Project.Assemblies)
					Assert.Equal(expected, assembly.Configuration.MaxParallelThreads);
			}

			[Theory]
			[InlineData("2x")]
			[InlineData("2.0x")]
			public static void MultiplierValue(string value)
			{
				var expected = Environment.ProcessorCount * 2;

				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", "-maxthreads", value);

				foreach (var assembly in commandLine.Project.Assemblies)
					Assert.Equal(expected, assembly.Configuration.MaxParallelThreads);
			}
		}

		public class Parallelization
		{
			[Fact]
			public static void ParallelizationOptionsAreNullByDefault()
			{
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json");

				foreach (var assembly in commandLine.Project.Assemblies)
				{
					Assert.Null(assembly.Configuration.ParallelizeAssembly);
					Assert.Null(assembly.Configuration.ParallelizeTestCollections);
				}
			}

			[Fact]
			public static void FailsWithoutOptionOrWithIncorrectOptions()
			{
				var commandLine1 = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", "-parallel");
				Assert.IsType<ArgumentException>(commandLine1.ParseFault);
				Assert.Equal("missing argument for -parallel", commandLine1.ParseFault.Message);

				var commandLine2 = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", "-parallel", "nonsense");
				Assert.IsType<ArgumentException>(commandLine2.ParseFault);
				Assert.Equal("incorrect argument value for -parallel", commandLine2.ParseFault.Message);
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
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", "-parallel", parallelOption);

				foreach (var assembly in commandLine.Project.Assemblies)
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
			var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json");

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
			var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", @switch);

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
			var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", @switch, "value1");

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
			var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", @switch, "value2", @switch, "value1");

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
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", @switch, "foo=bar");

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
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", @switch, "foo=bar", @switch, "foo=baz");

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
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", @switch, "foo=bar", @switch, "baz=biff");

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
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", @switch);

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
				var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", @switch, optionValue);

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
			var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", @switch);

			Assert.IsType<ArgumentException>(commandLine.ParseFault);
			Assert.Equal($"missing filename for {@switch}", commandLine.ParseFault.Message);
		}

		[Theory]
		[MemberData(nameof(SwitchesLowerCase))]
		[MemberData(nameof(SwitchesUpperCase))]
		public static void Output(string @switch)
		{
			var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json", @switch, "outputFile");

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
			var commandLine = TestableCommandLine.Parse("assemblyName.dll", "no-config.json");

			Assert.IsType<DefaultRunnerReporter>(commandLine.Reporter);
		}

		[Fact]
		public void NoExplicitReporter_NoEnvironmentallyEnabledReporters_UsesDefaultReporter()
		{
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: false);

			var commandLine = TestableCommandLine.Parse(new[] { implicitReporter }, "assemblyName.dll", "no-config.json");

			Assert.IsType<DefaultRunnerReporter>(commandLine.Reporter);
		}

		[Fact]
		public void ExplicitReporter_NoEnvironmentalOverride_UsesExplicitReporter()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");

			var commandLine = TestableCommandLine.Parse(new[] { explicitReporter }, "assemblyName.dll", "no-config.json", "-switch");

			Assert.Same(explicitReporter, commandLine.Reporter);
		}

		[Fact]
		public void ExplicitReporter_WithEnvironmentalOverride_UsesEnvironmentalOverride()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);

			var commandLine = TestableCommandLine.Parse(new[] { explicitReporter, implicitReporter }, "assemblyName.dll", "no-config.json", "-switch");

			Assert.Same(implicitReporter, commandLine.Reporter);
		}

		[Fact]
		public void WithEnvironmentalOverride_WithEnvironmentalOverridesDisabled_UsesDefaultReporter()
		{
			var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);

			var commandLine = TestableCommandLine.Parse(new[] { implicitReporter }, "assemblyName.dll", "no-config.json", "-noautoreporters");

			Assert.IsType<DefaultRunnerReporter>(commandLine.Reporter);
		}

		[Fact]
		public void NoExplicitReporter_SelectsFirstEnvironmentallyEnabledReporter()
		{
			var explicitReporter = Mocks.RunnerReporter("switch");
			var implicitReporter1 = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
			var implicitReporter2 = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);

			var commandLine = TestableCommandLine.Parse(new[] { explicitReporter, implicitReporter1, implicitReporter2 }, "assemblyName.dll", "no-config.json");

			Assert.Same(implicitReporter1, commandLine.Reporter);
		}
	}

	class TestableCommandLine : CommandLine
	{
		public readonly IRunnerReporter? Reporter;

		private TestableCommandLine(
			IReadOnlyList<IRunnerReporter> reporters,
			params string[] arguments)
				: base(arguments, filename => !filename.StartsWith("bad") && filename != "fileName")
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
