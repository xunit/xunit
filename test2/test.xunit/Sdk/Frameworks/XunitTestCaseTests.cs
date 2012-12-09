using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        var fact = new MockFactAttribute();
        var method = new MockMethodInfo();
        var type = new MockTypeInfo(methods: new[] { method });
        var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });

        var testCase = new XunitTestCase(assmInfo.Object, type.Object, method, fact);

        Assert.Equal("MockType.MockMethod", testCase.DisplayName);
        Assert.Null(testCase.SkipReason);
        Assert.Empty(testCase.Traits);
    }

    [Fact]
    public void SkipReason()
    {
        var fact = new MockFactAttribute(skip: "Skip Reason");
        var method = new MockMethodInfo();
        var type = new MockTypeInfo(methods: new[] { method });
        var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });

        var testCase = new XunitTestCase(assmInfo.Object, type.Object, method, fact);

        Assert.Equal("Skip Reason", testCase.SkipReason);
    }

    [Fact]
    public void Traits()
    {
        var fact = new MockFactAttribute();
        var trait1 = new MockTraitAttribute("Trait1", "Value1");
        var trait2 = new MockTraitAttribute("Trait2", "Value2");
        var method = new MockMethodInfo(attributes: new[] { trait1, trait2 });
        var type = new MockTypeInfo(methods: new[] { method });
        var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });

        var testCase = new XunitTestCase(assmInfo.Object, type.Object, method, fact);

        Assert.Equal("Value1", testCase.Traits["Trait1"]);
        Assert.Equal("Value2", testCase.Traits["Trait2"]);
    }

    public class DisplayName
    {
        [Fact]
        public void CustomDisplayName()
        {
            var fact = new MockFactAttribute(displayName: "Custom Display Name");
            var method = new MockMethodInfo();
            var type = new MockTypeInfo(methods: new[] { method });
            var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });

            var testCase = new XunitTestCase(assmInfo.Object, type.Object, method, fact);

            Assert.Equal("Custom Display Name", testCase.DisplayName);
        }

        [Fact]
        public void CorrectNumberOfTestArguments()
        {
            var fact = new MockFactAttribute();
            var param1 = new MockParameterInfo("p1");
            var param2 = new MockParameterInfo("p2");
            var param3 = new MockParameterInfo("p3");
            var method = new MockMethodInfo(parameters: new[] { param1.Object, param2.Object, param3.Object });
            var type = new MockTypeInfo(methods: new[] { method });
            var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });
            var arguments = new object[] { 42, "Hello, world!", 'A' };

            var testCase = new XunitTestCase(assmInfo.Object, type.Object, method, fact, arguments);

            Assert.Equal("MockType.MockMethod(p1: 42, p2: \"Hello, world!\", p3: 'A')", testCase.DisplayName);
        }

        [Fact]
        public void NotEnoughTestArguments()
        {
            var fact = new MockFactAttribute();
            var param = new MockParameterInfo("p1");
            var method = new MockMethodInfo(parameters: new[] { param.Object });
            var type = new MockTypeInfo(methods: new[] { method });
            var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });

            var testCase = new XunitTestCase(assmInfo.Object, type.Object, method, fact, arguments: new object[0]);

            Assert.Equal("MockType.MockMethod(p1: ???)", testCase.DisplayName);
        }

        [Fact]
        public void TooManyTestArguments()
        {
            var fact = new MockFactAttribute();
            var param = new MockParameterInfo("p1");
            var method = new MockMethodInfo(parameters: new[] { param.Object });
            var type = new MockTypeInfo(methods: new[] { method });
            var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });
            var arguments = new object[] { 42, 21.12 };

            var testCase = new XunitTestCase(assmInfo.Object, type.Object, method, fact, arguments);

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

            CollectionAssert.Collection(sink.Messages,
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
                    Assert.Same(testCase.Assembly, testCaseFinished.Assembly);
                    Assert.Equal(0, testCaseFinished.TestsRun);
                    Assert.Equal(0, testCaseFinished.TestsFailed);
                    Assert.Equal(0, testCaseFinished.TestsSkipped);
                    Assert.Equal(0M, testCaseFinished.ExecutionTime);
                }
            );
        }

        [Fact]
        public void CountsTestsFinished()
        {
            var testCase = TestableXunitTestCase.Create(msgSink =>
            {
                msgSink.OnMessage(new TestFinished());
                msgSink.OnMessage(new TestFinished());
            });
            var sink = new SpyMessageSink<ITestCaseFinished>();

            testCase.Run(sink);
            sink.Finished.WaitOne();

            var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(sink.Messages.Last());
            Assert.Equal(2, testCaseFinished.TestsRun);
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
                msgSink.OnMessage(new TestFinished { ExecutionTime = 1.2M });
                msgSink.OnMessage(new TestFinished { ExecutionTime = 2.3M });
            });
            var sink = new SpyMessageSink<ITestCaseFinished>();

            testCase.Run(sink);
            sink.Finished.WaitOne();

            var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(sink.Messages.Last());
            Assert.Equal(3.5M, testCaseFinished.ExecutionTime);
        }

        [Fact]
        public void ConstructorAndDisposeAreCalled()
        {
            ConstructionDisposeSpy.Messages.Clear();
            var testCase = TestableXunitTestCase.Create(typeof(ConstructionDisposeSpy), "TestMethod");

            testCase.Run(new SpyMessageSink<ITestCaseFinished>());

            CollectionAssert.Collection(ConstructionDisposeSpy.Messages,
                message => Assert.Equal("Constructor", message),
                message => Assert.Equal("Test Method", message),
                message => Assert.Equal("Dispose", message)
            );
        }

        class ConstructionDisposeSpy : IDisposable
        {
            public static List<string> Messages = new List<string>();

            public ConstructionDisposeSpy()
            {
                Messages.Add("Constructor");
            }

            public void Dispose()
            {
                Messages.Add("Dispose");
            }

            [Fact2]
            public void TestMethod()
            {
                Messages.Add("Test Method");
            }
        }

        [Fact]
        public void ThrowingConstructorCausesTestFailure()
        {
            var testCase = TestableXunitTestCase.Create(typeof(ThrowingConstructor), "TestMethod");
            var sink = new SpyMessageSink<ITestCaseFinished>();

            testCase.Run(sink);

            CollectionAssert.Collection(sink.Messages,
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message =>
                {
                    ITestFailed failedMessage = Assert.IsAssignableFrom<ITestFailed>(message);
                    Assert.IsType<ArgumentException>(failedMessage.Exception);
                },
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message)
            );
        }

        class ThrowingConstructor
        {
            public ThrowingConstructor()
            {
                throw new ArgumentException();
            }

            [Fact]
            public void TestMethod()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void ThrowingTestMethodCausesTestFailure()
        {
            var testCase = TestableXunitTestCase.Create(typeof(ThrowingTestMethod), "TestMethod");
            var sink = new SpyMessageSink<ITestCaseFinished>();

            testCase.Run(sink);

            CollectionAssert.Collection(sink.Messages,
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message =>
                {
                    ITestFailed failedMessage = Assert.IsAssignableFrom<ITestFailed>(message);
                    Assert.IsType<NotImplementedException>(failedMessage.Exception);
                },
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message)
            );
        }

        class ThrowingTestMethod
        {
            [Fact]
            public void TestMethod()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void ThrowingDisposeCausesTestFailure()
        {
            var testCase = TestableXunitTestCase.Create(typeof(ThrowingDispose), "TestMethod");
            var sink = new SpyMessageSink<ITestCaseFinished>();

            testCase.Run(sink);

            CollectionAssert.Collection(sink.Messages,
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message =>
                {
                    ITestFailed failedMessage = Assert.IsAssignableFrom<ITestFailed>(message);
                    Assert.IsType<ArgumentNullException>(failedMessage.Exception);
                },
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message)
            );
        }

        class ThrowingDispose : IDisposable
        {
            [Fact]
            public void TestMethod()
            {
            }

            public void Dispose()
            {
                throw new ArgumentNullException();
            }
        }

        [Fact]
        public void ThrowingTestMethodAndThrowingDisposeCausesAggregateException()
        {
            var testCase = TestableXunitTestCase.Create(typeof(ThrowingTestAndDispose), "TestMethod");
            var sink = new SpyMessageSink<ITestCaseFinished>();

            testCase.Run(sink);

            CollectionAssert.Collection(sink.Messages,
                message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
                message => Assert.IsAssignableFrom<ITestStarting>(message),
                message =>
                {
                    ITestFailed failedMessage = Assert.IsAssignableFrom<ITestFailed>(message);
                    AggregateException aggEx = Assert.IsType<AggregateException>(failedMessage.Exception);
                    CollectionAssert.Collection(aggEx.InnerExceptions,
                        ex => Assert.IsType<NotImplementedException>(ex),
                        ex => Assert.IsType<ArgumentNullException>(ex)
                    );
                },
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message)
            );
        }

        class ThrowingTestAndDispose : IDisposable
        {
            [Fact]
            public void TestMethod()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new ArgumentNullException();
            }
        }
    }

    class SpyMessage : ITestMessage { }

    public class TestableXunitTestCase : XunitTestCase
    {
        Action<IMessageSink> callback;

        TestableXunitTestCase(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, Action<IMessageSink> callback = null)
            : base(assembly, type, method, factAttribute)
        {
            this.callback = callback;
        }

        public static TestableXunitTestCase Create(Action<IMessageSink> callback = null)
        {
            var fact = new MockFactAttribute();
            var method = new MockMethodInfo();
            var type = new MockTypeInfo(methods: new[] { method });
            var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });

            return new TestableXunitTestCase(assmInfo.Object, type.Object, method, fact, callback ?? (sink => sink.OnMessage(new SpyMessage())));
        }

        public static TestableXunitTestCase Create(Type typeUnderTest, string methodName)
        {
            var methodUnderTest = typeUnderTest.GetMethod(methodName);
            var assembly = Reflector2.Wrap(typeUnderTest.Assembly);
            var type = Reflector2.Wrap(typeUnderTest);
            var method = Reflector2.Wrap(methodUnderTest);
            var fact = Reflector2.Wrap(CustomAttributeData.GetCustomAttributes(methodUnderTest).Single());
            return new TestableXunitTestCase(assembly, type, method, fact);
        }

        protected override void RunTests(IMessageSink messageSink)
        {
            if (callback != null)
                callback(messageSink);
            else
                base.RunTests(messageSink);
        }
    }
}
