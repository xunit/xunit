using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.ConsoleClient;

public class CommandLineTests
{
    public class UnknownSwitch
    {
        [Fact]
        public static void UnknownSwitchThrows()
        {
            var exception = Record.Exception(() => TestableCommandLine.Parse(new[] { "assemblyName.dll", "-unknown" }));

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("unknown option: -unknown", exception.Message);
        }
    }

    public class Filename
    {
        [Fact]
        public static void AssemblyFileNameNotPresentThrows()
        {
            var arguments = new string[1];
            arguments[0] = "fileName";

            var exception = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("file not found: fileName", exception.Message);
        }

        [Fact]
        public static void AssemblyFilePresentDoesNotThrow()
        {
            var arguments = new[] { "assemblyName.dll" };

            TestableCommandLine.Parse(arguments);  // Should not throw
        }

        [Theory]
        [InlineData("badConfig.config")]
        [InlineData("badConfig.json")]
        public static void DllExistsConfigFileDoesNotExist(string configFile)
        {
            var arguments = new[] { "assemblyName.dll", configFile };

            var exception = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("config file not found: " + configFile, exception.Message);
        }

#pragma warning disable CS0618
        [Fact]
        public static void MultipleAssembliesDoesNotThrow()
        {
            var arguments = new[] { "assemblyName.dll", "assemblyName2.dll" };

            var result = TestableCommandLine.Parse(arguments);

            Assert.Collection(result.Project,
                a =>
                {
                    Assert.Equal("/full/path/assemblyName.dll", a.AssemblyFilename);
                    Assert.Null(a.ConfigFilename);
                    Assert.True(a.ShadowCopy);
                },
                a =>
                {
                    Assert.Equal("/full/path/assemblyName2.dll", a.AssemblyFilename);
                    Assert.Null(a.ConfigFilename);
                    Assert.True(a.ShadowCopy);
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

            Assert.Collection(result.Project,
                a =>
                {
                    Assert.Equal("/full/path/assemblyName.dll", a.AssemblyFilename);
                    Assert.Null(a.ConfigFilename);
                    Assert.True(a.ShadowCopy);
                },
                a =>
                {
                    Assert.Equal("/full/path/assemblyName2.dll", a.AssemblyFilename);
                    Assert.Equal($"/full/path/{configFile}", a.ConfigFilename);
                    Assert.True(a.ShadowCopy);
                }
            );
        }
#pragma warning restore CS0618

        [Theory]
        [InlineData("assembly1.config", "assembly2.config")]
        [InlineData("assembly1.config", "assembly2.json")]
        [InlineData("assembly1.json", "assembly2.config")]
        [InlineData("assembly1.json", "assembly2.json")]
        public static void ConfigFileWhenExpectingAssemblyThrows(string configFile1, string configFile2)
        {
            var arguments = new[] { "assemblyName.dll", configFile1, configFile2 };

            var exception = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("expecting assembly, got config file: " + configFile2, exception.Message);
        }
    }

    public class DebugOption
    {
        [Fact]
        public static void DebugNotSetDebugIsFalse()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.Debug);
        }

        [Fact]
        public static void DebugSetDebugIsTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-debug" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.Debug);
        }
    }

    public class DiagnosticsOption
    {
        [Fact]
        public static void DiagnosticsNotSetDebugIsFalse()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.DiagnosticMessages);
        }

        [Fact]
        public static void DiagnosticsSetDebugIsTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-diagnostics" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.DiagnosticMessages);
        }
    }

    public class SerializeOption
    {
        [Fact]
        public static void SerializeNotSetSerializeIsFalse()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.Serialize);
        }

        [Fact]
        public static void SerializeSetSerializeIsTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-serialize" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.Serialize);
        }
    }

    public class MaxThreadsOption
    {
        [Fact]
        public static void DefaultValueIsNull()
        {
            var commandLine = TestableCommandLine.Parse("assemblyName.dll");

            Assert.Null(commandLine.MaxParallelThreads);
        }

        [Fact]
        public static void MissingValue()
        {
            var ex = Assert.Throws<ArgumentException>(() => TestableCommandLine.Parse("assemblyName.dll", "-maxthreads"));

            Assert.Equal("missing argument for -maxthreads", ex.Message);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("abc")]
        public static void InvalidValues(string value)
        {
            var ex = Assert.Throws<ArgumentException>(() => TestableCommandLine.Parse("assemblyName.dll", "-maxthreads", value));

            Assert.Equal("incorrect argument value for -maxthreads (must be 'default', 'unlimited', or a positive number)", ex.Message);
        }

        [Theory]
        [InlineData("default", 0)]
        [InlineData("unlimited", -1)]
        [InlineData("16", 16)]
        public static void ValidValues(string value, int expected)
        {
            var commandLine = TestableCommandLine.Parse("assemblyName.dll", "-maxthreads", value);

            Assert.Equal(expected, commandLine.MaxParallelThreads);
        }
    }

    public class AppDomainsOption
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
        public static void ValidValues(string value, AppDomainSupport expected)
        {
            var commandLine = TestableCommandLine.Parse("assemblyName.dll", "-appdomains", value);

            Assert.Equal(expected, commandLine.AppDomains);
        }
    }

    public class NoLogoOption
    {
        [Fact]
        public static void NoLogoNotSetNoLogoIsFalse()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.NoLogo);
        }

        [Fact]
        public static void NoLogoSetNoLogoIsTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-nologo" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.NoLogo);
        }
    }

