using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using NSubstitute;
using Xunit;
using Xunit.Runner.MSBuild;

public class xunitTests
{
    public class Execute
    {
        [Fact, PreserveWorkingDirectory]
        public static void ChangesCurrentDirectoryWhenWorkingFolderIsNotNull()
        {
            var tempFolder = Environment.GetEnvironmentVariable("TEMP");
            tempFolder = Path.GetFullPath(tempFolder); // Ensure that the 8.3 path is not used
            var xunit = new Testable_xunit { WorkingFolder = tempFolder };

            xunit.Execute();

            string actual = Directory.GetCurrentDirectory();
            string expected = tempFolder;

            if (actual[actual.Length - 1] != Path.DirectorySeparatorChar)
            {
                actual += Path.DirectorySeparatorChar;
            }
            if (expected[expected.Length - 1] != Path.DirectorySeparatorChar)
            {
                expected += Path.DirectorySeparatorChar;
            }

            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void LogsWelcomeBanner()
        {
            var xunit = new Testable_xunit();

            xunit.Execute();

            var versionAttribute = typeof(xunit).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var eventArgs = Assert.IsType<BuildMessageEventArgs>(xunit.BuildEngine.Captured(x => x.LogMessageEvent(null)).Args().Single());
            Assert.Equal($"xUnit.net MSBuild Runner v{versionAttribute.InformationalVersion} ({IntPtr.Size * 8}-bit Desktop .NET {Environment.Version})", eventArgs.Message);
            Assert.Equal(MessageImportance.High, eventArgs.Importance);
        }

        [Fact]
        public static void ReturnsTrueWhenExitCodeIsZeroAndFailCountIsZero()
        {
            var xunit = new Testable_xunit(exitCode: 0);

            var result = xunit.Execute();

            Assert.True(result);
        }

        [Fact]
        public static void ReturnsFalseWhenExitCodeIsNonZero()
        {
            var xunit = new Testable_xunit(exitCode: 1);

            var result = xunit.Execute();

            Assert.False(result);
        }
    }

    public class Testable_xunit : xunit
    {
        public readonly List<IRunnerReporter> AvailableReporters = new List<IRunnerReporter>();

        public Testable_xunit() : this(0) { }

        public Testable_xunit(int exitCode)
        {
            BuildEngine = Substitute.For<IBuildEngine>();
            Assemblies = new ITaskItem[0];
            ExitCode = exitCode;
        }

        protected override List<IRunnerReporter> GetAvailableRunnerReporters()
            => AvailableReporters;

        public new IRunnerReporter GetReporter()
            => base.GetReporter();
    }

    public class GetReporter
    {
        [Fact]
        public void NoReporters_UsesDefaultReporter()
        {
            var xunit = new Testable_xunit();

            var reporter = xunit.GetReporter();

            Assert.IsType<DefaultRunnerReporterWithTypes>(reporter);
        }

        [Fact]
        public void NoExplicitReporter_NoEnvironmentallyEnabledReporters_UsesDefaultReporter()
        {
            var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: false);
            var xunit = new Testable_xunit();
            xunit.AvailableReporters.Add(implicitReporter);

            var reporter = xunit.GetReporter();

            Assert.IsType<DefaultRunnerReporterWithTypes>(reporter);
        }

        [Fact]
        public void ExplicitReporter_NoEnvironmentalOverride_UsesExplicitReporter()
        {
            var explicitReporter = Mocks.RunnerReporter("switch");
            var xunit = new Testable_xunit { Reporter = "switch" };
            xunit.AvailableReporters.Add(explicitReporter);

            var reporter = xunit.GetReporter();

            Assert.Same(explicitReporter, reporter);
        }

        [Fact]
        public void ExplicitReporter_WithEnvironmentalOverride_UsesEnvironmentalOverride()
        {
            var explicitReporter = Mocks.RunnerReporter("switch");
            var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
            var xunit = new Testable_xunit { Reporter = "switch" };
            xunit.AvailableReporters.AddRange(new[] { explicitReporter, implicitReporter });

            var reporter = xunit.GetReporter();

            Assert.Same(implicitReporter, reporter);
        }

        [Fact]
        public void WithEnvironmentalOverride_WithEnvironmentalOverridesDisabled_UsesDefaultReporter()
        {
            var implicitReporter = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
            var xunit = new Testable_xunit { NoAutoReporters = true };
            xunit.AvailableReporters.Add(implicitReporter);

            var reporter = xunit.GetReporter();

            Assert.IsType<DefaultRunnerReporterWithTypes>(reporter);
        }

        [Fact]
        public void NoExplicitReporter_SelectsFirstEnvironmentallyEnabledReporter()
        {
            var explicitReporter = Mocks.RunnerReporter("switch");
            var implicitReporter1 = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
            var implicitReporter2 = Mocks.RunnerReporter(isEnvironmentallyEnabled: true);
            var xunit = new Testable_xunit();
            xunit.AvailableReporters.AddRange(new[] { explicitReporter, implicitReporter1, implicitReporter2 });

            var reporter = xunit.GetReporter();

            Assert.Same(implicitReporter1, reporter);
        }

        [Fact]
        public void BadChosenReporter_NoAvailableReporters()
        {
            var xunit = new Testable_xunit { Reporter = "foo" };

            var reporter = xunit.GetReporter();

            Assert.Null(reporter);
            var eventArgs = Assert.IsType<BuildErrorEventArgs>(xunit.BuildEngine.Captured(x => x.LogErrorEvent(null)).Args().Single());
            Assert.Equal("Reporter value 'foo' is invalid. There are no available reporters.", eventArgs.Message);
        }

        [Fact]
        public void BadChosenReporter_WithAvailableReporters()
        {
            var xunit = new Testable_xunit { Reporter = "foo" };
            xunit.AvailableReporters.AddRange(new[] { Mocks.RunnerReporter("switch1"), Mocks.RunnerReporter("switch2") });

            var reporter = xunit.GetReporter();

            Assert.Null(reporter);
            var eventArgs = Assert.IsType<BuildErrorEventArgs>(xunit.BuildEngine.Captured(x => x.LogErrorEvent(null)).Args().Single());
            Assert.Equal("Reporter value 'foo' is invalid. Available reporters: switch1, switch2", eventArgs.Message);
        }
    }
}
