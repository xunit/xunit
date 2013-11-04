using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.ConsoleClient;
using Xunit.Sdk;

public class CommandLineTests
{
    public class Filename
    {
        [Fact]
        public void AssemblyFileNameNotPresentThrows()
        {
            string[] arguments = new string[1];
            arguments[0] = "fileName";

            Exception exception = Record.Exception(() =>
                {
                    CommandLine.Parse(arguments);
                });

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("file not found: fileName", exception.Message);
        }

        [Fact]
        public void AssemblyFilePresentDoesNotThrow()
        {
            string[] arguments = new[] { "assemblyName.dll" };

            Assert.DoesNotThrow(() =>
            {
                TestableCommandLine.Parse(arguments);
            });
        }

        [Fact]
        public void DllExistsConfigFileDoesNotExist()
        {
            string[] arguments = new[] { "assemblyName.dll", "badConfig.config" };

            Exception exception = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("config file not found: badConfig.config", exception.Message);
        }
    }

    public class InvalidOption
    {
        [Fact]
        public void OptionWithoutSlashThrows()
        {
            string[] arguments = new[] { "assembly.dll", "assembly.config", "teamcity" };

            Exception exception = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("unknown command line option: teamcity", exception.Message);
        }

        [Fact]
        public void SecondArgumentOptionWithoutSlashThrows()
        {
            string[] arguments = new[] { "assembly.xunit2", "teamcity" };

            Exception exception = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("unknown command line option: teamcity", exception.Message);
        }
    }

