using System.Collections.Generic;
using System.Reflection;
using Moq;
using TestDriven.Framework;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.TdNet;

public class TdNetRunnerTests
{
    private static readonly Assembly thisAssembly = typeof(TdNetRunnerTests).Assembly;

    public class RunAssembly
    {
        [Fact]
        public void CallsDiscoverToGetListOfTestsInAssembly()
        {
            var listener = new Mock<ITestListener>();
            var runner = new TestableTdNetRunner();

            runner.RunAssembly(listener.Object, thisAssembly);

            runner.Controller.Verify(c => c.Find(false, It.IsAny<IMessageSink>()));
        }

        [Fact]
        public void CallsRunToRunTheListOfTests()
        {
            var listener = new Mock<ITestListener>();
            var runner = new TestableTdNetRunner();

            runner.RunAssembly(listener.Object, thisAssembly);

            Assert.Equal(runner.Controller.TestCasesToRun, runner.Controller.TestCasesRan);
        }
    }

    public class RunMember
    {
        class TestClassWithoutInnerClasses { }

        class TestClassWithInnerClasses
        {
            public class InnerClass1 { }
            public class InnerClass2 { }
        }

        [Fact]
        public void RunClassRunsMembersInTheClass()
        {
            var listener = new Mock<ITestListener>();
            var runner = new TestableTdNetRunner();

            runner.RunMember(listener.Object, thisAssembly, typeof(TestClassWithoutInnerClasses));

            Assert.Collection(runner.Controller.Operations,
                msg => Assert.Equal("Discovery: type TdNetRunnerTests+RunMember+TestClassWithoutInnerClasses (includeSourceInformation = False)", msg),
                msg => Assert.Equal("Run: 1 test case(s)", msg)
            );
        }

        [Fact]
        public void RunClassRunsMembersInTheInnerClasses()
        {
            var listener = new Mock<ITestListener>();
            var runner = new TestableTdNetRunner();

            runner.RunMember(listener.Object, thisAssembly, typeof(TestClassWithInnerClasses));

            Assert.Collection(runner.Controller.Operations,
                msg => Assert.Equal("Discovery: type TdNetRunnerTests+RunMember+TestClassWithInnerClasses (includeSourceInformation = False)", msg),
                msg => Assert.Equal("Run: 1 test case(s)", msg),
                msg => Assert.Equal("Discovery: type TdNetRunnerTests+RunMember+TestClassWithInnerClasses+InnerClass1 (includeSourceInformation = False)", msg),
                msg => Assert.Equal("Run: 1 test case(s)", msg),
                msg => Assert.Equal("Discovery: type TdNetRunnerTests+RunMember+TestClassWithInnerClasses+InnerClass2 (includeSourceInformation = False)", msg),
                msg => Assert.Equal("Run: 1 test case(s)", msg)
            );
        }

        [Fact]
        public void RunMethod()
        {
            var listener = new Mock<ITestListener>();
            var runner = new TestableTdNetRunner();
            var testCase = new MockTestCase<RunMember>("RunMethod");
            runner.Controller.TestCasesToRun.Add(testCase.Object);

            runner.RunMember(listener.Object, testCase.Assembly, testCase.MethodInfo);

            Assert.Collection(runner.Controller.Operations,
                msg => Assert.Equal("Discovery: type TdNetRunnerTests+RunMember (includeSourceInformation = False)", msg),
                msg => Assert.Equal("Run: 1 test case(s)", msg)
            );
        }
    }

    public class RunNamespace
    {
        [Fact]
        public void RunsOnlyTestMethodsInTheGivenNamespace()
        {
            var listener = new Mock<ITestListener>();
            var runner = new TestableTdNetRunner();
            runner.Controller.TestCasesToRun.Clear();
            var testCaseInNamespace = new MockTestCase<DummyNamespace.ClassInNamespace>("TestMethod");
            runner.Controller.TestCasesToRun.Add(testCaseInNamespace.Object);
            var testCaseOutsideOfNamespace = new MockTestCase<RunNamespace>("RunsOnlyTestMethodsInTheGivenNamespace");
            runner.Controller.TestCasesToRun.Add(testCaseOutsideOfNamespace.Object);

            runner.RunNamespace(listener.Object, testCaseInNamespace.Assembly, "DummyNamespace");

            var result = Assert.Single(runner.Controller.TestCasesRan);
            Assert.Same(testCaseInNamespace.Object, result);
        }
    }

    class TestableTdNetRunner : TdNetRunner
    {
        public TestableTdNetRunner()
        {
            Controller = new MockXunitController();
        }

        public MockXunitController Controller;

        protected override IXunitController CreateController(string assemblyFileName)
        {
            return Controller.Object;
        }
    }
}

namespace DummyNamespace
{
    public class ClassInNamespace
    {
        public void TestMethod() { }
    }
}