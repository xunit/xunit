using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.ConsoleClient;
using Xunit.Sdk;
using System.Reflection;

public class CommandLineTests
{
    public class Filename
    {
        [Fact]
        public void AssemblyFileNameNotPresentThrows()
        {
            string[] arguments = new string[1];
            arguments[0] = "fileName";

            var exception = Record.Exception(() =>
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

            var exception = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("config file not found: badConfig.config", exception.Message);
        }
    }

    public class InvalidOption
    {
        [Fact]
        public void OptionWithoutSlashThrows()
        {
            var arguments = new[] { "assembly.dll", "assembly.config", "teamcity" };

            var exception = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(exception);
            Assert.Equal("unknown command line option: teamcity", exception.Message);
        }
    }

    public class NoShadowOption
    {
        [Fact]
        public void NoShadowNotSetShadowCopyTrue()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            var assembly = Assert.Single(commandLine.Project.Assemblies);
            Assert.True(assembly.ShadowCopy);
        }

        [Fact]
        public void NoShadowSetShadowCopyFalse()
        {
            var arguments = new[] { "assemblyName.dll", "-noshadow" };

            var commandLine = TestableCommandLine.Parse(arguments);

            var assembly = Assert.Single(commandLine.Project.Assemblies);
            Assert.False(assembly.ShadowCopy);
        }
    }

    public class SilentOption
    {
        [Fact]
        public void SilentOptionNotPassedSilentFalse()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.Silent);
        }

        [Fact]
        public void SilentOptionSilentIsTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-silent" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.Silent);
        }

        [Fact]
        public void SilentOptionIgnoreCaseSilentIsTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-sIlEnT" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.Silent);
        }
    }

    public class WaitOption
    {
        [Fact]
        public void WaitOptionNotPassedWaitFalse()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.Wait);
        }

        [Fact]
        public void WaitOptionWaitIsTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-wait" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.Wait);
        }

        [Fact]
        public void WaitOptionIgnoreCaseWaitIsTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-wAiT" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.Wait);
        }
    }

    public class TeamCityArgument
    {
        [Fact, TeamCityEnvironmentRestore]
        public void TeamCityOptionNotPassedTeamCityFalse()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.False(commandLine.TeamCity);
        }

        [Fact, TeamCityEnvironmentRestore(Value = "TeamCity")]
        public void TeamCityOptionNotPassedEnvironmentSetTeamCityTrue()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.TeamCity);
        }

        [Fact, TeamCityEnvironmentRestore]
        public void TeamCityOptionTeamCityTrue()
        {
            var arguments = new[] { "assemblyName.dll", "-teamcity" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.True(commandLine.TeamCity);
        }

        [Fact, TeamCityEnvironmentRestore]
        public void TeamCityOptionIgnoreCaseTeamCityTrue()
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
        public void TraitArgumentNotPassed()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(0, commandLine.Project.Filters.IncludedTraits.Count);
        }

        [Fact]
        public void SingleValidTraitArgument()
        {
            var arguments = new[] { "assemblyName.dll", "-trait", "foo=bar" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(1, commandLine.Project.Filters.IncludedTraits.Count);
            Assert.Equal(1, commandLine.Project.Filters.IncludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.IncludedTraits["foo"]);
        }

        [Fact]
        public void MultipleValidTraitArguments_SameName()
        {
            var arguments = new[] { "assemblyName.dll", "-trait", "foo=bar", "-trait", "foo=baz" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(1, commandLine.Project.Filters.IncludedTraits.Count);
            Assert.Equal(2, commandLine.Project.Filters.IncludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.IncludedTraits["foo"]);
            Assert.Contains("baz", commandLine.Project.Filters.IncludedTraits["foo"]);
        }

        [Fact]
        public void MultipleValidTraitArguments_DifferentName()
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
        public void MissingOptionValue()
        {
            var arguments = new[] { "assemblyName.dll", "-trait" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("missing argument for -trait", ex.Message);
        }

        [Fact]
        public void OptionValueMissingEquals()
        {
            var arguments = new[] { "assemblyName.dll", "-trait", "foobar" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -trait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public void OptionValueMissingName()
        {
            var arguments = new[] { "assemblyName.dll", "-trait", "=bar" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -trait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public void OptionNameMissingValue()
        {
            var arguments = new[] { "assemblyName.dll", "-trait", "foo=" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -trait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public void TooManyEqualsSigns()
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
        public void TraitArgumentNotPassed()
        {
            var arguments = new[] { "assemblyName.dll" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(0, commandLine.Project.Filters.ExcludedTraits.Count);
        }

        [Fact]
        public void SingleValidTraitArgument()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait", "foo=bar" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(1, commandLine.Project.Filters.ExcludedTraits.Count);
            Assert.Equal(1, commandLine.Project.Filters.ExcludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.ExcludedTraits["foo"]);
        }

        [Fact]
        public void MultipleValidTraitArguments_SameName()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait", "foo=bar", "-notrait", "foo=baz" };

            var commandLine = TestableCommandLine.Parse(arguments);

            Assert.Equal(1, commandLine.Project.Filters.ExcludedTraits.Count);
            Assert.Equal(2, commandLine.Project.Filters.ExcludedTraits["foo"].Count());
            Assert.Contains("bar", commandLine.Project.Filters.ExcludedTraits["foo"]);
            Assert.Contains("baz", commandLine.Project.Filters.ExcludedTraits["foo"]);
        }

        [Fact]
        public void MultipleValidTraitArguments_DifferentName()
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
        public void MissingOptionValue()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("missing argument for -notrait", ex.Message);
        }

        [Fact]
        public void OptionValueMissingEquals()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait", "foobar" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -notrait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public void OptionValueMissingName()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait", "=bar" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -notrait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public void OptionNameMissingValue()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait", "foo=" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -notrait (should be \"name=value\")", ex.Message);
        }

        [Fact]
        public void TooManyEqualsSigns()
        {
            var arguments = new[] { "assemblyName.dll", "-notrait", "foo=bar=baz" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("incorrect argument format for -notrait (should be \"name=value\")", ex.Message);
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
            var arguments = new[] { "assemblyName.dll", "-xml" };

            var ex = Record.Exception(() => TestableCommandLine.Parse(arguments));

            Assert.IsType<ArgumentException>(ex);
            Assert.Equal("missing filename for -xml", ex.Message);
        }

        [Fact]
        public void Output()
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
            : base(arguments) { }

        public new static TestableCommandLine Parse(params string[] arguments)
        {
            return new TestableCommandLine(arguments);
        }

        protected override XunitProject Parse()
        {
            return Parse(filename => filename != "badConfig.config");
        }
    }
}
