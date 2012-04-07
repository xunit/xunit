using System;
using Microsoft.Build.Utilities;
using TestUtility;
using Xunit;
using Xunit.Runner.MSBuild;

public class ExitCodeIntegrationTests
{
    [Fact]
    public void PassingTest()
    {
        string code = @"
            using Xunit;

            public class MockTest
            {
                [Fact]
                public void TestMethod()
                {
                }
            }
        ";

        using (MockAssembly mockAssembly = new MockAssembly())
        {
            mockAssembly.Compile(code);

            xunit task = new xunit
            {
                Assembly = new TaskItem(mockAssembly.FileName),
                BuildEngine = new StubBuildEngine()
            };

            task.Execute();

            Assert.Equal(0, task.ExitCode);
        }
    }

    [Fact]
    public void FailingTest()
    {
        string code = @"
            using Xunit;

            public class MockTest
            {
                [Fact]
                public void TestMethod()
                {
                    Assert.True(false);
                }
            }
        ";

        using (MockAssembly mockAssembly = new MockAssembly())
        {
            mockAssembly.Compile(code);

            xunit task = new xunit
            {
                Assembly = new TaskItem(mockAssembly.FileName),
                BuildEngine = new StubBuildEngine()
            };

            task.Execute();

            Assert.Equal(-1, task.ExitCode);
        }
    }

    [Fact]
    public void InvalidAssemblyName()
    {
        xunit task = new xunit
        {
            Assembly = new TaskItem(Guid.NewGuid().ToString()),
            BuildEngine = new StubBuildEngine()
        };

        task.Execute();

        Assert.Equal(-1, task.ExitCode);
    }
}