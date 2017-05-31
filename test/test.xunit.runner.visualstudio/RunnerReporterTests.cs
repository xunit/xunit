using System;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.VisualStudio.TestAdapter;

public class RunnerReporterTests
{
    public class TestRunnerReporterNotEnabled : IRunnerReporter
    {
        string IRunnerReporter.Description
        {
            get { throw new NotImplementedException(); }
        }

        bool IRunnerReporter.IsEnvironmentallyEnabled => false;

        string IRunnerReporter.RunnerSwitch
        {
            get { throw new NotImplementedException(); }
        }

        IMessageSink IRunnerReporter.CreateMessageHandler(IRunnerLogger logger)
        {
            throw new NotImplementedException();
        }
    }

    public class TestRunnerReporter : TestRunnerReporterNotEnabled, IRunnerReporter
    {
        bool IRunnerReporter.IsEnvironmentallyEnabled
        {
            get { return true; }
        }
    }

    [Fact]
    public void GetRunnerReporter()
    {
        var runnerReporter = VsTestRunner.GetRunnerReporter(new[] { Assembly.GetExecutingAssembly().Location });

        Assert.Equal(typeof(TestRunnerReporter).AssemblyQualifiedName, runnerReporter.GetType().AssemblyQualifiedName);
    }
}
