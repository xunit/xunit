using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class TestClassCommandTests
    {
        public class Fixtures
        {
            [Fact]
            public void FixtureDataDisposeFailure_InvocationException()
            {
                TestClassCommand command = new TestClassCommand(typeof(DataDisposeFailureSpy));
                DataDisposeThrow.Exception = new TargetInvocationException(new Exception());

                ClassResult result = TestClassCommandRunner.Execute(command, null, null, null);

                Assert.Equal("System.Reflection.TargetInvocationException", result.ExceptionType);
            }

            [Fact]
            public void FixtureDataConstructorFailure_InvocationException()
            {
                TestClassCommand command = new TestClassCommand();
                command.TypeUnderTest = Reflector.Wrap(typeof(DataCtorFailureSpy));
                DataCtorThrow.Exception = new TargetInvocationException(new Exception());

                ClassResult result = TestClassCommandRunner.Execute(command, null, null, null);

                Assert.Equal("System.Reflection.TargetInvocationException", result.ExceptionType);
            }

            [Fact]
            public void CannotUseTestClassAsItsOwnFixture()
            {
                TestClassCommand command = new TestClassCommand(typeof(InvalidTestClassWithSelfFixture));

                ClassResult result = TestClassCommandRunner.Execute(command, null, null, null);

                Assert.Equal(typeof(InvalidOperationException).FullName, result.ExceptionType);
                Assert.Equal(0, result.Results.Count);
            }

            internal class DataCtorFailureSpy : FixtureSpy, IUseFixture<DataCtorThrow>
            {
                public static int dummyTestCalled;

                [Fact]
                public void DummyTest()
                {
                    dummyTestCalled++;
                }

                public void SetFixture(DataCtorThrow data) { }
            }

            internal class DataCtorThrow
            {
                public static Exception Exception;

                public DataCtorThrow()
                {
                    throw Exception;
                }
            }

            internal class DataDisposeFailureSpy : FixtureSpy, IUseFixture<DataDisposeThrow>
            {
                public static int dummyTestCalled;

                [Fact]
                public void DummyTest()
                {
                    dummyTestCalled++;
                }

                public void SetFixture(DataDisposeThrow data) { }
            }

            public class DataDisposeThrow : IDisposable
            {
                public static Exception Exception;

                public void Dispose()
                {
                    throw Exception;
                }
            }

            internal class InvalidTestClassWithSelfFixture : IUseFixture<InvalidTestClassWithSelfFixture>
            {
                [Fact]
                public void DummyTest()
                {
                    Assert.Equal(1, 1);
                }

                public void SetFixture(InvalidTestClassWithSelfFixture data) { }
            }

            public class MultipleMethods : IUseFixture<object>
            {
                public static object fixtureData = null;

                public void SetFixture(object data)
                {
                    if (fixtureData == null)
                        fixtureData = data;
                    else
                        Assert.Same(fixtureData, data);
                }

                [Fact]
                public void Test1()
                {
                }

                [Fact]
                public void Test2()
                {
                }
            }
        }

        public class Classes
        {
            [Fact]
            public void ClassResultContainsOneResultForEachTestMethod()
            {
                TestClassCommand command = new TestClassCommand();
                command.TypeUnderTest = Reflector.Wrap(typeof(Spy));

                ClassResult result = TestClassCommandRunner.Execute(command, null, null, null);

                Assert.Equal(3, result.Results.Count);
            }

            [Fact]
            public void CtorFailure()
            {
                TestClassCommand command = new TestClassCommand();
                command.TypeUnderTest = Reflector.Wrap(typeof(CtorFailureSpy));
                CtorFailureSpy.Reset();
                CtorFailureSpy.dummyTestCalled = 0;

                TestClassCommandRunner.Execute(command, null, null, null);

                Assert.Equal(1, CtorFailureSpy.dataCtorCalled);
                Assert.Equal(1, CtorFailureSpy.ctorCalled);
                Assert.Equal(0, CtorFailureSpy.dummyTestCalled);
                Assert.Equal(0, CtorFailureSpy.disposeCalled);
                Assert.Equal(1, CtorFailureSpy.dataDisposeCalled);
            }

            [Fact]
            public void DisposeFailure()
            {
                TestClassCommand command = new TestClassCommand();
                command.TypeUnderTest = Reflector.Wrap(typeof(DisposeFailureSpy));
                DisposeFailureSpy.Reset();
                DisposeFailureSpy.dummyTestCalled = 0;

                TestClassCommandRunner.Execute(command, null, null, null);

                Assert.Equal(1, DisposeFailureSpy.dataCtorCalled);
                Assert.Equal(1, DisposeFailureSpy.ctorCalled);
                Assert.Equal(1, DisposeFailureSpy.dummyTestCalled);
                Assert.Equal(1, DisposeFailureSpy.disposeCalled);
                Assert.Equal(1, DisposeFailureSpy.dataDisposeCalled);
                Assert.Equal("ctorData ctor setFixture dispose disposeData ", DisposeFailureSpy.callOrder);
            }

            [Fact]
            public void RandomizerUsedToDetermineTestOrder()
            {
                RandomSpy randomizer = new RandomSpy();
                TestClassCommand command = new TestClassCommand(typeof(OrderingSpy));
                command.Randomizer = randomizer;

                TestClassCommandRunner.Execute(command, null, null, null);

                Assert.Equal(OrderingSpy.TestMethodCount, randomizer.Next__Count);
            }

            internal class CtorFailureSpy : FixtureSpy
            {
                public static int dummyTestCalled;

                public CtorFailureSpy()
                {
                    throw new Exception();
                }

                [Fact]
                public void DummyTest()
                {
                    dummyTestCalled++;
                }
            }

            internal class DisposeFailureSpy : FixtureSpy
            {
                public static int dummyTestCalled;

                public override void Dispose()
                {
                    base.Dispose();
                    throw new Exception();
                }

                [Fact]
                public void DummyTest()
                {
                    dummyTestCalled++;
                }
            }

            internal class OrderingSpy
            {
                public const int TestMethodCount = 9;

                [Fact]
                public void Test1() { }

                [Fact]
                public void Test2() { }

                [Fact]
                public void Test3() { }

                [Fact]
                public void Test4() { }

                [Fact]
                public void Test5() { }

                [Fact]
                public void Test6() { }

                [Fact]
                public void Test7() { }

                [Fact]
                public void Test8() { }

                [Fact]
                public void Test9() { }
            }

            internal class Spy
            {
                [Fact]
                public void FailedTest()
                {
                    throw new InvalidOperationException();
                }

                public void NonTestMethod() { }

                [Fact]
                public void PassedTest() { }

                [Fact(Skip = "Reason")]
                public void Skip() { }
            }

            class RandomSpy : Random
            {
                public int Next__Count = 0;

                public override int Next(int maxValue)
                {
                    ++Next__Count;
                    return base.Next(maxValue);
                }
            }
        }

        public class Methods
        {
            [Fact]
            public void TestMethodCounters()
            {
                TestClassCommand command = new TestClassCommand();
                command.TypeUnderTest = Reflector.Wrap(typeof(InstrumentedTestClass));
                InstrumentedTestClass.Reset();
                InstrumentedTestClass.passedTestCalled = 0;
                InstrumentedTestClass.failedTestCalled = 0;
                InstrumentedTestClass.skipTestCalled = 0;
                InstrumentedTestClass.nonTestCalled = 0;

                TestClassCommandRunner.Execute(command, null, null, null);

                Assert.Equal(1, InstrumentedTestClass.dataCtorCalled);
                Assert.Equal(2, InstrumentedTestClass.ctorCalled);        // Two non-skipped tests, the skipped test does not create an instance
                Assert.Equal(1, InstrumentedTestClass.passedTestCalled);
                Assert.Equal(1, InstrumentedTestClass.failedTestCalled);
                Assert.Equal(0, InstrumentedTestClass.skipTestCalled);
                Assert.Equal(0, InstrumentedTestClass.nonTestCalled);
                Assert.Equal(2, InstrumentedTestClass.disposeCalled);
                Assert.Equal(1, InstrumentedTestClass.dataDisposeCalled);
                Assert.Equal("ctorData ctor setFixture dispose ctor setFixture dispose disposeData ", InstrumentedTestClass.callOrder);
            }

            [Fact]
            public void TestsCanBePrivateMethods()
            {
                TestClassCommand command = new TestClassCommand();
                command.TypeUnderTest = Reflector.Wrap(typeof(PrivateSpy));
                PrivateSpy.Reset();

                TestClassCommandRunner.Execute(command, null, null, null);

                Assert.True(PrivateSpy.WasRun);
            }

            [Fact]
            public void SettingSkipReasonGeneratesSkipCommand()
            {
                MethodInfo method = typeof(ClassWithSkippedTest).GetMethod("SkippedTest");
                TestClassCommand classCommand = new TestClassCommand(typeof(ClassWithSkippedTest));

                var commands = new List<ITestCommand>(classCommand.EnumerateTestCommands(Reflector.Wrap(method)));

                ITestCommand command = Assert.Single(commands);
                SkipCommand skipCommand = Assert.IsType<SkipCommand>(command);
                Assert.Equal("My Skip Reason", skipCommand.Reason);
            }

            internal class InstrumentedTestClass : FixtureSpy
            {
                public static int failedTestCalled;
                public static int nonTestCalled;
                public static int passedTestCalled;
                public static int skipTestCalled;

                [Fact]
                public void FailedTest()
                {
                    failedTestCalled++;
                }

                public void NonTestMethod()
                {
                    nonTestCalled++;
                }

                [Fact]
                public void PassedTest()
                {
                    passedTestCalled++;
                }

                [Fact(Skip = "reason")]
                public void SkippedTest()
                {
                    skipTestCalled++;
                }
            }

            internal class PrivateSpy
            {
                public static bool WasRun = false;

                [Fact]
                void PrivateTest()
                {
                    WasRun = true;
                }

                public static void Reset()
                {
                    WasRun = false;
                }
            }

            internal class ClassWithSkippedTest
            {
                [Fact(Skip = "My Skip Reason")]
                public void SkippedTest() { }
            }
        }
    }
}
