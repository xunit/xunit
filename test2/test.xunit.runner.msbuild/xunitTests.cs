using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.MSBuild;
using Xunit.Sdk;

public class xunitTests
{
    public class CreateVisitor
    {
        [Fact]
        public void DefaultVisitorIsStandardOutputVisitor()
        {
            var xunit = new Testable_xunit { TeamCity = false };

            var visitor = xunit.CreateVisitor_Public("filename");

            Assert.IsType<StandardOutputVisitor>(visitor);
        }

        [Fact]
        public void VisitorIsTeamCityVisitorWhenTeamCityIsTrue()
        {
            var xunit = new Testable_xunit { TeamCity = true };

            var visitor = xunit.CreateVisitor_Public("filename");

            Assert.IsType<TeamCityVisitor>(visitor);
        }
    }

    public class Execute
    {
        [Fact, PreserveWorkingDirectory]
        public void ChangesCurrentDirectoryWhenWorkingFolderIsNotNull()
        {
            var tempFolder = Environment.GetEnvironmentVariable("TEMP");
            var xunit = new Testable_xunit { WorkingFolder = tempFolder };

            xunit.Execute();

            Assert.Equal(tempFolder, Directory.GetCurrentDirectory());
        }

        [Fact]
        public void DoesNotChangeCurrentDirectoryWhenWorkingFolderIsNull()
        {
            var currentFolder = Directory.GetCurrentDirectory();
            var xunit = new Testable_xunit();

            xunit.Execute();

            Assert.Equal(currentFolder, Directory.GetCurrentDirectory());
        }

        [Fact]
        public void LogsWelcomeBanner()
        {
            var xunit = new Testable_xunit();

            xunit.Execute();

            xunit.MockBuildEngine.Verify(b => b.LogMessageEvent(It.Is<BuildMessageEventArgs>(bmea => ValidateWelcomeBanner(bmea))));
        }

        private bool ValidateWelcomeBanner(BuildMessageEventArgs eventArgs)
        {
            Assert.Equal(String.Format("xUnit.net MSBuild runner ({0}-bit .NET {1})", IntPtr.Size * 8, Environment.Version), eventArgs.Message);
            Assert.Equal(MessageImportance.High, eventArgs.Importance);
            return true;
        }

        [Fact]
        public void CallsExecuteAssemblyOnceForEachAssembly()
        {
            var assm1 = new TaskItem(@"C:\Full\Path\1");
            var assm2 = new TaskItem(@"C:\Full\Path\2", new Dictionary<string, string> { { "ConfigFile", @"C:\Config\File" } });
            var mockXunit = new Mock<Testable_xunit> { CallBase = true };
            mockXunit.Object.Assemblies = new ITaskItem[] { assm1, assm2 };
            mockXunit.Setup(x => x.ExecuteAssembly_Public(@"C:\Full\Path\1", null)).Verifiable();
            mockXunit.Setup(x => x.ExecuteAssembly_Public(@"C:\Full\Path\2", @"C:\Config\File")).Verifiable();

            mockXunit.Object.Execute();

            mockXunit.Verify();
        }

        [Fact]
        public void ReturnsTrueWhenExitCodeIsZeroAndFailCountIsZero()
        {
            var xunit = new Testable_xunit(exitCode: 0);

            var result = xunit.Execute();

            Assert.True(result);
        }

        [Fact]
        public void ReturnsFalseWhenExitCodeIsNonZero()
        {
            var xunit = new Testable_xunit(exitCode: 1);

            var result = xunit.Execute();

            Assert.False(result);
        }

        [Fact]
        public void ReturnsFalseWhenFailCountIsNonZero()
        {
            var visitor = new MSBuildVisitor(null, null) { Failed = 1 };
            var mockXunit = new Mock<Testable_xunit> { CallBase = true };
            mockXunit.Setup(x => x.CreateVisitor_Public(It.IsAny<string>())).Returns(visitor);
            mockXunit.Object.Assemblies = new[] { new Mock<ITaskItem>().Object };

            var result = mockXunit.Object.Execute();

            Assert.False(result);
        }

