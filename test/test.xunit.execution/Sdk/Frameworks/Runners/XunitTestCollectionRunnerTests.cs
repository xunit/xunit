using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class XunitTestCollectionRunnerTests
{
    [Fact]
    public static async void CreatesFixtures()
    {
        var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionUnderTest)), null);
        var testCase = Mocks.XunitTestCase<XunitTestCollectionRunnerTests>("CreatesFixtures", collection);
        var runner = TestableXunitTestCollectionRunner.Create(testCase);

        await runner.RunAsync();

        Assert.Collection(runner.CollectionFixtureMappings.OrderBy(mapping => mapping.Key.Name),
            mapping => Assert.IsType<FixtureUnderTest>(mapping.Value),
            mapping => Assert.IsType<object>(mapping.Value)
        );
    }

    [Fact]
    public static async void DisposesFixtures()
    {
        var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionUnderTest)), null);
        var testCase = Mocks.XunitTestCase<XunitTestCollectionRunnerTests>("DisposesFixtures", collection);
        var runner = TestableXunitTestCollectionRunner.Create(testCase);

        await runner.RunAsync();

        var fixtureUnderTest = runner.CollectionFixtureMappings.Values.OfType<FixtureUnderTest>().Single();
        Assert.True(fixtureUnderTest.Disposed);
    }

    [Fact]
    public static async void DisposeAndAsyncLifetimeShouldBeCalledInTheRightOrder()
    {
        var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionForFixtureAsyncLifetimeAndDisposableUnderTest)), null);
        var testCase = Mocks.XunitTestCase<XunitTestCollectionRunnerTests>("DisposeAndAsyncLifetimeShouldBeCalledInTheRightOrder", collection);
        var runner = TestableXunitTestCollectionRunner.Create(testCase);

        var runnerSessionTask = runner.RunAsync();

        await Task.Delay(500);

        var fixtureUnderTest = runner.CollectionFixtureMappings.Values.OfType<FixtureAsyncLifetimeAndDisposableUnderTest>().Single();

        Assert.True(fixtureUnderTest.DisposeAsyncCalled);
        Assert.False(fixtureUnderTest.Disposed);

        fixtureUnderTest.DisposeAsyncSignaler.SetResult(true);

        await runnerSessionTask;

        Assert.True(fixtureUnderTest.Disposed);
    }

    class CollectionForFixtureAsyncLifetimeAndDisposableUnderTest : ICollectionFixture<FixtureAsyncLifetimeAndDisposableUnderTest> { }

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
    public static async void MultiplePublicConstructorsOnCollectionFixture_ReturnsError()
    {
        var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionsWithMultiCtorCollectionFixture)), null);
        var testCase = Mocks.XunitTestCase<XunitTestCollectionRunnerTests>("CreatesFixtures", collection);
        var runner = TestableXunitTestCollectionRunner.Create(testCase);

        await runner.RunAsync();

        var ex = Assert.IsType<TestClassException>(runner.RunTestClassAsync_AggregatorResult);
        Assert.Equal("Collection fixture type 'XunitTestCollectionRunnerTests+CollectionFixtureWithMultipleConstructors' may only define a single public constructor.", ex.Message);
    }

    class CollectionFixtureWithMultipleConstructors
    {
        public CollectionFixtureWithMultipleConstructors() { }
        public CollectionFixtureWithMultipleConstructors(int unused) { }
    }

    class CollectionsWithMultiCtorCollectionFixture : ICollectionFixture<CollectionFixtureWithMultipleConstructors> { }

    [Fact]
    public static async void UnresolvedConstructorParameterOnCollectionFixture_ReturnsError()
    {
        var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionWithCollectionFixtureWithDependency)), null);
        var testCase = Mocks.XunitTestCase<XunitTestCollectionRunnerTests>("CreatesFixtures", collection);
        var runner = TestableXunitTestCollectionRunner.Create(testCase);

        await runner.RunAsync();

        var ex = Assert.IsType<TestClassException>(runner.RunTestClassAsync_AggregatorResult);
        Assert.Equal("Collection fixture type 'XunitTestCollectionRunnerTests+CollectionFixtureWithCollectionFixtureDependency' had one or more unresolved constructor arguments: DependentCollectionFixture collectionFixture", ex.Message);
    }

    class DependentCollectionFixture { }

    class CollectionFixtureWithCollectionFixtureDependency
    {
        public DependentCollectionFixture CollectionFixture;

        public CollectionFixtureWithCollectionFixtureDependency(DependentCollectionFixture collectionFixture)
        {
            CollectionFixture = collectionFixture;
        }
    }

    class CollectionWithCollectionFixtureWithDependency : ICollectionFixture<CollectionFixtureWithCollectionFixtureDependency> { }

    [Fact]
    public static async void CanInjectMessageSinkIntoCollectionFixture()
    {
        var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionWithCollectionFixtureWithMessageSinkDependency)), null);
        var testCase = Mocks.XunitTestCase<XunitTestCollectionRunnerTests>("CreatesFixtures", collection);
        var runner = TestableXunitTestCollectionRunner.Create(testCase);

        await runner.RunAsync();

        Assert.Null(runner.RunTestClassAsync_AggregatorResult);
        var classFixture = runner.CollectionFixtureMappings.Values.OfType<CollectionFixtureWithMessageSinkDependency>().Single();
        Assert.NotNull(classFixture.MessageSink);
        Assert.Same(runner.DiagnosticMessageSink, classFixture.MessageSink);
    }

    [Fact]
    public static async void CanLogSinkMessageFromCollectionFixture()
    {
        var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionWithCollectionFixtureWithMessageSinkDependency)), null);
        var testCase = Mocks.XunitTestCase<XunitTestCollectionRunnerTests>("CreatesFixtures", collection);
        var runner = TestableXunitTestCollectionRunner.Create(testCase);

        await runner.RunAsync();

        var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<IDiagnosticMessage>());
        Assert.Equal("CollectionFixtureWithMessageSinkDependency constructor message", diagnosticMessage.Message);
    }

    class CollectionFixtureWithMessageSinkDependency
    {
        public IMessageSink MessageSink;

        public CollectionFixtureWithMessageSinkDependency(IMessageSink messageSink)
        {
            MessageSink = messageSink;
            MessageSink.OnMessage(new Xunit.Sdk.DiagnosticMessage("CollectionFixtureWithMessageSinkDependency constructor message"));
        }
    }

    class CollectionWithCollectionFixtureWithMessageSinkDependency : ICollectionFixture<CollectionFixtureWithMessageSinkDependency> { }

    public class TestCaseOrderer
    {
        [Fact]
        public static async void UsesCustomTestOrderer()
        {
            var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionUnderTest)), null);
            var testCase = Mocks.XunitTestCase<XunitTestCollectionRunnerTests>("DisposesFixtures", collection);
            var runner = TestableXunitTestCollectionRunner.Create(testCase);

            await runner.RunAsync();

            Assert.IsType<CustomTestCaseOrderer>(runner.TestCaseOrderer);
        }

        [Fact]
        public static async void SettingUnknownTestCaseOrderLogsDiagnosticMessage()
        {
            var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionWithUnknownTestCaseOrderer)), "TestCollectionDisplayName");
            var testCase = Mocks.XunitTestCase<XunitTestCollectionRunnerTests>("DisposesFixtures", collection);
            var runner = TestableXunitTestCollectionRunner.Create(testCase);

            await runner.RunAsync();

            Assert.IsType<MockTestCaseOrderer>(runner.TestCaseOrderer);
            var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<IDiagnosticMessage>());
            Assert.Equal("Could not find type 'UnknownType' in UnknownAssembly for collection-level test case orderer on test collection 'TestCollectionDisplayName'", diagnosticMessage.Message);
        }

        [TestCaseOrderer("UnknownType", "UnknownAssembly")]
        class CollectionWithUnknownTestCaseOrderer { }

        [Fact]
        public static async void SettingTestCaseOrdererWithThrowingConstructorLogsDiagnosticMessage()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionWithCtorThrowingTestCaseOrderer)), "TestCollectionDisplayName");
            var testCase = Mocks.XunitTestCase<XunitTestCollectionRunnerTests>("DisposesFixtures", collection);
            var runner = TestableXunitTestCollectionRunner.Create(testCase);

            await runner.RunAsync();

            Assert.IsType<MockTestCaseOrderer>(runner.TestCaseOrderer);
            var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<IDiagnosticMessage>());
            Assert.StartsWith("Collection-level test case orderer 'XunitTestCollectionRunnerTests+TestCaseOrderer+MyCtorThrowingTestCaseOrderer' for test collection 'TestCollectionDisplayName' threw 'System.DivideByZeroException' during construction: Attempted to divide by zero.", diagnosticMessage.Message);
        }

        [TestCaseOrderer("XunitTestCollectionRunnerTests+TestCaseOrderer+MyCtorThrowingTestCaseOrderer", "test.xunit.execution")]
        class CollectionWithCtorThrowingTestCaseOrderer { }

        class MyCtorThrowingTestCaseOrderer : ITestCaseOrderer
        {
            public MyCtorThrowingTestCaseOrderer()
            {
                throw new DivideByZeroException();
            }

            public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
            {
                return Enumerable.Empty<TTestCase>();
            }
        }
    }

    class FixtureUnderTest : IDisposable
    {
        public bool Disposed;

        public void Dispose()
        {
            Disposed = true;
        }
    }

    [TestCaseOrderer("XunitTestCollectionRunnerTests+CustomTestCaseOrderer", "test.xunit.execution")]
    class CollectionUnderTest : ICollectionFixture<FixtureUnderTest>, ICollectionFixture<object> { }

    class CustomTestCaseOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            return testCases;
        }
    }

    class TestableXunitTestCollectionRunner : XunitTestCollectionRunner
    {
        public List<IMessageSinkMessage> DiagnosticMessages;
        public Exception RunTestClassAsync_AggregatorResult;

        TestableXunitTestCollectionRunner(ITestCollection testCollection,
                                          IEnumerable<IXunitTestCase> testCases,
                                          List<IMessageSinkMessage> diagnosticMessages,
                                          IMessageBus messageBus,
                                          ITestCaseOrderer testCaseOrderer,
                                          ExceptionAggregator aggregator,
                                          CancellationTokenSource cancellationTokenSource)
            : base(testCollection, testCases, SpyMessageSink.Create(messages: diagnosticMessages), messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            DiagnosticMessages = diagnosticMessages;
        }

        public static TestableXunitTestCollectionRunner Create(IXunitTestCase testCase)
        {
            return new TestableXunitTestCollectionRunner(
                testCase.TestMethod.TestClass.TestCollection,
                new[] { testCase },
                new List<IMessageSinkMessage>(),
                new SpyMessageBus(),
                new MockTestCaseOrderer(),
                new ExceptionAggregator(),
                new CancellationTokenSource()
            );
        }

        public new Dictionary<Type, object> CollectionFixtureMappings
        {
            get { return base.CollectionFixtureMappings; }
        }

        public new ITestCaseOrderer TestCaseOrderer
        {
            get { return base.TestCaseOrderer; }
        }

        public new IMessageSink DiagnosticMessageSink
        {
            get { return base.DiagnosticMessageSink; }
        }

        protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
        {
            RunTestClassAsync_AggregatorResult = Aggregator.ToException();

            return Task.FromResult(new RunSummary());
        }
    }
}
