using System;
using System.Linq;
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
    }

    class SpyMessage : ITestMessage { }

    public class TestableXunitTestCase : XunitTestCase
    {
        Action<IMessageSink> callback;

        TestableXunitTestCase(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, Action<IMessageSink> callback = null)
            : base(assembly, type, method, factAttribute)
        {
            this.callback = callback ?? (sink => sink.OnMessage(new SpyMessage()));
        }

        public static TestableXunitTestCase Create(Action<IMessageSink> callback = null)
        {
            var fact = new MockFactAttribute();
            var method = new MockMethodInfo();
            var type = new MockTypeInfo(methods: new[] { method });
            var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });

            return new TestableXunitTestCase(assmInfo.Object, type.Object, method, fact, callback);
        }

        protected override void RunTests(IMessageSink messageSink)
        {
            callback(messageSink);
        }
    }
}
