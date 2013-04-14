using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
        var fact = Mocks.FactAttribute();
        var method = Mocks.MethodInfo();
        var type = Mocks.TypeInfo(methods: new[] { method });
        var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

        var testCase = new XunitTestCase(assmInfo, type, method, fact);

        Assert.Equal("MockType.MockMethod", testCase.DisplayName);
        Assert.Null(testCase.SkipReason);
        Assert.Empty(testCase.Traits);
    }

    [Fact]
    public void SkipReason()
    {
        var fact = Mocks.FactAttribute(skip: "Skip Reason");
        var method = Mocks.MethodInfo();
        var type = Mocks.TypeInfo(methods: new[] { method });
        var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

        var testCase = new XunitTestCase(assmInfo, type, method, fact);

        Assert.Equal("Skip Reason", testCase.SkipReason);
    }

    [Fact]
    public void Traits()
    {
        var fact = Mocks.FactAttribute();
        var trait1 = Mocks.TraitAttribute("Trait1", "Value1");
        var trait2 = Mocks.TraitAttribute("Trait2", "Value2");
        var method = Mocks.MethodInfo(attributes: new[] { trait1, trait2 });
        var type = Mocks.TypeInfo(methods: new[] { method });
        var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

        var testCase = new XunitTestCase(assmInfo, type, method, fact);

        Assert.Equal("Value1", testCase.Traits["Trait1"]);
        Assert.Equal("Value2", testCase.Traits["Trait2"]);
    }

    public class DisplayName
    {
        [Fact]
        public void CustomDisplayName()
        {
            var fact = Mocks.FactAttribute(displayName: "Custom Display Name");
            var method = Mocks.MethodInfo();
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

            var testCase = new XunitTestCase(assmInfo, type, method, fact);

            Assert.Equal("Custom Display Name", testCase.DisplayName);
        }

        [Fact]
        public void CorrectNumberOfTestArguments()
        {
            var fact = Mocks.FactAttribute();
            var param1 = Mocks.ParameterInfo("p1");
            var param2 = Mocks.ParameterInfo("p2");
            var param3 = Mocks.ParameterInfo("p3");
            var method = Mocks.MethodInfo(parameters: new[] { param1, param2, param3 });
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });
            var arguments = new object[] { 42, "Hello, world!", 'A' };

            var testCase = new XunitTestCase(assmInfo, type, method, fact, arguments);

            Assert.Equal("MockType.MockMethod(p1: 42, p2: \"Hello, world!\", p3: 'A')", testCase.DisplayName);
        }

        [Fact]
        public void NotEnoughTestArguments()
        {
            var fact = Mocks.FactAttribute();
            var param = Mocks.ParameterInfo("p1");
            var method = Mocks.MethodInfo(parameters: new[] { param });
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

            var testCase = new XunitTestCase(assmInfo, type, method, fact, arguments: new object[0]);

            Assert.Equal("MockType.MockMethod(p1: ???)", testCase.DisplayName);
        }

        [Fact]
        public void TooManyTestArguments()
        {
            var fact = Mocks.FactAttribute();
            var param = Mocks.ParameterInfo("p1");
            var method = Mocks.MethodInfo(parameters: new[] { param });
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });
            var arguments = new object[] { 42, 21.12 };

            var testCase = new XunitTestCase(assmInfo, type, method, fact, arguments);

            Assert.Equal("MockType.MockMethod(p1: 42, ???: 21.12)", testCase.DisplayName);
        }
    }

    public class Run
    {
        [Fact]
        public void IssuesTestCaseMessagesAndCallsRunTests()
        {
            var testCase = TestableXunitTestCase.Create();
            var sink = new SpyMessageSink<ITestCaseFinished>();

            testCase.Run(sink);
            sink.Finished.WaitOne();

            Assert.Collection(sink.Messages,
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
            var testCase = TestableXunitTestCase.Create(msgSink =>
            {
                msgSink.OnMessage(Substitute.For<ITestResultMessage>());
                msgSink.OnMessage(Substitute.For<ITestPassed>());
                msgSink.OnMessage(Substitute.For<ITestFailed>());
                msgSink.OnMessage(Substitute.For<ITestSkipped>());
            });
            var sink = new SpyMessageSink<ITestCaseFinished>();

            testCase.Run(sink);
            sink.Finished.WaitOne();

            var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(sink.Messages.Last());
            Assert.Equal(4, testCaseFinished.TestsRun);
        }

        [Fact]
        public void CountsTestsFailed()
        {
            var testCase = TestableXunitTestCase.Create(msgSink =>
            {
                msgSink.OnMessage(new TestFailed());
                msgSink.OnMessage(new TestFailed());
            });
            var sink = new SpyMessageSink<ITestCaseFinished>();

            testCase.Run(sink);
            sink.Finished.WaitOne();

            var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(sink.Messages.Last());
            Assert.Equal(2, testCaseFinished.TestsFailed);
        }

        [Fact]
        public void CountsTestsSkipped()
        {
            var testCase = TestableXunitTestCase.Create(msgSink =>
            {
                msgSink.OnMessage(new TestSkipped());
                msgSink.OnMessage(new TestSkipped());
            });
            var sink = new SpyMessageSink<ITestCaseFinished>();

            testCase.Run(sink);
            sink.Finished.WaitOne();

            var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(sink.Messages.Last());
            Assert.Equal(2, testCaseFinished.TestsSkipped);
        }

        [Fact]
        public void AggregatesTestRunTime()
        {
            var testCase = TestableXunitTestCase.Create(msgSink =>
            {
                msgSink.OnMessage(new TestPassed { ExecutionTime = 1.2M });
                msgSink.OnMessage(new TestFailed { ExecutionTime = 2.3M });
            });
            var sink = new SpyMessageSink<ITestCaseFinished>();

            testCase.Run(sink);
            sink.Finished.WaitOne();

            var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(sink.Messages.Last());
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

    class DummyBeforeAfterTest : SpyBeforeAfterTest { }

    class SpyMessage : ITestMessage { }

    public class TestableXunitTestCase : XunitTestCase
    {
        Action<IMessageSink> callback;
        SpyMessageSink<ITestMessage> sink = new SpyMessageSink<ITestMessage>();

        TestableXunitTestCase(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, Action<IMessageSink> callback = null)
            : base(assembly, type, method, factAttribute)
        {
            this.callback = callback;
        }

        public List<ITestMessage> Messages
        {
            get { return sink.Messages; }
        }

        public static TestableXunitTestCase Create(Action<IMessageSink> callback = null)
        {
            var fact = Mocks.FactAttribute();
            var method = Mocks.MethodInfo();
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

            return new TestableXunitTestCase(assmInfo, type, method, fact, callback ?? (sink => sink.OnMessage(new SpyMessage())));
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

        public void RunTests()
        {
            RunTests(sink);
        }

        protected override bool RunTests(IMessageSink messageSink)
        {
            if (callback == null)
                return base.RunTests(messageSink);

            callback(messageSink);
            return true;
        }
    }
}