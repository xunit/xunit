using System;
using System.Collections.Generic;
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
            mapping => Assert.IsType<Object>(mapping.Value)
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

        protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
        {
            return Task.FromResult(new RunSummary());
        }
    }
}
