using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class XunitTestClassRunnerTests
{
    [Fact]
    public static async void ClassCannotBeDecoratedWithICollectionFixture()
    {
        var testCase = Mocks.XunitTestCase<ClassWithCollectionFixture>("Passing");
        var runner = TestableXunitTestClassRunner.Create(testCase);

        await runner.RunAsync();

        Assert.IsType<TestClassException>(runner.RunTestMethodAsync_AggregatorResult);
        Assert.Equal("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead).", runner.RunTestMethodAsync_AggregatorResult.Message);
    }

    class ClassWithCollectionFixture : ICollectionFixture<object>
    {
        [Fact]
        public void Passing() { }
    }

    [Fact]
    public static async void TestClassCannotHaveMoreThanOneConstructor()
    {
        var testCase = Mocks.XunitTestCase<ClassWithTwoConstructors>("Passing");
        var runner = TestableXunitTestClassRunner.Create(testCase);

        await runner.RunAsync();

        Assert.IsType<TestClassException>(runner.RunTestMethodAsync_AggregatorResult);
        Assert.Equal("A test class may only define a single public constructor.", runner.RunTestMethodAsync_AggregatorResult.Message);
    }

    class ClassWithTwoConstructors
    {
        public ClassWithTwoConstructors() { }
        public ClassWithTwoConstructors(int x) { }

        [Fact]
        public void Passing() { }
    }

    [Fact]
    public static async void TestClassCanHavePublicAndPrivateConstructor()
    {
        var testCase = Mocks.XunitTestCase<ClassWithMixedConstructors>("Passing");
        var runner = TestableXunitTestClassRunner.Create(testCase);

        await runner.RunAsync();

        Assert.Null(runner.RunTestMethodAsync_AggregatorResult);
    }

    class ClassWithMixedConstructors
    {
        public ClassWithMixedConstructors() { }
        ClassWithMixedConstructors(int x) { }

        [Fact]
        public void Passing() { }
    }

    [Fact]
    public static async void TestClassCanHaveStaticConstructor()
    {
        var testCase = Mocks.XunitTestCase<ClassWithStaticConstructor>("Passing");
        var runner = TestableXunitTestClassRunner.Create(testCase);

        await runner.RunAsync();

        Assert.Null(runner.RunTestMethodAsync_AggregatorResult);
    }

    class ClassWithStaticConstructor
    {
        static ClassWithStaticConstructor() { }
        public ClassWithStaticConstructor() { }

        [Fact]
        public void Passing() { }
    }

    [Fact]
    public static async void CreatesFixturesFromClassAndCollection()
    {
        var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionUnderTest)), null);
        var testCase = Mocks.XunitTestCase<ClassUnderTest>("Passing", collection);
        var runner = TestableXunitTestClassRunner.Create(testCase);

        await runner.RunAsync();

        Assert.Collection(runner.ClassFixtureMappings.OrderBy(mapping => mapping.Key.Name),
            mapping => Assert.IsType<FixtureUnderTest>(mapping.Value),
            mapping => Assert.IsType<object>(mapping.Value)
        );
    }

    [Fact]
    public static async void DisposesFixtures()
    {
        var testCase = Mocks.XunitTestCase<ClassUnderTest>("Passing");
        var runner = TestableXunitTestClassRunner.Create(testCase);

        await runner.RunAsync();

        var fixtureUnderTest = runner.ClassFixtureMappings.Values.OfType<FixtureUnderTest>().Single();
        Assert.True(fixtureUnderTest.Disposed);
    }

    [Fact]
    public static async void DisposeAndAsyncLifetimeShouldBeCalledInTheRightOrder()
    {
        var testCase = Mocks.XunitTestCase<TestClassForFixtureAsyncLifetimeAndDisposableUnderTest>("Passing");
        var runner = TestableXunitTestClassRunner.Create(testCase);

        var runnerSessionTask = runner.RunAsync();

        await Task.Delay(500);

        var fixtureUnderTest = runner.ClassFixtureMappings.Values.OfType<FixtureAsyncLifetimeAndDisposableUnderTest>().Single();

        Assert.True(fixtureUnderTest.DisposeAsyncCalled);
        Assert.False(fixtureUnderTest.Disposed);

        fixtureUnderTest.DisposeAsyncSignaler.SetResult(true);

        await runnerSessionTask;

        Assert.True(fixtureUnderTest.Disposed);
    }

    class TestClassForFixtureAsyncLifetimeAndDisposableUnderTest : IClassFixture<FixtureAsyncLifetimeAndDisposableUnderTest>
    {
        [Fact]
        public void Passing() { }
    }

    class FixtureAsyncLifetimeAndDisposableUnderTest : IAsyncLifetime, IDisposable
    {
        public bool Disposed;

        public bool DisposeAsyncCalled;

        public TaskCompletionSource<bool> DisposeAsyncSignaler = new TaskCompletionSource<bool>();

        public void Dispose()
        {
            Disposed = true;
        }

        public Task InitializeAsync()
        {
            return Task.FromResult(true);
        }

        public async Task DisposeAsync()
        {
            DisposeAsyncCalled = true;

            await DisposeAsyncSignaler.Task;
        }
    }

    [Fact]
    public static async void MultiplePublicConstructorsOnClassFixture_ReturnsError()
    {
        var testCase = Mocks.XunitTestCase<TestClassWithMultiCtorClassFixture>("Passing");
        var runner = TestableXunitTestClassRunner.Create(testCase);

        await runner.RunAsync();

        var ex = Assert.IsType<TestClassException>(runner.RunTestMethodAsync_AggregatorResult);
        Assert.Equal("Class fixture type 'XunitTestClassRunnerTests+ClassFixtureWithMultipleConstructors' may only define a single public constructor.", ex.Message);
    }

    class ClassFixtureWithMultipleConstructors
    {
        public ClassFixtureWithMultipleConstructors() { }
        public ClassFixtureWithMultipleConstructors(int unused) { }
    }

    class TestClassWithMultiCtorClassFixture : IClassFixture<ClassFixtureWithMultipleConstructors>
    {
        [Fact]
        public void Passing() { }
    }

    [Fact]
    public static async void UnresolvedConstructorParameterOnClassFixture_ReturnsError()
    {
        var testCase = Mocks.XunitTestCase<TestClassWithClassFixtureWithDependency>("Passing");
        var runner = TestableXunitTestClassRunner.Create(testCase);

        await runner.RunAsync();

        var ex = Assert.IsType<TestClassException>(runner.RunTestMethodAsync_AggregatorResult);
        Assert.Equal("Class fixture type 'XunitTestClassRunnerTests+ClassFixtureWithCollectionFixtureDependency' had one or more unresolved constructor arguments: DependentCollectionFixture collectionFixture", ex.Message);
    }

    [Fact]
    public static async void CanInjectCollectionFixtureIntoClassFixture()
    {
        var testCase = Mocks.XunitTestCase<TestClassWithClassFixtureWithDependency>("Passing");
        var collectionFixture = new DependentCollectionFixture();
        var runner = TestableXunitTestClassRunner.Create(testCase, collectionFixture);

        await runner.RunAsync();

        Assert.Null(runner.RunTestMethodAsync_AggregatorResult);
        var classFixture = runner.ClassFixtureMappings.Values.OfType<ClassFixtureWithCollectionFixtureDependency>().Single();
        Assert.Same(collectionFixture, classFixture.CollectionFixture);
    }

    class DependentCollectionFixture { }

    class ClassFixtureWithCollectionFixtureDependency
    {
        public DependentCollectionFixture CollectionFixture;

        public ClassFixtureWithCollectionFixtureDependency(DependentCollectionFixture collectionFixture)
        {
            CollectionFixture = collectionFixture;
        }
    }

    class TestClassWithClassFixtureWithDependency : IClassFixture<ClassFixtureWithCollectionFixtureDependency>
    {
        [Fact]
        public void Passing() { }
    }

    [Fact]
    public static async void CanInjectMessageSinkIntoClassFixture()
    {
        var testCase = Mocks.XunitTestCase<TestClassWithClassFixtureWithMessageSinkDependency>("Passing");
        var runner = TestableXunitTestClassRunner.Create(testCase);

        await runner.RunAsync();

        Assert.Null(runner.RunTestMethodAsync_AggregatorResult);
        var classFixture = runner.ClassFixtureMappings.Values.OfType<ClassFixtureWithMessageSinkDependency>().Single();
        Assert.NotNull(classFixture.MessageSink);
        Assert.Same(runner.DiagnosticMessageSink, classFixture.MessageSink);
    }

    [Fact]
    public static async void CanLogSinkMessageFromClassFixture()
    {
        var testCase = Mocks.XunitTestCase<TestClassWithClassFixtureWithMessageSinkDependency>("Passing");
        var runner = TestableXunitTestClassRunner.Create(testCase);

        await runner.RunAsync();

        var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<IDiagnosticMessage>());
        Assert.Equal("ClassFixtureWithMessageSinkDependency constructor message", diagnosticMessage.Message);
    }

    class ClassFixtureWithMessageSinkDependency
    {
        public IMessageSink MessageSink;

        public ClassFixtureWithMessageSinkDependency(IMessageSink messageSink)
        {
            MessageSink = messageSink;
            MessageSink.OnMessage(new Xunit.Sdk.DiagnosticMessage("ClassFixtureWithMessageSinkDependency constructor message"));
        }
    }

    class TestClassWithClassFixtureWithMessageSinkDependency : IClassFixture<ClassFixtureWithMessageSinkDependency>
    {
        [Fact]
        public void Passing() { }
    }

    public class TestCaseOrderer
    {
        [Fact]
        public static async void UsesCustomTestOrderer()
        {
            var testCase = Mocks.XunitTestCase<ClassUnderTest>("Passing");
            var runner = TestableXunitTestClassRunner.Create(testCase);

            await runner.RunAsync();

            Assert.IsType<CustomTestCaseOrderer>(runner.TestCaseOrderer);
        }

        [Fact]
        public static async void SettingUnknownTestCaseOrderLogsDiagnosticMessage()
        {
            var testCase = Mocks.XunitTestCase<TestClassWithUnknownTestCaseOrderer>("Passing");
            var runner = TestableXunitTestClassRunner.Create(testCase);

            await runner.RunAsync();

            Assert.IsType<MockTestCaseOrderer>(runner.TestCaseOrderer);
            var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<IDiagnosticMessage>());
            Assert.Equal("Could not find type 'UnknownType' in UnknownAssembly for class-level test case orderer on test class 'XunitTestClassRunnerTests+TestCaseOrderer+TestClassWithUnknownTestCaseOrderer'", diagnosticMessage.Message);
        }

        [TestCaseOrderer("UnknownType", "UnknownAssembly")]
        class TestClassWithUnknownTestCaseOrderer
        {
            [Fact]
            public void Passing() { }
        }

        [Fact]
        public static async void SettingTestCaseOrdererWithThrowingConstructorLogsDiagnosticMessage()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            var testCase = Mocks.XunitTestCase<TestClassWithCtorThrowingTestCaseOrder>("Passing");
            var runner = TestableXunitTestClassRunner.Create(testCase);

            await runner.RunAsync();

            Assert.IsType<MockTestCaseOrderer>(runner.TestCaseOrderer);
            var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<IDiagnosticMessage>());
            Assert.StartsWith("Class-level test case orderer 'XunitTestClassRunnerTests+TestCaseOrderer+MyCtorThrowingTestCaseOrderer' for test class 'XunitTestClassRunnerTests+TestCaseOrderer+TestClassWithCtorThrowingTestCaseOrder' threw 'System.DivideByZeroException' during construction: Attempted to divide by zero.", diagnosticMessage.Message);
        }

        [TestCaseOrderer("XunitTestClassRunnerTests+TestCaseOrderer+MyCtorThrowingTestCaseOrderer", "test.xunit.execution")]
        class TestClassWithCtorThrowingTestCaseOrder
        {
            [Fact]
            public void Passing() { }
        }

        class MyCtorThrowingTestCaseOrderer : ITestCaseOrderer
        {
            public MyCtorThrowingTestCaseOrderer()
            {
                throw new DivideByZeroException();
            }

            public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
                => Enumerable.Empty<TTestCase>();
        }
    }

    [Fact]
    public static async void PassesFixtureValuesToConstructor()
    {
        var testCase = Mocks.XunitTestCase<ClassUnderTest>("Passing");
        var runner = TestableXunitTestClassRunner.Create(testCase, 42, "Hello, world!", 21.12m);

        await runner.RunAsync();

        var args = Assert.Single(runner.ConstructorArguments);
        Assert.Collection(args,
            arg => Assert.IsType<FixtureUnderTest>(arg),
            arg => Assert.Equal("Hello, world!", arg),
            arg => Assert.Equal(21.12m, arg)
        );
    }

    class FixtureUnderTest : IDisposable
    {
        public bool Disposed;

        public void Dispose()
        {
            Disposed = true;
        }
    }

    class CollectionUnderTest : IClassFixture<object> { }

    [TestCaseOrderer("XunitTestClassRunnerTests+CustomTestCaseOrderer", "test.xunit.execution")]
    class ClassUnderTest : IClassFixture<FixtureUnderTest>
    {
        public ClassUnderTest(FixtureUnderTest x, string y, decimal z) { }

        [Fact]
        public void Passing() { }
    }

    class CustomTestCaseOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
            => testCases;
    }

    class TestableXunitTestClassRunner : XunitTestClassRunner
    {
        public List<object[]> ConstructorArguments = new List<object[]>();
        public List<IMessageSinkMessage> DiagnosticMessages;
        public Exception RunTestMethodAsync_AggregatorResult;

        TestableXunitTestClassRunner(ITestClass testClass,
                                     IReflectionTypeInfo @class,
                                     IEnumerable<IXunitTestCase> testCases,
                                     List<IMessageSinkMessage> diagnosticMessages,
                                     IMessageBus messageBus,
                                     ITestCaseOrderer testCaseOrderer,
                                     ExceptionAggregator aggregator,
                                     CancellationTokenSource cancellationTokenSource,
                                     IDictionary<Type, object> collectionFixtureMappings)
            : base(testClass, @class, testCases, SpyMessageSink.Create(messages: diagnosticMessages), messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings)
        {
            DiagnosticMessages = diagnosticMessages;
        }

        public new Dictionary<Type, object> ClassFixtureMappings
            => base.ClassFixtureMappings;

        public new ITestCaseOrderer TestCaseOrderer
            => base.TestCaseOrderer;

        public new IMessageSink DiagnosticMessageSink
            => base.DiagnosticMessageSink;

        public static TestableXunitTestClassRunner Create(IXunitTestCase testCase, params object[] collectionFixtures)
            => new TestableXunitTestClassRunner(
                testCase.TestMethod.TestClass,
                (IReflectionTypeInfo)testCase.TestMethod.TestClass.Class,
                new[] { testCase },
                new List<IMessageSinkMessage>(),
                new SpyMessageBus(),
                new MockTestCaseOrderer(),
                new ExceptionAggregator(),
                new CancellationTokenSource(),
                collectionFixtures.ToDictionary(fixture => fixture.GetType())
            );

        protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, object[] constructorArguments)
        {
            ConstructorArguments.Add(constructorArguments);
            RunTestMethodAsync_AggregatorResult = Aggregator.ToException();

            return Task.FromResult(new RunSummary());
        }
    }
}
