using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.ConsoleClient;
using Xunit.Sdk;

public class CommandLineTests
{
    public class Filename
    {
        [Fact]
        public static void MissingAssemblyFileNameThrows()
        {
            var exception = Record.Exception(() => CommandLine.Parse(new[] { "-teamcity" }));

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("must specify at least one assembly", exception.Message);
        }

        [Fact]
        public static void AssemblyFileNameNotPresentThrows()
        {
            var arguments = new string[1];
            arguments[0] = "fileName";

            var exception = Record.Exception(() =>
                            {
                                CommandLine.Parse(arguments);
                            });

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("file not found: fileName", exception.Message);
        }

        [Fact]
        public static void AssemblyFilePresentDoesNotThrow()
        {
            var arguments = new[] { "assemblyName.dll" };

            TestableCommandLine.Parse(arguments);  // Should not throw
        }

        [Fact]
        public static void DllExistsConfigFileDoesNotExist()
        {
            var arguments = new[] { "assemblyName.dll", "badConfig.config" };

            var exception = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("config file not found: badConfig.config", exception.Message);
        }

        [Fact]
        public static void MultipleAssembliesDoesNotThrow()
        {
            var arguments = new[] { "assemblyName.dll", "assemblyName2.dll" };

            var result = TestableCommandLine.Parse(arguments);

            Assert.Collection(result.Project,
                a =>
                {
                    Assert.Equal(Path.GetFullPath("assemblyName.dll"), a.AssemblyFilename);
                    Assert.Null(a.ConfigFilename);
                    Assert.True(a.ShadowCopy);
                },
                a =>
                {
                    Assert.Equal(Path.GetFullPath("assemblyName2.dll"), a.AssemblyFilename);
                    Assert.Null(a.ConfigFilename);
                    Assert.True(a.ShadowCopy);
                }
            );
        }

        [Fact]
        public static void MultipleAssembliesOneWithConfig()
        {
            var arguments = new[] { "assemblyName.dll", "assemblyName2.dll", "assembly2.config" };

            var result = TestableCommandLine.Parse(arguments);

            Assert.Collection(result.Project,
                a =>
                {
                    Assert.Equal(Path.GetFullPath("assemblyName.dll"), a.AssemblyFilename);
                    Assert.Null(a.ConfigFilename);
                    Assert.True(a.ShadowCopy);
                },
                a =>
                {
                    Assert.Equal(Path.GetFullPath("assemblyName2.dll"), a.AssemblyFilename);
                    Assert.Equal(Path.GetFullPath("assembly2.config"), a.ConfigFilename);
                    Assert.True(a.ShadowCopy);
                }
            );
        }

        [Fact]
        public static void ConfigFileWhenExpectingAssemblyThrows()
        {
            var arguments = new[] { "assemblyName.dll", "assembly1.config", "assembly2.config" };

            var exception = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("expecting assembly, got config file: assembly2.config", exception.Message);
        }
    }

    public class AppVeyorOption
    {
        [Fact, AppVeyorEnvironmentRestore]
        public static void AppVeyorOptionNotPassedAppVeyorFalse()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.AppVeyor);
        }

        [Fact, AppVeyorEnvironmentRestore(Value = "AppVeyor")]
        public static void AppVeyorOptionNotPassedEnvironmentSetAppVeyorTrue()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.AppVeyor);
        }

        [Fact, AppVeyorEnvironmentRestore]
        public static void AppVeyorOptionAppVeyorTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-appveyor" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.AppVeyor);
        }

        [Fact, AppVeyorEnvironmentRestore]
        public static void AppVeyorOptionIgnoreCaseAppVeyorTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-aPpVeyOr" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.AppVeyor);
        }

        class AppVeyorEnvironmentRestore : BeforeAfterTestAttribute
        {
            string originalValue;

            public string Value { get; set; }

            public override void Before(MethodInfo methodUnderTest)
            {
                originalValue = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");
                Environment.SetEnvironmentVariable("APPVEYOR_API_URL", Value);
            }

            public override void After(MethodInfo methodUnderTest)
            {
                Environment.SetEnvironmentVariable("APPVEYOR_API_URL", originalValue);
            }
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

        [Fact]
        public static void InvalidValue()
        {
            var ex = Assert.Throws<ArgumentException>(() => TestableCommandLine.Parse("assemblyName.dll", "-maxthreads", "abc"));

            Assert.Equal("incorrect argument value for -maxthreads", ex.Message);
        }

        [Fact]
        public static void SetsMaxParallelThreads()
        {
            var commandLine = TestableCommandLine.Parse("assemblyName.dll", "-maxthreads", "16");

            Assert.Equal(16, commandLine.MaxParallelThreads);
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

    public class QuietOption
    {
        [Fact]
        public static void QuietNotSetQuietIsFalse()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.Quiet);
        }

        [Fact]
        public static void QuietSetQuietIsTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-quiet" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.Quiet);
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

    public class TeamCityArgument
    {
        [Fact, TeamCityEnvironmentRestore]
        public static void TeamCityOptionNotPassedTeamCityFalse()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.TeamCity);
        }

        [Fact, TeamCityEnvironmentRestore(Value = "TeamCity")]
        public static void TeamCityOptionNotPassedEnvironmentSetTeamCityTrue()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.TeamCity);
        }

        [Fact, TeamCityEnvironmentRestore]
        public static void TeamCityOptionTeamCityTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-teamcity" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.TeamCity);
        }

        [Fact, TeamCityEnvironmentRestore]
        public static void TeamCityOptionIgnoreCaseTeamCityTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-tEaMcItY" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.TeamCity);
        }

        class TeamCityEnvironmentRestore : BeforeAfterTestAttribute
        {
            string originalValue;

            public string Value { get; set; }

            public override void Before(MethodInfo methodUnderTest)
            {
                originalValue = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME");
                Environment.SetEnvironmentVariable("TEAMCITY_PROJECT_NAME", Value);
            }

            public override void After(MethodInfo methodUnderTest)
            {
                Environment.SetEnvironmentVariable("TEAMCITY_PROJECT_NAME", originalValue);
            }
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
            var project = TestableCommandLine.Parse("assemblyName.dll");

            Assert.Null(project.ParallelizeAssemblies);
            Assert.Null(project.ParallelizeTestCollections);
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
            var project = TestableCommandLine.Parse("assemblyName.dll", "-parallel", parallelOption);

            Assert.Equal(expectedAssemblyParallelization, project.ParallelizeAssemblies);
            Assert.Equal(expectedCollectionsParallelization, project.ParallelizeTestCollections);
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
        private TestableCommandLine(params string[] arguments)
            : base(arguments, filename => filename != "badConfig.config") { }

        public new static TestableCommandLine Parse(params string[] arguments)
        {
            return new TestableCommandLine(arguments);
        }
    }
}
