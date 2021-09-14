using System;
using System.Collections.Generic;
using System.Reflection;
using NSubstitute;
using TestDriven.Framework;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.TdNet;

public class TdNetRunnerTests
{
    private static readonly Assembly thisAssembly = typeof(TdNetRunnerTests).Assembly;

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
            var listener = Substitute.For<ITestListener>();
            var runner = new TestableTdNetRunner();

            runner.RunMember(listener, thisAssembly, typeof(TypeUnderTest));

            Assert.Collection(runner.Operations,
                msg => Assert.Equal("RunClass(type: TdNetRunnerTests+RunMember+TypeUnderTest, initialRunState: NoTests)", msg)
            );
        }

        [Fact]
        public void WithMethod()
        {
            var listener = Substitute.For<ITestListener>();
            var runner = new TestableTdNetRunner();

            runner.RunMember(listener, thisAssembly, typeof(TypeUnderTest).GetMethod("Method"));

            Assert.Collection(runner.Operations,
                msg => Assert.Equal("RunMethod(method: TdNetRunnerTests+RunMember+TypeUnderTest.Method, initialRunState: NoTests)", msg)
            );
        }

        [Fact]
        public void WithUnsupportedMemberTypes()
        {
            var listener = Substitute.For<ITestListener>();
            var runner = new TestableTdNetRunner();

            runner.RunMember(listener, thisAssembly, typeof(TypeUnderTest).GetProperty("Property"));
            runner.RunMember(listener, thisAssembly, typeof(TypeUnderTest).GetField("Field"));
            runner.RunMember(listener, thisAssembly, typeof(TypeUnderTest).GetEvent("Event"));

            Assert.Empty(runner.Operations);
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
        //     var listener = Substitute.For<ITestListener>();
        //     var runner = new TestableTdNetRunner();

        //    runner.RunMember(listener, thisAssembly, typeof(TestClassWithoutInnerClasses));

        //    Assert.Collection(runner.Controller.Operations,
        //        msg => Assert.Equal("Discovery: type TdNetRunnerTests+RunMember+TestClassWithoutInnerClasses (includeSourceInformation = False)", msg),
        //        msg => Assert.Equal("Run: 1 test case(s)", msg)
        //     );
        // }

        // [Fact]
        //public void RunClassRunsMembersInTheInnerClasses()
        // {
        //     var listener = Substitute.For<ITestListener>();
        //     var runner = new TestableTdNetRunner();

        //    runner.RunMember(listener, thisAssembly, typeof(TestClassWithInnerClasses));

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
        //     var listener = Substitute.For<ITestListener>();
        //     var runner = new TestableTdNetRunner();
        //    var testCase = Mocks.TestCase<RunMember>("RunMethod");
        //    runner.Controller.TestCasesToRun.Add(testCase.Object);

        //    runner.RunMember(listener, testCase.Assembly, testCase.MethodInfo);

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
            var listener = Substitute.For<ITestListener>();
            var runner = new TestableTdNetRunner();
            var testCaseInNamespace = Mocks.TestCase<DummyNamespace.ClassInNamespace>("TestMethod");
            var testCaseOutsideOfNamespace = Mocks.TestCase<RunNamespace>("RunsOnlyTestMethodsInTheGivenNamespace");
            runner.TestsToDiscover.Clear();
            runner.TestsToDiscover.Add(testCaseInNamespace);
            runner.TestsToDiscover.Add(testCaseOutsideOfNamespace);

            runner.RunNamespace(listener, typeof(DummyNamespace.ClassInNamespace).Assembly, "DummyNamespace");

            Assert.Collection(runner.Operations,
                msg => Assert.Equal("Discovery()", msg),
                msg => Assert.Equal("Run(initialRunState: NoTests)", msg)
            );
            Assert.Collection(runner.TestsRun,
                testCase => Assert.Same(testCaseInNamespace, testCase)
            );
        }
    }

    class TestableTdNetRunner : TdNetRunner
    {
        public TdNetRunnerHelper Helper;
        public List<string> Operations = new List<string>();
        public List<ITestCase> TestsRun = new List<ITestCase>();
        public List<ITestCase> TestsToDiscover = new List<ITestCase> { Substitute.For<ITestCase>() };

        public TestableTdNetRunner()
        {
            Helper = Substitute.For<TdNetRunnerHelper>();

            Helper.Discover().Returns(
                callInfo =>
                {
                    Operations.Add("Discovery()");
                    return TestsToDiscover;
                });

            Helper.Run(null, TestRunState.NoTests).ReturnsForAnyArgs(
                callInfo =>
                {
                    Operations.Add($"Run(initialRunState: {callInfo[1]})");
                    TestsRun.AddRange((IEnumerable<ITestCase>)callInfo[0]);
                    return TestRunState.NoTests;
                });

            Helper.RunClass(null, TestRunState.NoTests).ReturnsForAnyArgs(
                callInfo =>
                {
                    Operations.Add($"RunClass(type: {callInfo[0]}, initialRunState: {callInfo[1]})");
                    return TestRunState.NoTests;
                });

            Helper.RunMethod(null, TestRunState.NoTests).ReturnsForAnyArgs(
                callInfo =>
                {
                    var method = (MethodInfo)callInfo[0];
                    Operations.Add($"RunMethod(method: {method.DeclaringType.FullName}.{method.Name}, initialRunState: {callInfo[1]})");
                    return TestRunState.NoTests;
                });
        }

        public override TdNetRunnerHelper CreateHelper(ITestListener testListener, Assembly assembly)
        {
            return Helper;
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
