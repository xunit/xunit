using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using IAttributeInfo = Xunit.Abstractions.IAttributeInfo;
using IMethodInfo = Xunit.Abstractions.IMethodInfo;
using ITypeInfo = Xunit.Abstractions.ITypeInfo;

public class XunitTestCaseTests
{
    [Fact]
    public void DefaultFactAttribute()
    {
        var testCollection = new XunitTestCollection();
        var fact = Mocks.FactAttribute();
        var method = Mocks.MethodInfo();
        var type = Mocks.TypeInfo(methods: new[] { method });
        var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

        var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact);

        Assert.Equal("MockType.MockMethod", testCase.DisplayName);
        Assert.Null(testCase.SkipReason);
        Assert.Empty(testCase.Traits);
    }

    [Fact]
    public void SkipReason()
    {
        var testCollection = new XunitTestCollection();
        var fact = Mocks.FactAttribute(skip: "Skip Reason");
        var method = Mocks.MethodInfo();
        var type = Mocks.TypeInfo(methods: new[] { method });
        var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

        var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact);

        Assert.Equal("Skip Reason", testCase.SkipReason);
    }

    [Fact]
    public void Traits()
    {
        var testCollection = new XunitTestCollection();
        var fact = Mocks.FactAttribute();
        var trait1 = Mocks.TraitAttribute("Trait1", "Value1");
        var trait2 = Mocks.TraitAttribute("Trait2", "Value2");
        var method = Mocks.MethodInfo(attributes: new[] { trait1, trait2 });
        var type = Mocks.TypeInfo(methods: new[] { method });
        var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

        var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact);

        Assert.Equal("Value1", Assert.Single(testCase.Traits["Trait1"]));
        Assert.Equal("Value2", Assert.Single(testCase.Traits["Trait2"]));
    }

    [Fact]
    public void TraitsOnClass()
    {
        var testCollection = new XunitTestCollection();
        var fact = Mocks.FactAttribute();
        var trait1 = Mocks.TraitAttribute("Trait1", "Value1");
        var trait2 = Mocks.TraitAttribute("Trait2", "Value2");
        var method = Mocks.MethodInfo();
        var type = Mocks.TypeInfo(methods: new[] { method }, attributes: new[] { trait1, trait2 });
        var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

        var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact);

        Assert.Equal("Value1", Assert.Single(testCase.Traits["Trait1"]));
        Assert.Equal("Value2", Assert.Single(testCase.Traits["Trait2"]));
    }

    public class DisplayName
    {
        [Fact]
        public void CustomDisplayName()
        {
            var testCollection = new XunitTestCollection();
            var fact = Mocks.FactAttribute(displayName: "Custom Display Name");
            var method = Mocks.MethodInfo();
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

            var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact);

            Assert.Equal("Custom Display Name", testCase.DisplayName);
        }

        [Fact]
        public void CorrectNumberOfTestArguments()
        {
            var testCollection = new XunitTestCollection();
            var fact = Mocks.FactAttribute();
            var param1 = Mocks.ParameterInfo("p1");
            var param2 = Mocks.ParameterInfo("p2");
            var param3 = Mocks.ParameterInfo("p3");
            var method = Mocks.MethodInfo(parameters: new[] { param1, param2, param3 });
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });
            var arguments = new object[] { 42, "Hello, world!", 'A' };

            var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact, arguments);

            Assert.Equal("MockType.MockMethod(p1: 42, p2: \"Hello, world!\", p3: 'A')", testCase.DisplayName);
        }

        [Fact]
        public void NotEnoughTestArguments()
        {
            var testCollection = new XunitTestCollection();
            var fact = Mocks.FactAttribute();
            var param = Mocks.ParameterInfo("p1");
            var method = Mocks.MethodInfo(parameters: new[] { param });
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

            var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact, arguments: new object[0]);

            Assert.Equal("MockType.MockMethod(p1: ???)", testCase.DisplayName);
        }

        [Fact]
        public void TooManyTestArguments()
        {
            var testCollection = new XunitTestCollection();
            var fact = Mocks.FactAttribute();
            var param = Mocks.ParameterInfo("p1");
            var method = Mocks.MethodInfo(parameters: new[] { param });
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });
            var arguments = new object[] { 42, 21.12 };

            var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact, arguments);

            Assert.Equal("MockType.MockMethod(p1: 42, ???: 21.12)", testCase.DisplayName);
        }
    }

    public class Run
    {
        [Fact]
        public void IssuesTestCaseMessagesAndCallsRunTests()
        {
            var testCase = TestableXunitTestCase.Create();
            var bus = new SpyMessageBus<ITestCaseFinished>();

            testCase.Run(bus);
            bus.Finished.WaitOne();

            Assert.Collection(bus.Messages,
                message =>
                {
                    var testCaseStarting = Assert.IsAssignableFrom<ITestCaseStarting>(message);
                    Assert.Same(testCase, testCaseStarting.TestCase);
                },
                message => Assert.IsType<SpyMessage>(message),
                message =>
                {
                    var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(message);
                    Assert.Same(testCase, testCaseFinished.TestCase);
                    Assert.Equal(0, testCaseFinished.TestsRun);
                    Assert.Equal(0, testCaseFinished.TestsFailed);
                    Assert.Equal(0, testCaseFinished.TestsSkipped);
                    Assert.Equal(0M, testCaseFinished.ExecutionTime);
                }
            );
        }

        [Fact]
        public void CountsTestResultMessages()
        {
            var testCase = TestableXunitTestCase.Create(msgBus =>
            {
                msgBus.QueueMessage(Substitute.For<ITestResultMessage>());
                msgBus.QueueMessage(Substitute.For<ITestPassed>());
                msgBus.QueueMessage(Substitute.For<ITestFailed>());
                msgBus.QueueMessage(Substitute.For<ITestSkipped>());
            });
            var bus = new SpyMessageBus<ITestCaseFinished>();

            testCase.Run(bus);
            bus.Finished.WaitOne();

            var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(bus.Messages.Last());
            Assert.Equal(4, testCaseFinished.TestsRun);
        }

        [Fact]
        public void CountsTestsFailed()
        {
            var testCase = TestableXunitTestCase.Create(msgBus =>
            {
                msgBus.QueueMessage(new TestFailed());
                msgBus.QueueMessage(new TestFailed());
            });
            var bus = new SpyMessageBus<ITestCaseFinished>();

            testCase.Run(bus);
            bus.Finished.WaitOne();

            var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(bus.Messages.Last());
            Assert.Equal(2, testCaseFinished.TestsFailed);
        }

        [Fact]
        public void CountsTestsSkipped()
        {
            var testCase = TestableXunitTestCase.Create(msgBus =>
            {
                msgBus.QueueMessage(new TestSkipped());
                msgBus.QueueMessage(new TestSkipped());
            });
            var bus = new SpyMessageBus<ITestCaseFinished>();

            testCase.Run(bus);
            bus.Finished.WaitOne();

            var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(bus.Messages.Last());
            Assert.Equal(2, testCaseFinished.TestsSkipped);
        }

        [Fact]
        public void AggregatesTestRunTime()
        {
            var testCase = TestableXunitTestCase.Create(msgBus =>
            {
                msgBus.QueueMessage(new TestPassed { ExecutionTime = 1.2M });
                msgBus.QueueMessage(new TestFailed { ExecutionTime = 2.3M });
            });
            var bus = new SpyMessageBus<ITestCaseFinished>();

            testCase.Run(bus);
            bus.Finished.WaitOne();

            var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(bus.Messages.Last());
            Assert.Equal(3.5M, testCaseFinished.ExecutionTime);
        }
    }

    public class RunTests
    {
        public class StaticTestMethods
        {
            [Fact]
            public void Skipped()
            {
                var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "SkippedMethod");

                testCase.RunTests();

                Assert.Collection(testCase.Messages,
                    message => Assert.IsAssignableFrom<ITestStarting>(message),
                    message => Assert.IsAssignableFrom<ITestSkipped>(message),
                    message => Assert.IsAssignableFrom<ITestFinished>(message)
                );
            }

            [Fact]
            public void NonSkipped()
            {
                var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "NonSkippedMethod");

                testCase.RunTests();

                Assert.Collection(testCase.Messages,
                    message => Assert.IsAssignableFrom<ITestStarting>(message),
                    message => Assert.IsAssignableFrom<ITestPassed>(message),
                    message => Assert.IsAssignableFrom<ITestFinished>(message)
                );
            }

            class ClassUnderTest
            {
                [Fact]
                public static void NonSkippedMethod() { }

                [Fact(Skip = "Please don't run me")]
                public static void SkippedMethod()
                {
                    throw new NotImplementedException();
                }
            }
        }

        public class ConstructorWithoutDispose
        {
            [Fact]
            public void Skipped()
            {
                var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "SkippedMethod");

                testCase.RunTests();

                Assert.Collection(testCase.Messages,
                    message => Assert.IsAssignableFrom<ITestStarting>(message),
                    message => Assert.IsAssignableFrom<ITestSkipped>(message),
                    message => Assert.IsAssignableFrom<ITestFinished>(message)
                );
            }

            [Fact]
            public void NonSkipped()
            {
                var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "NonSkippedMethod");

                testCase.RunTests();

                Assert.Collection(testCase.Messages,
                    message => Assert.IsAssignableFrom<ITestStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                    message => Assert.IsAssignableFrom<ITestPassed>(message),
                    message => Assert.IsAssignableFrom<ITestFinished>(message)
                );
            }

            class ClassUnderTest
            {
                [Fact]
                public void NonSkippedMethod() { }

                [Fact(Skip = "Please don't run me")]
                public void SkippedMethod()
                {
                    throw new NotImplementedException();
                }
            }

            [Fact]
            public void ThrowingConstructor()
            {
                var testCase = TestableXunitTestCase.Create(typeof(ThrowingCtorClassUnderTest), "NonSkippedMethod");

                testCase.RunTests();

                Assert.Collection(testCase.Messages,
                    message => Assert.IsAssignableFrom<ITestStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                    message =>
                    {
                        ITestFailed failed = Assert.IsAssignableFrom<ITestFailed>(message);
                        Assert.Equal(typeof(DivideByZeroException).FullName, failed.ExceptionType);
                    },
                    message => Assert.IsAssignableFrom<ITestFinished>(message)
                );
            }

            class ThrowingCtorClassUnderTest
            {
                public ThrowingCtorClassUnderTest()
                {
                    throw new DivideByZeroException();
                }

                [Fact]
                public void NonSkippedMethod()
                {
                    throw new InvalidFilterCriteriaException();
                }
            }
        }

        public class ConstructorWithDispose
        {
            [Fact]
            public void Skipped()
            {
                var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "SkippedMethod");

                testCase.RunTests();

                Assert.Collection(testCase.Messages,
                    message => Assert.IsAssignableFrom<ITestStarting>(message),
                    message => Assert.IsAssignableFrom<ITestSkipped>(message),
                    message => Assert.IsAssignableFrom<ITestFinished>(message)
                );
            }

            [Fact]
            public void NonSkipped()
            {
                var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "NonSkippedMethod");

                testCase.RunTests();

                Assert.Collection(testCase.Messages,
                    message => Assert.IsAssignableFrom<ITestStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                    message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                    message => Assert.IsAssignableFrom<ITestPassed>(message),
                    message => Assert.IsAssignableFrom<ITestFinished>(message)
                );
            }

            class ClassUnderTest : IDisposable
            {
                [Fact]
                public void NonSkippedMethod() { }

                [Fact(Skip = "Please don't run me")]
                public void SkippedMethod()
                {
                    throw new NotImplementedException();
                }

                public void Dispose() { }
            }

            [Fact]
            public void ThrowingConstructor()
            {
                var testCase = TestableXunitTestCase.Create(typeof(ThrowingCtorClassUnderTest), "NonSkippedMethod");

                testCase.RunTests();

                Assert.Collection(testCase.Messages,
                    message => Assert.IsAssignableFrom<ITestStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                    message =>
                    {
                        ITestFailed failed = Assert.IsAssignableFrom<ITestFailed>(message);
                        Assert.Equal(typeof(DivideByZeroException).FullName, failed.ExceptionType);
                    },
                    message => Assert.IsAssignableFrom<ITestFinished>(message)
                );
            }

            class ThrowingCtorClassUnderTest : IDisposable
            {
                public ThrowingCtorClassUnderTest()
                {
                    throw new DivideByZeroException();
                }

                [Fact]
                public void NonSkippedMethod()
                {
                    throw new InvalidFilterCriteriaException();
                }

                public void Dispose()
                {
                    throw new NotImplementedException();
                }
            }

            [Fact]
            public void ThrowingDispose_SuccessfulTest()
            {
                var testCase = TestableXunitTestCase.Create(typeof(ThrowingDisposeClassUnderTest), "PassingTest");

                testCase.RunTests();

                Assert.Collection(testCase.Messages,
                    message => Assert.IsAssignableFrom<ITestStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                    message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                    message =>
                    {
                        ITestFailed failed = Assert.IsAssignableFrom<ITestFailed>(message);
                        Assert.Equal(typeof(NotImplementedException).FullName, failed.ExceptionType);
                    },
                    message => Assert.IsAssignableFrom<ITestFinished>(message)
                );
            }

            [Fact]
            public void ThrowingDispose_FailingTest()
            {
                var testCase = TestableXunitTestCase.Create(typeof(ThrowingDisposeClassUnderTest), "FailingTest");

                testCase.RunTests();

                Assert.Collection(testCase.Messages,
                    message => Assert.IsAssignableFrom<ITestStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                    message => Assert.IsAssignableFrom<ITestClassDisposeStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassDisposeFinished>(message),
                    message =>
                    {
                        ITestFailed failed = Assert.IsAssignableFrom<ITestFailed>(message);
                        Assert.Equal(typeof(AggregateException).FullName, failed.ExceptionType);
                        Assert.Contains(typeof(InvalidFilterCriteriaException).FullName, failed.Message);
                        Assert.Contains(typeof(NotImplementedException).FullName, failed.Message);
                    },
                    message => Assert.IsAssignableFrom<ITestFinished>(message)
                );
            }

            class ThrowingDisposeClassUnderTest : IDisposable
            {
                [Fact]
                public void PassingTest() { }

                [Fact]
                public void FailingTest()
                {
                    throw new InvalidFilterCriteriaException();
                }

                public void Dispose()
                {
                    throw new NotImplementedException();
                }
            }
        }

        public class BeforeAfter_OnTestMethod
        {
            [Fact]
            public void Skipped()
            {
                var testCase = TestableXunitTestCase.Create(typeof(SkippedClassUnderTest), "SkippedMethod");

                testCase.RunTests();

                Assert.Collection(testCase.Messages,
                    message => Assert.IsAssignableFrom<ITestStarting>(message),
                    message => Assert.IsAssignableFrom<ITestSkipped>(message),
                    message => Assert.IsAssignableFrom<ITestFinished>(message)
                );
            }

            class SkippedClassUnderTest
            {
                [SpyBeforeAfterTest]
                [Fact(Skip = "Please don't run me")]
                public void SkippedMethod()
                {
                    throw new NotImplementedException();
                }
            }

            public class SingleBeforeAfterAttribute
            {
                [Fact]
                public void Success()
                {
                    var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "PassingTestMethod");

                    testCase.RunTests();

                    Assert.Collection(testCase.Messages,
                        message => Assert.IsAssignableFrom<ITestStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                        message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                        message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                        message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                        message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                        message => Assert.IsAssignableFrom<ITestPassed>(message),
                        message => Assert.IsAssignableFrom<ITestFinished>(message)
                    );
                }

                [Fact]
                public void BeforeThrows()
                {
                    var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "ThrowInBefore");

                    testCase.RunTests();

                    Assert.Collection(testCase.Messages,
                        message => Assert.IsAssignableFrom<ITestStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                        message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                        message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                        message =>
                        {
                            var failed = Assert.IsAssignableFrom<ITestFailed>(message);
                            Assert.Equal(typeof(SpyBeforeAfterTest.BeforeException).FullName, failed.ExceptionType);
                        },
                        message => Assert.IsAssignableFrom<ITestFinished>(message)
                    );
                }

                [Fact]
                public void AfterThrows()
                {
                    var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "ThrowInAfter");

                    testCase.RunTests();

                    Assert.Collection(testCase.Messages,
                        message => Assert.IsAssignableFrom<ITestStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                        message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                        message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                        message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                        message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                        message =>
                        {
                            var failed = Assert.IsAssignableFrom<ITestFailed>(message);
                            Assert.Equal(typeof(SpyBeforeAfterTest.AfterException).FullName, failed.ExceptionType);
                        },
                        message => Assert.IsAssignableFrom<ITestFinished>(message)
                    );
                }

                [Fact]
                public void AfterAndTestMethodThrows()
                {
                    var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "ThrowInAfterAndTest");

                    testCase.RunTests();

                    Assert.Collection(testCase.Messages,
                        message => Assert.IsAssignableFrom<ITestStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                        message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                        message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                        message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                        message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                        message =>
                        {
                            var failed = Assert.IsAssignableFrom<ITestFailed>(message);
                            Assert.Equal(typeof(AggregateException).FullName, failed.ExceptionType);
                            Assert.Contains(typeof(NotImplementedException).FullName, failed.Message);
                            Assert.Contains(typeof(SpyBeforeAfterTest.AfterException).FullName, failed.Message);
                        },
                        message => Assert.IsAssignableFrom<ITestFinished>(message)
                    );
                }

                class ClassUnderTest
                {
                    [Fact]
                    [SpyBeforeAfterTest]
                    public void PassingTestMethod() { }

                    [Fact]
                    [SpyBeforeAfterTest(ThrowInBefore = true)]
                    public void ThrowInBefore()
                    {
                        throw new NotImplementedException();
                    }

                    [Fact]
                    [SpyBeforeAfterTest(ThrowInAfter = true)]
                    public void ThrowInAfter()
                    {
                    }

                    [Fact]
                    [SpyBeforeAfterTest(ThrowInAfter = true)]
                    public void ThrowInAfterAndTest()
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            public class MultipleBeforeAfterAttributes
            {
                [Fact]
                public void Success()
                {
                    var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "PassingTestMethod");

                    testCase.RunTests();

                    Assert.Collection(testCase.Messages,
                        message => Assert.IsAssignableFrom<ITestStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                        message => Assert.Equal("DummyBeforeAfterTest", Assert.IsAssignableFrom<IBeforeTestStarting>(message).AttributeName),
                        message => Assert.Equal("DummyBeforeAfterTest", Assert.IsAssignableFrom<IBeforeTestFinished>(message).AttributeName),
                        message => Assert.Equal("SpyBeforeAfterTest", Assert.IsAssignableFrom<IBeforeTestStarting>(message).AttributeName),
                        message => Assert.Equal("SpyBeforeAfterTest", Assert.IsAssignableFrom<IBeforeTestFinished>(message).AttributeName),
                        message => Assert.Equal("SpyBeforeAfterTest", Assert.IsAssignableFrom<IAfterTestStarting>(message).AttributeName),
                        message => Assert.Equal("SpyBeforeAfterTest", Assert.IsAssignableFrom<IAfterTestFinished>(message).AttributeName),
                        message => Assert.Equal("DummyBeforeAfterTest", Assert.IsAssignableFrom<IAfterTestStarting>(message).AttributeName),
                        message => Assert.Equal("DummyBeforeAfterTest", Assert.IsAssignableFrom<IAfterTestFinished>(message).AttributeName),
                        message => Assert.IsAssignableFrom<ITestPassed>(message),
                        message => Assert.IsAssignableFrom<ITestFinished>(message)
                    );
                }

                [Fact]
                public void EarlyFailurePreventsLaterBeforeAfter()
                {
                    var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "ThrowInBefore");

                    testCase.RunTests();

                    Assert.Collection(testCase.Messages,
                        message => Assert.IsAssignableFrom<ITestStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                        message => Assert.Equal("DummyBeforeAfterTest", Assert.IsAssignableFrom<IBeforeTestStarting>(message).AttributeName),
                        message => Assert.Equal("DummyBeforeAfterTest", Assert.IsAssignableFrom<IBeforeTestFinished>(message).AttributeName),
                        message => Assert.IsAssignableFrom<ITestFailed>(message),
                        message => Assert.IsAssignableFrom<ITestFinished>(message)
                    );
                }

                [Fact]
                public void EarlyAfterFailureDoesNotPreventLaterAfterRun()
                {
                    var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "ThrowInAfter");

                    testCase.RunTests();

                    Assert.Collection(testCase.Messages,
                        message => Assert.IsAssignableFrom<ITestStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                        message => Assert.Equal("DummyBeforeAfterTest", Assert.IsAssignableFrom<IBeforeTestStarting>(message).AttributeName),
                        message => Assert.Equal("DummyBeforeAfterTest", Assert.IsAssignableFrom<IBeforeTestFinished>(message).AttributeName),
                        message => Assert.Equal("SpyBeforeAfterTest", Assert.IsAssignableFrom<IBeforeTestStarting>(message).AttributeName),
                        message => Assert.Equal("SpyBeforeAfterTest", Assert.IsAssignableFrom<IBeforeTestFinished>(message).AttributeName),
                        message => Assert.Equal("SpyBeforeAfterTest", Assert.IsAssignableFrom<IAfterTestStarting>(message).AttributeName),
                        message => Assert.Equal("SpyBeforeAfterTest", Assert.IsAssignableFrom<IAfterTestFinished>(message).AttributeName),
                        message => Assert.Equal("DummyBeforeAfterTest", Assert.IsAssignableFrom<IAfterTestStarting>(message).AttributeName),
                        message => Assert.Equal("DummyBeforeAfterTest", Assert.IsAssignableFrom<IAfterTestFinished>(message).AttributeName),
                        message =>
                        {
                            var failed = Assert.IsAssignableFrom<ITestFailed>(message);
                            Assert.Equal(typeof(SpyBeforeAfterTest.AfterException).FullName, failed.ExceptionType);
                        },
                        message => Assert.IsAssignableFrom<ITestFinished>(message)
                    );
                }

                class ClassUnderTest
                {
                    [Fact]
                    [DummyBeforeAfterTest]
                    [SpyBeforeAfterTest]
                    public void PassingTestMethod() { }

                    [Fact]
                    [DummyBeforeAfterTest(ThrowInBefore = true)]
                    [SpyBeforeAfterTest]
                    public void ThrowInBefore()
                    {
                        throw new NotImplementedException();
                    }

                    [Fact]
                    [DummyBeforeAfterTest]
                    [SpyBeforeAfterTest(ThrowInAfter = true)]
                    public void ThrowInAfter()
                    {
                    }
                }
            }
        }

        public class BeforeAfter_OnTestClass
        {
            public class Skipped
            {
                [Fact]
                public void SkippedMethod()
                {
                    var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "SkippedMethod");

                    testCase.RunTests();

                    Assert.Collection(testCase.Messages,
                        message => Assert.IsAssignableFrom<ITestStarting>(message),
                        message => Assert.IsAssignableFrom<ITestSkipped>(message),
                        message => Assert.IsAssignableFrom<ITestFinished>(message)
                    );
                }

                [Fact]
                public void NonSkippedMethod()
                {
                    var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "NonSkippedMethod");

                    testCase.RunTests();

                    Assert.Collection(testCase.Messages,
                        message => Assert.IsAssignableFrom<ITestStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                        message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                        message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                        message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                        message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                        message => Assert.IsAssignableFrom<ITestPassed>(message),
                        message => Assert.IsAssignableFrom<ITestFinished>(message)
                    );
                }

                [Fact]
                public void BeforeAfterOnBothClassAndMethod()
                {
                    var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "MethodWithBeforeAfter");

                    testCase.RunTests();

                    Assert.Collection(testCase.Messages,
                        message => Assert.IsAssignableFrom<ITestStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                        message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                        message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                        message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
                        message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
                        message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                        message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                        message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
                        message => Assert.IsAssignableFrom<IAfterTestFinished>(message),
                        message => Assert.IsAssignableFrom<ITestPassed>(message),
                        message => Assert.IsAssignableFrom<ITestFinished>(message)
                    );
                }

                [Fact]
                public void ClassBeforeAfterRunsBeforeMethodBeforeAfter()
                {
                    var testCase = TestableXunitTestCase.Create(typeof(ClassUnderTest), "MethodWithDummyBeforeAfter");

                    testCase.RunTests();

                    Assert.Collection(testCase.Messages,
                        message => Assert.IsAssignableFrom<ITestStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                        message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                        message => Assert.Equal("DummyBeforeAfterTest", Assert.IsAssignableFrom<IBeforeTestStarting>(message).AttributeName),
                        message => Assert.Equal("DummyBeforeAfterTest", Assert.IsAssignableFrom<IBeforeTestFinished>(message).AttributeName),
                        message => Assert.Equal("SpyBeforeAfterTest", Assert.IsAssignableFrom<IBeforeTestStarting>(message).AttributeName),
                        message => Assert.Equal("SpyBeforeAfterTest", Assert.IsAssignableFrom<IBeforeTestFinished>(message).AttributeName),
                        message => Assert.Equal("SpyBeforeAfterTest", Assert.IsAssignableFrom<IAfterTestStarting>(message).AttributeName),
                        message => Assert.Equal("SpyBeforeAfterTest", Assert.IsAssignableFrom<IAfterTestFinished>(message).AttributeName),
                        message => Assert.Equal("DummyBeforeAfterTest", Assert.IsAssignableFrom<IAfterTestStarting>(message).AttributeName),
                        message => Assert.Equal("DummyBeforeAfterTest", Assert.IsAssignableFrom<IAfterTestFinished>(message).AttributeName),
                        message => Assert.IsAssignableFrom<ITestPassed>(message),
                        message => Assert.IsAssignableFrom<ITestFinished>(message)
                    );
                }

                [SpyBeforeAfterTest]
                class ClassUnderTest
                {
                    [Fact(Skip = "Please don't run me")]
                    public void SkippedMethod()
                    {
                        throw new NotImplementedException();
                    }

                    [Fact]
                    public void NonSkippedMethod()
                    {
                    }

                    [Fact]
                    [SpyBeforeAfterTest]
                    public void MethodWithBeforeAfter()
                    {
                    }

                    [Fact]
                    [DummyBeforeAfterTest]
                    public void MethodWithDummyBeforeAfter()
                    {
                    }
                }
            }
        }

        public class NonReflectionDiscovery
        {
            [Fact]
            public void CanRunTestThatWasDiscoveredWithoutReflection()
            {
                var typeUnderTest = typeof(ClassUnderTest);
                var methodUnderTest = typeUnderTest.GetMethod("TestMethod");
                var factAttributeUnderTest = CustomAttributeData.GetCustomAttributes(methodUnderTest).Single(a => a.AttributeType == typeof(FactAttribute));

                var assembly = new AssemblyWrapper(Reflector.Wrap(typeUnderTest.Assembly));
                var type = new TypeWrapper(Reflector.Wrap(typeUnderTest));
                var method = new MethodWrapper(Reflector.Wrap(methodUnderTest));
                var attribute = new AttributeWrapper(Reflector.Wrap(factAttributeUnderTest));
                var testCase = TestableXunitTestCase.Create(assembly, type, method, attribute);

                testCase.RunTests();

                Assert.Collection(testCase.Messages,
                    message => Assert.IsAssignableFrom<ITestStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
                    message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
                    message =>
                    {
                        var failed = Assert.IsAssignableFrom<ITestFailed>(message);
                        Assert.Equal(typeof(TrueException).FullName, failed.ExceptionType);
                    },
                    message => Assert.IsAssignableFrom<ITestFinished>(message)
                );
            }

            class ClassUnderTest
            {
                [Fact]
                public void TestMethod()
                {
                    Assert.True(false);
                }
            }
        }
    }

    public class Serialization
    {
        [Fact]
        public void CanRoundTrip_PublicClass_PublicTestMethod()
        {
            var serializer = new BinaryFormatter();
            var testCase = Create(typeof(Serialization), "CanRoundTrip_PublicClass_PublicTestMethod");
            var memoryStream = new MemoryStream();

            serializer.Serialize(memoryStream, testCase);
            memoryStream.Position = 0;

            Assert.DoesNotThrow(() => serializer.Deserialize(memoryStream));
        }

        [Fact]
        void CanRoundTrip_PublicClass_PrivateTestMethod()
        {
            var serializer = new BinaryFormatter();
            var testCase = Create(typeof(Serialization), "CanRoundTrip_PublicClass_PrivateTestMethod");
            var memoryStream = new MemoryStream();

            serializer.Serialize(memoryStream, testCase);
            memoryStream.Position = 0;

            Assert.DoesNotThrow(() => serializer.Deserialize(memoryStream));
        }

        [Fact]
        public void CannotRoundTrip_PrivateClass()
        {
            var serializer = new BinaryFormatter();
            var testCase = Create(typeof(PrivateClass), "TestMethod");
            var memoryStream = new MemoryStream();

            serializer.Serialize(memoryStream, testCase);
            memoryStream.Position = 0;

            Assert.DoesNotThrow(() => serializer.Deserialize(memoryStream));
        }

        class PrivateClass
        {
            [Fact]
            public void TestMethod()
            {
                Assert.True(false);
            }
        }
    }

    public class UniqueID
    {
        [Fact(Skip = "UniqueID stability is occasionally failing")]
        public void UniqueIDIsStable_NoArguments()
        {
            var value = Create(typeof(ClassUnderTest), "TestMethod").UniqueID;

            for (int x = 0; x < 5; x++)
                Assert.Equal(value, Create(typeof(ClassUnderTest), "TestMethod").UniqueID);
        }

        [Fact(Skip = "UniqueID stability is occasionally failing")]
        public void UniqueIDIsStable_WithArguments()
        {
            var value42 = Create(typeof(ClassUnderTest), "TestMethod", 42).UniqueID;
            var valueHelloWorld = Create(typeof(ClassUnderTest), "TestMethod", "Hello, world!").UniqueID;
            var valueNull = Create(typeof(ClassUnderTest), "TestMethod", (string)null).UniqueID;

            Assert.NotEqual(value42, valueHelloWorld);
            Assert.NotEqual(value42, valueNull);

            for (int x = 0; x < 5; x++)
            {
                Assert.Equal(value42, Create(typeof(ClassUnderTest), "TestMethod", 42).UniqueID);
                Assert.Equal(valueHelloWorld, Create(typeof(ClassUnderTest), "TestMethod", "Hello, world!").UniqueID);
                Assert.Equal(valueNull, Create(typeof(ClassUnderTest), "TestMethod", (string)null).UniqueID);
            }
        }

        class ClassUnderTest
        {
            [Fact]
            public void TestMethod() { }
        }
    }

    static XunitTestCase Create(Type typeUnderTest, string methodName, params object[] arguments)
    {
        var testCollection = new XunitTestCollection();
        var methodUnderTest = typeUnderTest.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        var assembly = Reflector.Wrap(typeUnderTest.Assembly);
        var type = Reflector.Wrap(typeUnderTest);
        var method = Reflector.Wrap(methodUnderTest);
        var fact = Reflector.Wrap(CustomAttributeData.GetCustomAttributes(methodUnderTest)
                                                     .Single(cad => cad.AttributeType == typeof(FactAttribute)));

        return new XunitTestCase(testCollection, assembly, type, method, fact, arguments.Length == 0 ? null : arguments);
    }

    class DummyBeforeAfterTest : SpyBeforeAfterTest { }

    class SpyMessage : IMessageSinkMessage
    {
        public void Dispose() { }
    }

    public class TestableXunitTestCase : XunitTestCase
    {
        Action<IMessageBus> callback;
        SpyMessageBus<IMessageSinkMessage> bus = new SpyMessageBus<IMessageSinkMessage>();

        TestableXunitTestCase(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, Action<IMessageBus> callback = null)
            : base(new XunitTestCollection(), assembly, type, method, factAttribute)
        {
            this.callback = callback;
        }

        public List<IMessageSinkMessage> Messages
        {
            get { return bus.Messages; }
        }

        public static TestableXunitTestCase Create(Action<IMessageBus> callback = null)
        {
            var fact = Mocks.FactAttribute();
            var method = Mocks.MethodInfo();
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

            return new TestableXunitTestCase(assmInfo, type, method, fact, callback ?? (sink => sink.QueueMessage(new SpyMessage())));
        }

        public static TestableXunitTestCase Create(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute)
        {
            return new TestableXunitTestCase(assembly, type, method, factAttribute);
        }

        public static TestableXunitTestCase Create(Type typeUnderTest, string methodName)
        {
            var methodUnderTest = typeUnderTest.GetMethod(methodName);
            var assembly = Reflector.Wrap(typeUnderTest.Assembly);
            var type = Reflector.Wrap(typeUnderTest);
            var method = Reflector.Wrap(methodUnderTest);
            var fact = Reflector.Wrap(CustomAttributeData.GetCustomAttributes(methodUnderTest)
                                                         .Single(cad => cad.AttributeType == typeof(FactAttribute)));
            return new TestableXunitTestCase(assembly, type, method, fact);
        }

        protected override IEnumerable<BeforeAfterTestAttribute> GetBeforeAfterAttributes(Type classUnderTest, MethodInfo methodUnderTest)
        {
            // Order by name so they are discovered in a predictable order, for these tests
            return base.GetBeforeAfterAttributes(classUnderTest, methodUnderTest).OrderBy(a => a.GetType().Name);
        }

        public bool Run(IMessageBus messageBus)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            Run(messageBus, new object[0], new ExceptionAggregator(), cancellationTokenSource);
            return cancellationTokenSource.IsCancellationRequested;
        }

        public void RunTests()
        {
            RunTests(bus, new object[0], new ExceptionAggregator(), new CancellationTokenSource());
        }

        protected override void RunTests(IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            if (callback == null)
                base.RunTests(messageBus, constructorArguments, aggregator, cancellationTokenSource);
            else
                callback(messageBus);
        }
    }
}