        [Fact]
        public void StopsExecutingAssemblyWhenCanceled()
        {
            var assembly = new TaskItem(@"C:\Full\Path\1");
            var mockXunit = new Mock<Testable_xunit> { CallBase = true };
            mockXunit.Object.Assemblies = new ITaskItem[] { assembly };
            mockXunit.Setup(x => x.ExecuteAssembly_Public(It.IsAny<string>(), It.IsAny<string>()));
            mockXunit.Object.Cancel();

            mockXunit.Object.Execute();

            mockXunit.Verify(x => x.ExecuteAssembly_Public(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }

    public class ExecuteAssembly
    {
        [Fact]
        public void LogsAssemblyMessage()
        {
            var xunit = new Testable_xunit();

            xunit.ExecuteAssembly_Public("assemblyFilename", "configFilename");

            xunit.MockBuildEngine.Verify(b => b.LogMessageEvent(It.Is<BuildMessageEventArgs>(bmea => ValidateAssemblyBanner(bmea))));
        }

        private bool ValidateAssemblyBanner(BuildMessageEventArgs eventArgs)
        {
            Assert.Equal("Test assembly: assemblyFilename", eventArgs.Message);
            Assert.Equal(MessageImportance.High, eventArgs.Importance);
            return true;
        }

        [Fact]
        public void DisposesOfFrontController()
        {
            var xunit = new Testable_xunit();

            xunit.ExecuteAssembly_Public("assemblyFilename", "configFilename");

            xunit.MockFrontController.Verify(c => c.Dispose());
        }

        [Fact]
        public void DiscoversAllTestsInAssembly()
        {
            var xunit = new Testable_xunit();

            xunit.ExecuteAssembly_Public("assemblyFilename", "configFilename");

            xunit.MockFrontController.Verify(fc => fc.Find(false, It.IsAny<IMessageSink>()));
        }

        [Fact]
        public void RunsDiscoveredTests()
        {
            var xunit = new Testable_xunit();
            var runTestCases = new List<ITestCase>();
            xunit.MockFrontController.Setup(fc => fc.Run(It.IsAny<IEnumerable<ITestCase>>(), It.IsAny<IMessageSink>()))
                                     .Callback<IEnumerable<ITestCase>, IMessageSink>((testCases, sink) =>
                                     {
                                         runTestCases.AddRange(testCases);
                                         sink.OnMessage(new TestAssemblyFinished());
                                     });

            xunit.ExecuteAssembly_Public("assemblyFilename", "configFilename");

            Assert.Equal(xunit.DiscoveryTestCases, runTestCases);
        }

        [Fact]
        public void ErrorsDuringExecutionAreLogged()
        {
            var exception = new DivideByZeroException();
            var xunit = new Mock<Testable_xunit> { CallBase = true };
            var messages = new List<string>();
            xunit.Setup(fc => fc.CreateFrontController_Public(It.IsAny<string>(), It.IsAny<string>()))
                 .Throws(exception);

            xunit.Object.ExecuteAssembly_Public("assemblyFilename", "configFilename");

            xunit.Object.MockBuildEngine.Verify(b => b.LogErrorEvent(It.Is<BuildErrorEventArgs>(beea => CaptureErrorMessage(beea, messages))));
            Assert.Equal<object>("System.DivideByZeroException: Attempted to divide by zero.", messages[0]);
        }

        private bool CaptureErrorMessage(BuildErrorEventArgs eventArgs, List<string> messages)
        {
            messages.Add(eventArgs.Message);
            return true;
        }
    }

    public class Testable_xunit : xunit
    {
        public readonly Mock<IBuildEngine> MockBuildEngine;
        public readonly Mock<IFrontController> MockFrontController;
        public readonly List<ITestCase> DiscoveryTestCases = new List<ITestCase>();
        public readonly MSBuildVisitor Visitor = new MSBuildVisitor(null, null);

        public Testable_xunit() : this(0) { }

        public Testable_xunit(int exitCode)
        {
            MockBuildEngine = new Mock<IBuildEngine>();
            MockBuildEngine.As<IBuildEngine2>();
            MockBuildEngine.As<IBuildEngine3>();
            MockBuildEngine.As<IBuildEngine4>();

            MockFrontController = new Mock<IFrontController>();
            MockFrontController.Setup(fc => fc.Find(It.IsAny<bool>(), It.IsAny<IMessageSink>()))
                               .Callback<bool, IMessageSink>(ReturnDiscoveryMessages);
            MockFrontController.Setup(fc => fc.Find(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IMessageSink>()))
                               .Callback<string, bool, IMessageSink>((_, __, sink) => ReturnDiscoveryMessages(__, sink));
            MockFrontController.Setup(fc => fc.Run(It.IsAny<IEnumerable<ITestCase>>(), It.IsAny<IMessageSink>()))
                               .Callback<IEnumerable<ITestCase>, IMessageSink>((_, sink) => sink.OnMessage(new TestAssemblyFinished()));

            BuildEngine = MockBuildEngine.Object;

            Assemblies = new ITaskItem[0];

            ExitCode = exitCode;
        }

        public virtual IFrontController CreateFrontController_Public(string assemblyFilename, string configFileName)
        {
            return MockFrontController.Object;
        }

        protected override IFrontController CreateFrontController(string assemblyFilename, string configFileName)
        {
            return CreateFrontController_Public(assemblyFilename, configFileName);
        }

        public virtual MSBuildVisitor CreateVisitor_Public(string assemblyFileName)
        {
            return base.CreateVisitor(assemblyFileName);
        }

        protected override MSBuildVisitor CreateVisitor(string assemblyFileName)
        {
            return CreateVisitor_Public(assemblyFileName);
        }

        public virtual void ExecuteAssembly_Public(string assemblyFilename, string configFileName)
        {
            base.ExecuteAssembly(assemblyFilename, configFileName, new MSBuildVisitor(null, null));
        }

        protected override void ExecuteAssembly(string assemblyFilename, string configFileName, MSBuildVisitor resultsVisitor)
        {
            ExecuteAssembly_Public(assemblyFilename, configFileName);
        }

        private void ReturnDiscoveryMessages(bool _, IMessageSink sink)
        {
            foreach (var testCase in DiscoveryTestCases)
                sink.OnMessage(new TestCaseDiscoveryMessage { TestCase = testCase });

            sink.OnMessage(new DiscoveryCompleteMessage());
        }
    }
}