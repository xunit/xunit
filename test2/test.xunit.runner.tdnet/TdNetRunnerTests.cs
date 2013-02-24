using System;
using System.Reflection;
using Moq;
using TestDriven.Framework;
using Xunit;
using Xunit.Runner.TdNet;

public class TdNetRunnerTests
{
    private static readonly Assembly thisAssembly = typeof(TdNetRunnerTests).Assembly;

    public class RunAssembly
    {
        [Fact]
        public void DiscoversTestsInAssemblyAndRunsThem()
        {
            var listener = new Mock<ITestListener>();
            var runner = new TestableTdNetRunner();

            runner.RunAssembly(listener.Object, thisAssembly);

            Assert.Collection(runner.Helper.Operations,
                msg => Assert.Equal("Discovery()", msg),
                msg => Assert.Equal("Run(initialRunState: NoTests)", msg)
            );
        }

        //[Fact]
        //public void CallsDiscoverToGetListOfTestsInAssembly()
        //{
        //    var listener = new Mock<ITestListener>();
        //    var runner = new TestableTdNetRunner();

        //    runner.RunAssembly(listener.Object, thisAssembly);

        //    runner.Controller.Verify(c => c.Find(false, It.IsAny<IMessageSink>()));
        //}

        //[Fact]
        //public void CallsRunToRunTheListOfTests()
        //{
        //    var listener = new Mock<ITestListener>();
        //    var runner = new TestableTdNetRunner();

        //    runner.RunAssembly(listener.Object, thisAssembly);

        //    Assert.Equal(runner.Controller.TestCasesToRun, runner.Controller.TestCasesRan);
        //}
    }

    public class RunMember
    {
        class TypeUnderTest
        {
#pragma warning disable 67,649
            public event Action Event;
            public int Field;
            public int Property { get; set; }
            public void Method() { }
#pragma warning restore 67,649
        }

        [Fact]
        public void WithType()
        {
            var listener = new Mock<ITestListener>();
            var runner = new TestableTdNetRunner();

            runner.RunMember(listener.Object, thisAssembly, typeof(TypeUnderTest));

            Assert.Collection(runner.Helper.Operations,
                msg => Assert.Equal("RunClass(type: TdNetRunnerTests+RunMember+TypeUnderTest, initialRunState: NoTests)", msg)
            );
        }

        [Fact]
        public void WithMethod()
        {
            var listener = new Mock<ITestListener>();
            var runner = new TestableTdNetRunner();

            runner.RunMember(listener.Object, thisAssembly, typeof(TypeUnderTest).GetMethod("Method"));

            Assert.Collection(runner.Helper.Operations,
                msg => Assert.Equal("RunMethod(method: TdNetRunnerTests+RunMember+TypeUnderTest.Method, initialRunState: NoTests)", msg)
            );
        }

        [Fact]
        public void WithUnsupportedMemberTypes()
        {
            var listener = new Mock<ITestListener>();
            var runner = new TestableTdNetRunner();

            runner.RunMember(listener.Object, thisAssembly, typeof(TypeUnderTest).GetProperty("Property"));
            runner.RunMember(listener.Object, thisAssembly, typeof(TypeUnderTest).GetField("Field"));
            runner.RunMember(listener.Object, thisAssembly, typeof(TypeUnderTest).GetEvent("Event"));

            Assert.Empty(runner.Helper.Operations);
        }

        //class TestClassWithoutInnerClasses { }

        //class TestClassWithInnerClasses
        //{
        //    public class InnerClass1 { }
        //    public class InnerClass2 { }
        //}

        // [Fact]
        //public void RunClassRunsMembersInTheClass()
        // {
        //     var listener = new Mock<ITestListener>();
        //     var runner = new TestableTdNetRunner();

        //    runner.RunMember(listener.Object, thisAssembly, typeof(TestClassWithoutInnerClasses));

        //    Assert.Collection(runner.Controller.Operations,
        //        msg => Assert.Equal("Discovery: type TdNetRunnerTests+RunMember+TestClassWithoutInnerClasses (includeSourceInformation = False)", msg),
        //        msg => Assert.Equal("Run: 1 test case(s)", msg)
        //     );
        // }

        // [Fact]
        //public void RunClassRunsMembersInTheInnerClasses()
        // {
        //     var listener = new Mock<ITestListener>();
        //     var runner = new TestableTdNetRunner();

        //    runner.RunMember(listener.Object, thisAssembly, typeof(TestClassWithInnerClasses));

        //    Assert.Collection(runner.Controller.Operations,
        //        msg => Assert.Equal("Discovery: type TdNetRunnerTests+RunMember+TestClassWithInnerClasses (includeSourceInformation = False)", msg),
        //        msg => Assert.Equal("Run: 1 test case(s)", msg),
        //        msg => Assert.Equal("Discovery: type TdNetRunnerTests+RunMember+TestClassWithInnerClasses+InnerClass1 (includeSourceInformation = False)", msg),
        //        msg => Assert.Equal("Run: 1 test case(s)", msg),
        //        msg => Assert.Equal("Discovery: type TdNetRunnerTests+RunMember+TestClassWithInnerClasses+InnerClass2 (includeSourceInformation = False)", msg),
        //        msg => Assert.Equal("Run: 1 test case(s)", msg)
        //     );
        // }

        // [Fact]
        //public void RunMethod()
        // {
        //     var listener = new Mock<ITestListener>();
        //     var runner = new TestableTdNetRunner();
        //    var testCase = new MockTestCase<RunMember>("RunMethod");
        //    runner.Controller.TestCasesToRun.Add(testCase.Object);

        //    runner.RunMember(listener.Object, testCase.Assembly, testCase.MethodInfo);

        //    Assert.Collection(runner.Controller.Operations,
        //        msg => Assert.Equal("Discovery: type TdNetRunnerTests+RunMember (includeSourceInformation = False)", msg),
        //        msg => Assert.Equal("Run: 1 test case(s)", msg)
        //    );
        // }
    }

    public class RunNamespace
    {
        [Fact]
        public void RunsOnlyTestMethodsInTheGivenNamespace()
        {
            var listener = new Mock<ITestListener>();
            var runner = new TestableTdNetRunner();
            var testCaseInNamespace = new MockTestCase<DummyNamespace.ClassInNamespace>("TestMethod");
            var testCaseOutsideOfNamespace = new MockTestCase<RunNamespace>("RunsOnlyTestMethodsInTheGivenNamespace");
            runner.Helper.TestsToDiscover.Clear();
            runner.Helper.TestsToDiscover.Add(testCaseInNamespace.Object);
            runner.Helper.TestsToDiscover.Add(testCaseOutsideOfNamespace.Object);

            runner.RunNamespace(listener.Object, testCaseInNamespace.Assembly, "DummyNamespace");

            Assert.Collection(runner.Helper.Operations,
                msg => Assert.Equal("Discovery()", msg),
                msg => Assert.Equal("Run(initialRunState: NoTests)", msg)
            );
            Assert.Collection(runner.Helper.TestsRun,
                testCase => Assert.Same(testCaseInNamespace.Object, testCase)
            );
        }
    }

    class TestableTdNetRunner : TdNetRunner
    {
        public TestableTdNetRunner()
        {
            Helper = new MockTdNetRunnerHelper();
        }

        public MockTdNetRunnerHelper Helper;

        public override TdNetRunnerHelper CreateHelper(ITestListener testListener, Assembly assembly)
        {
            return Helper.Object;
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