    public class NoShadowOption
    {
        [Fact]
        public void NoShadowNotSetShadowCopyTrue()
        {
            string[] arguments = new[] { "assemblyName.dll" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            XunitProjectAssembly assembly = Assert.Single(commandLine.Project.Assemblies);
            Assert.True(assembly.ShadowCopy);
        }

        [Fact]
        public void NoShadowSetShadowCopyFalse()
        {
            string[] arguments = new[] { "assemblyName.dll", "-noshadow" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            XunitProjectAssembly assembly = Assert.Single(commandLine.Project.Assemblies);
            Assert.False(assembly.ShadowCopy);
        }
    }

    public class SilentOption
    {
        [Fact]
        public void SilentOptionNotPassedSilentFalse()
        {
            string[] arguments = new[] { "assemblyName.dll" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.Silent);
        }

        [Fact]
        public void SilentOptionSilentIsTrue()
        {
            string[] arguments = new[] { "assemblyName.dll", "-silent" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.Silent);
        }

        [Fact]
        public void SilentOptionIgnoreCaseSilentIsTrue()
        {
            string[] arguments = new[] { "assemblyName.dll", "-sIlEnT" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.Silent);
        }
    }

    public class WaitOption
    {
        [Fact]
        public void WaitOptionNotPassedWaitFalse()
        {
            string[] arguments = new[] { "assemblyName.dll" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.Wait);
        }

        [Fact]
        public void WaitOptionWaitIsTrue()
        {
            string[] arguments = new[] { "assemblyName.dll", "-wait" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.Wait);
        }

        [Fact]
        public void WaitOptionIgnoreCaseWaitIsTrue()
        {
            string[] arguments = new[] { "assemblyName.dll", "-wAiT" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.Wait);
        }
    }

    public class TeamCityArgument
    {
        [Fact, TeamCityEnvironmentRestore]
        public void TeamCityOptionNotPassedTeamCityFalse()
        {
            string[] arguments = new[] { "assemblyName.dll" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.TeamCity);
        }

        [Fact, TeamCityEnvironmentRestore(Value = "TeamCity")]
        public void TeamCityOptionNotPassedEnvironmentSetTeamCityTrue()
        {
            string[] arguments = new[] { "assemblyName.dll" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.TeamCity);
        }

        [Fact, TeamCityEnvironmentRestore]
        public void TeamCityOptionTeamCityTrue()
        {
            string[] arguments = new[] { "assemblyName.dll", "-teamcity" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.TeamCity);
        }

        [Fact, TeamCityEnvironmentRestore]
        public void TeamCityOptionIgnoreCaseTeamCityTrue()
        {
            string[] arguments = new[] { "assemblyName.dll", "-tEaMcItY" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.TeamCity);
        }

        class TeamCityEnvironmentRestore : BeforeAfterTestAttribute
        {
            string originalValue;

            public string Value { get; set; }

            public override void Before(System.Reflection.MethodInfo methodUnderTest)
            {
                originalValue = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME");
                Environment.SetEnvironmentVariable("TEAMCITY_PROJECT_NAME", Value);
            }

            public override void After(System.Reflection.MethodInfo methodUnderTest)
            {
                Environment.SetEnvironmentVariable("TEAMCITY_PROJECT_NAME", originalValue);
            }
        }
    }

    public class TraitArgument
    {
        [Fact]
        public void TraitArgumentNotPassed()
        {
            string[] arguments = new[] { "assemblyName.dll" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(0, commandLine.Project.Filters.IncludedTraits.Count);
        }

        [Fact]
        public void SingleValidTraitArgument()
        {
            string[] arguments = new[] { "assemblyName.dll", "-trait", "foo=bar" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(1, commandLine.Project.Filters.IncludedTraits.Count);
            Assert.Equal(1, commandLine.Project.Filters.IncludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.IncludedTraits["foo"]);
        }

        [Fact]
        public void MultipleValidTraitArguments_SameName()
        {
            string[] arguments = new[] { "assemblyName.dll", "-trait", "foo=bar", "-trait", "foo=baz" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(1, commandLine.Project.Filters.IncludedTraits.Count);
            Assert.Equal(2, commandLine.Project.Filters.IncludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.IncludedTraits["foo"]);
            Assert.Contains("baz", commandLine.Project.Filters.IncludedTraits["foo"]);
        }

        [Fact]
        public void MultipleValidTraitArguments_DifferentName()
        {
            string[] arguments = new[] { "assemblyName.dll", "-trait", "foo=bar", "-trait", "baz=biff" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(2, commandLine.Project.Filters.IncludedTraits.Count);
            Assert.Equal(1, commandLine.Project.Filters.IncludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.IncludedTraits["foo"]);
            Assert.Equal(1, commandLine.Project.Filters.IncludedTraits["baz"].Count());
            Assert.Contains("biff", commandLine.Project.Filters.IncludedTraits["baz"]);
        }

        [Fact]
        public void MissingOptionValue()
        {
            string[] arguments = new[] { "assemblyName.dll", "-trait" };

            Exception ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("missing argument for -trait", ex.Message);
        }

        [Fact]
        public void OptionValueMissingEquals()
        {
            string[] arguments = new[] { "assemblyName.dll", "-trait", "foobar" };

            Exception ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -trait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public void OptionValueMissingName()
        {
            string[] arguments = new[] { "assemblyName.dll", "-trait", "=bar" };

            Exception ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -trait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public void OptionNameMissingValue()
        {
            string[] arguments = new[] { "assemblyName.dll", "-trait", "foo=" };

            Exception ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -trait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public void TooManyEqualsSigns()
        {
            string[] arguments = new[] { "assemblyName.dll", "-trait", "foo=bar=baz" };

            Exception ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -trait (should be \"name=value\")", ex.Message);
        }
    }

    public class MinusTraitArgument
    {
        [Fact]
        public void TraitArgumentNotPassed()
        {
            string[] arguments = new[] { "assemblyName.dll" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(0, commandLine.Project.Filters.ExcludedTraits.Count);
        }

        [Fact]
        public void SingleValidTraitArgument()
        {
            string[] arguments = new[] { "assemblyName.dll", "-notrait", "foo=bar" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(1, commandLine.Project.Filters.ExcludedTraits.Count);
            Assert.Equal(1, commandLine.Project.Filters.ExcludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.ExcludedTraits["foo"]);
        }

        [Fact]
        public void MultipleValidTraitArguments_SameName()
        {
            string[] arguments = new[] { "assemblyName.dll", "-notrait", "foo=bar", "-notrait", "foo=baz" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(1, commandLine.Project.Filters.ExcludedTraits.Count);
            Assert.Equal(2, commandLine.Project.Filters.ExcludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.ExcludedTraits["foo"]);
            Assert.Contains("baz", commandLine.Project.Filters.ExcludedTraits["foo"]);
        }

        [Fact]
        public void MultipleValidTraitArguments_DifferentName()
        {
            string[] arguments = new[] { "assemblyName.dll", "-notrait", "foo=bar", "-notrait", "baz=biff" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(2, commandLine.Project.Filters.ExcludedTraits.Count);
            Assert.Equal(1, commandLine.Project.Filters.ExcludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.ExcludedTraits["foo"]);
            Assert.Equal(1, commandLine.Project.Filters.ExcludedTraits["baz"].Count());
            Assert.Contains("biff", commandLine.Project.Filters.ExcludedTraits["baz"]);
        }

        [Fact]
        public void MissingOptionValue()
        {
            string[] arguments = new[] { "assemblyName.dll", "-notrait" };

            Exception ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("missing argument for -notrait", ex.Message);
        }

        [Fact]
        public void OptionValueMissingEquals()
        {
            string[] arguments = new[] { "assemblyName.dll", "-notrait", "foobar" };

            Exception ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -notrait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public void OptionValueMissingName()
        {
            string[] arguments = new[] { "assemblyName.dll", "-notrait", "=bar" };

            Exception ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -notrait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public void OptionNameMissingValue()
        {
            string[] arguments = new[] { "assemblyName.dll", "-notrait", "foo=" };

            Exception ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -notrait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public void TooManyEqualsSigns()
        {
            string[] arguments = new[] { "assemblyName.dll", "-notrait", "foo=bar=baz" };

            Exception ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -notrait (should be \"name=value\")", ex.Message);
        }
    }

    public class IsProjectFilename
    {
        [Fact]
        public void IsProjectFileNameTrue()
        {
            string fileName = "xUnit.xunit2";

            bool isProjectFilename = CommandLine.IsProjectFilename(fileName);

            Assert.True(isProjectFilename);
        }

        [Fact]
        public void IsProjectFileNameTrueIgoresCase()
        {
            string fileName = "xUnit.xuNiT2";

            bool isProjectFilename = CommandLine.IsProjectFilename(fileName);

            Assert.True(isProjectFilename);
        }

        [Fact]
        public void IsProjectFileNameFalse()
        {
            string fileName = "xUnit.sln";

            bool isProjectFilename = CommandLine.IsProjectFilename(fileName);

            Assert.False(isProjectFilename);
        }
    }

    public class Parallel
    {
        [Fact]
        public void ParallelIsOffByDefault()
        {
            var project = TestableCommandLine.Parse("assemblyName.dll");

            Assert.False(project.Parallel);
        }

        [Fact]
        public void ParallelCanBeTurnedOn()
        {
            var project = TestableCommandLine.Parse("assemblyName.dll", "-parallel");

            Assert.True(project.Parallel);
        }
    }

    public class Transform
    {
        [Fact]
        public void OutputMissingFilename()
        {
            string[] arguments = new[] { "assemblyName.dll", "-xml" };

            Exception ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("missing filename for -xml", ex.Message);
        }

        [Fact]
        public void OutputOnProjectFile()
        {
            string[] arguments = new[] { "assemblyName.xunit2", "-xml", "foo.xml" };

            Exception ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("the -xml command line option isn't valid for .xunit2 projects", ex.Message);
        }

        [Fact]
        public void OutputOnNonProjectFile()
        {
            string[] arguments = new[] { "assemblyName.dll", "-xml", "foo.xml" };

            TestableCommandLine commandLine = TestableCommandLine.Parse(arguments);

            KeyValuePair<string, string> output = Assert.Single(commandLine.Project.Output);
            Assert.Equal("xml", output.Key);
            Assert.Equal("foo.xml", output.Value);
        }
    }

    class TestableCommandLine : CommandLine
    {
        private TestableCommandLine(params string[] arguments)
            : base(arguments) { }

        public new static TestableCommandLine Parse(params string[] arguments)
        {
            return new TestableCommandLine(arguments);
        }

        protected override XunitProject GetMultiAssemblyProject(string filename)
        {
            return new XunitProject();
        }

        protected override XunitProject Parse()
        {
            return Parse(filename => filename != "badConfig.config");
        }
    }
}