#pragma warning disable CS0618
    public class NoShadowOption
    {
        [Fact]
        public static void NoShadowNotSetShadowCopyTrue()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            var assembly = Assert.Single(commandLine.Project.Assemblies);
            Assert.True(assembly.ShadowCopy);
        }

        [Fact]
        public static void NoShadowSetShadowCopyFalse()
        {
            var arguments = new[] { "assemblyName.dll", "-noshadow" };

            var commandLine = TestableCommandLine.Parse(arguments);

            var assembly = Assert.Single(commandLine.Project.Assemblies);
            Assert.False(assembly.ShadowCopy);
        }
    }
#pragma warning restore CS0618

    public class FailSkipsOption
    {
        [Fact]
        public static void FailSkipsOptionNotSetFailSkipsFalse()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.FailSkips);
        }

        [Theory]
        [InlineData("-failskips")]
        [InlineData("-fAiLsKIpS")]
        public static void FailSkipsOptionSetFailSkipsTrue(string option)
        {
            var arguments = new[] { "assemblyName.dll", option };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.FailSkips);
        }
    }

    public class StopOnFailOption
    {
        [Fact]
        public static void StopOnFailOptionNotSetStopOnFailFalse()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.StopOnFail);
        }

        [Theory]
        [InlineData("-stoponfail")]
        [InlineData("-sToPoNfAiL")]
        public static void StopOnFailOptionSetOnFailTrue(string option)
        {
            var arguments = new[] { "assemblyName.dll", option };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.StopOnFail);
        }
    }

    public class WaitOption
    {
        [Fact]
        public static void WaitOptionNotPassedWaitFalse()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.Wait);
        }

        [Fact]
        public static void WaitOptionWaitIsTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-wait" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.Wait);
        }

        [Fact]
        public static void WaitOptionIgnoreCaseWaitIsTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-wAiT" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.Wait);
        }
    }

    public class TraitArgument
    {
        [Fact]
        public static void TraitArgumentNotPassed()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(0, commandLine.Project.Filters.IncludedTraits.Count);
        }

        [Fact]
        public static void SingleValidTraitArgument()
        {
            var arguments = new[] { "assemblyName.dll", "-trait", "foo=bar" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(1, commandLine.Project.Filters.IncludedTraits.Count);
            Assert.Equal(1, commandLine.Project.Filters.IncludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.IncludedTraits["foo"]);
        }

        [Fact]
        public static void MultipleValidTraitArguments_SameName()
        {
            var arguments = new[] { "assemblyName.dll", "-trait", "foo=bar", "-trait", "foo=baz" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(1, commandLine.Project.Filters.IncludedTraits.Count);
            Assert.Equal(2, commandLine.Project.Filters.IncludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.IncludedTraits["foo"]);
            Assert.Contains("baz", commandLine.Project.Filters.IncludedTraits["foo"]);
        }

        [Fact]
        public static void MultipleValidTraitArguments_DifferentName()
        {
            var arguments = new[] { "assemblyName.dll", "-trait", "foo=bar", "-trait", "baz=biff" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(2, commandLine.Project.Filters.IncludedTraits.Count);
            Assert.Equal(1, commandLine.Project.Filters.IncludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.IncludedTraits["foo"]);
            Assert.Equal(1, commandLine.Project.Filters.IncludedTraits["baz"].Count());
            Assert.Contains("biff", commandLine.Project.Filters.IncludedTraits["baz"]);
        }

        [Fact]
        public static void MissingOptionValue()
        {
            var arguments = new[] { "assemblyName.dll", "-trait" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("missing argument for -trait", ex.Message);
        }

        [Fact]
        public static void OptionValueMissingEquals()
        {
            var arguments = new[] { "assemblyName.dll", "-trait", "foobar" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -trait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public static void OptionValueMissingName()
        {
            var arguments = new[] { "assemblyName.dll", "-trait", "=bar" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -trait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public static void OptionNameMissingValue()
        {
            var arguments = new[] { "assemblyName.dll", "-trait", "foo=" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -trait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public static void TooManyEqualsSigns()
        {
            var arguments = new[] { "assemblyName.dll", "-trait", "foo=bar=baz" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -trait (should be \"name=value\")", ex.Message);
        }
    }

    public class MinusTraitArgument
    {
        [Fact]
        public static void TraitArgumentNotPassed()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(0, commandLine.Project.Filters.ExcludedTraits.Count);
        }

        [Fact]
        public static void SingleValidTraitArgument()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait", "foo=bar" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(1, commandLine.Project.Filters.ExcludedTraits.Count);
            Assert.Equal(1, commandLine.Project.Filters.ExcludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.ExcludedTraits["foo"]);
        }

        [Fact]
        public static void MultipleValidTraitArguments_SameName()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait", "foo=bar", "-notrait", "foo=baz" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(1, commandLine.Project.Filters.ExcludedTraits.Count);
            Assert.Equal(2, commandLine.Project.Filters.ExcludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.ExcludedTraits["foo"]);
            Assert.Contains("baz", commandLine.Project.Filters.ExcludedTraits["foo"]);
        }

        [Fact]
        public static void MultipleValidTraitArguments_DifferentName()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait", "foo=bar", "-notrait", "baz=biff" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(2, commandLine.Project.Filters.ExcludedTraits.Count);
            Assert.Equal(1, commandLine.Project.Filters.ExcludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.ExcludedTraits["foo"]);
            Assert.Equal(1, commandLine.Project.Filters.ExcludedTraits["baz"].Count());
            Assert.Contains("biff", commandLine.Project.Filters.ExcludedTraits["baz"]);
        }

        [Fact]
        public static void MissingOptionValue()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("missing argument for -notrait", ex.Message);
        }

        [Fact]
        public static void OptionValueMissingEquals()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait", "foobar" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -notrait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public static void OptionValueMissingName()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait", "=bar" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -notrait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public static void OptionNameMissingValue()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait", "foo=" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -notrait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public static void TooManyEqualsSigns()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait", "foo=bar=baz" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -notrait (should be \"name=value\")", ex.Message);
        }
    }

    public class MethodArgument
    {
        [Fact]
        public static void MethodArgumentNotPassed()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(0, commandLine.Project.Filters.IncludedMethods.Count);
        }

        [Fact]
        public static void SingleValidMethodArgument()
        {
            const string name = "Namespace.Class.Method1";

            var arguments = new[] { "assemblyName.dll", "-method", name };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(1, commandLine.Project.Filters.IncludedMethods.Count);
            Assert.True(commandLine.Project.Filters.IncludedMethods.Contains(name));
        }

        [Fact]
        public static void MultipleValidMethodArguments()
        {
            const string name1 = "Namespace.Class.Method1";
            const string name2 = "Namespace.Class.Method2";

            var arguments = new[] { "assemblyName.dll", "-method", name1, "-method", name2 };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(2, commandLine.Project.Filters.IncludedMethods.Count);
            Assert.True(commandLine.Project.Filters.IncludedMethods.Contains(name1));
            Assert.True(commandLine.Project.Filters.IncludedMethods.Contains(name2));
        }

        [Fact]
        public static void MissingOptionValue()
        {
            var arguments = new[] { "assemblyName.dll", "-method" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("missing argument for -method", ex.Message);
        }
    }

    public class ClassArgument
    {
        [Fact]
        public static void ClassArgumentNotPassed()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(0, commandLine.Project.Filters.IncludedMethods.Count);
        }

        [Fact]
        public static void SingleValidClassArgument()
        {
            const string name = "Namespace.Class";

            var arguments = new[] { "assemblyName.dll", "-class", name };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(1, commandLine.Project.Filters.IncludedClasses.Count);
            Assert.True(commandLine.Project.Filters.IncludedClasses.Contains(name));
        }

        [Fact]
        public static void MultipleValidClassArguments()
        {
            const string name1 = "Namespace.Class1";
            const string name2 = "Namespace.Class2";

            var arguments = new[] { "assemblyName.dll", "-class", name1, "-class", name2 };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(2, commandLine.Project.Filters.IncludedClasses.Count);
            Assert.True(commandLine.Project.Filters.IncludedClasses.Contains(name1));
            Assert.True(commandLine.Project.Filters.IncludedClasses.Contains(name2));
        }

        [Fact]
        public static void MissingOptionValue()
        {
            var arguments = new[] { "assemblyName.dll", "-class" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("missing argument for -class", ex.Message);
        }
    }

    public class ParallelizationOptions
    {
        [Fact]
        public static void ParallelizationOptionsAreNullByDefault()
        {
            var commandLine = TestableCommandLine.Parse("assemblyName.dll");

            Assert.Null(commandLine.ParallelizeAssemblies);
            Assert.Null(commandLine.ParallelizeTestCollections);
        }

        [Fact]
        public static void FailsWithoutOptionOrWithIncorrectOptions()
        {
            var aex1 = Assert.Throws<ArgumentException>(() => TestableCommandLine.Parse("assemblyName.dll", "-parallel"));
            Assert.Equal("missing argument for -parallel", aex1.Message);

            var aex2 = Assert.Throws<ArgumentException>(() => TestableCommandLine.Parse("assemblyName.dll", "-parallel", "nonsense"));
            Assert.Equal("incorrect argument value for -parallel", aex2.Message);
        }

        [Theory]
        [InlineData("none", false, false)]
        [InlineData("assemblies", true, false)]
        [InlineData("collections", false, true)]
        [InlineData("all", true, true)]
        public static void ParallelCanBeTurnedOn(string parallelOption, bool expectedAssemblyParallelization, bool expectedCollectionsParallelization)
        {
            var commandLine = TestableCommandLine.Parse("assemblyName.dll", "-parallel", parallelOption);

            Assert.Equal(expectedAssemblyParallelization, commandLine.ParallelizeAssemblies);
            Assert.Equal(expectedCollectionsParallelization, commandLine.ParallelizeTestCollections);
        }
    }

    public class Reporters
    {
        [Fact]
        public void NoReporters_UsesDefaultReporter()
        {
            var commandLine = TestableCommandLine.Parse("assemblyName.dll");

            Assert.IsType<DefaultRunnerReporterWithTypes>(commandLine.Reporter);
        }

        [Fact]
        public void NoExplicitReporter_NoEnvironmentallyEnabledReporters_UsesDefaultReporter()
        {
            var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: false);

            var commandLine = TestableCommandLine.Parse(new[] { implicitReporter }, "assemblyName.dll");

            Assert.IsType<DefaultRunnerReporterWithTypes>(commandLine.Reporter);
        }

        [Fact]
        public void ExplicitReporter_NoEnvironmentalOverride_UsesExplicitReporter()
        {
            var explicitReporter = Mocks.RunnerReporter("switch");

            var commandLine = TestableCommandLine.Parse(new[] { explicitReporter }, "assemblyName.dll", "-switch");

            Assert.Same(explicitReporter, commandLine.Reporter);
        }

        [Fact]
        public void ExplicitReporter_WithEnvironmentalOverride_UsesEnvironmentalOverride()
        {
            var explicitReporter = Mocks.RunnerReporter("switch");
            var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);

            var commandLine = TestableCommandLine.Parse(new[] { explicitReporter, implicitReporter }, "assemblyName.dll", "-switch");

            Assert.Same(implicitReporter, commandLine.Reporter);
        }

        [Fact]
        public void WithEnvironmentalOverride_WithEnvironmentalOverridesDisabled_UsesDefaultReporter()
        {
            var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);

            var commandLine = TestableCommandLine.Parse(new[] { implicitReporter }, "assemblyName.dll", "-noautoreporters");

            Assert.IsType<DefaultRunnerReporterWithTypes>(commandLine.Reporter);
        }

        [Fact]
        public void NoExplicitReporter_SelectsFirstEnvironmentallyEnabledReporter()
        {
            var explicitReporter = Mocks.RunnerReporter("switch");
            var implicitReporter1 = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
            var implicitReporter2 = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);

            var commandLine = TestableCommandLine.Parse(new[] { explicitReporter, implicitReporter1, implicitReporter2 }, "assemblyName.dll");

            Assert.Same(implicitReporter1, commandLine.Reporter);
        }
    }

    public class Transform
    {
        [Fact]
        public static void OutputMissingFilename()
        {
            var arguments = new[] { "assemblyName.dll", "-xml" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("missing filename for -xml", ex.Message);
        }

        [Fact]
        public static void Output()
        {
            var arguments = new[] { "assemblyName.dll", "-xml", "foo.xml" };

            var commandLine = TestableCommandLine.Parse(arguments);

            var output = Assert.Single(commandLine.Project.Output);
            Assert.Equal("xml", output.Key);
            Assert.Equal("foo.xml", output.Value);
        }
    }

    class TestableCommandLine : CommandLine
    {
        public readonly IRunnerReporter Reporter;

        private TestableCommandLine(IReadOnlyList<IRunnerReporter> reporters, params string[] arguments)
            : base(arguments, filename => !filename.StartsWith("badConfig.") && filename != "fileName")
        {
            Reporter = ChooseReporter(reporters);
        }

        protected override string GetFullPath(string fileName)
            => $"/full/path/{fileName}";

        public static new TestableCommandLine Parse(params string[] arguments)
            => new TestableCommandLine(new IRunnerReporter[0], arguments);

        public static TestableCommandLine Parse(IReadOnlyList<IRunnerReporter> reporters, params string[] arguments)
            => new TestableCommandLine(reporters, arguments);
    }
}
