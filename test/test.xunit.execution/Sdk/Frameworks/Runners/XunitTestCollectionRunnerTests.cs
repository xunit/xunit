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

    [Fact]
    public static async void UsesCustomTestOrderer()
    {
        var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionUnderTest)), null);
        var testCase = Mocks.XunitTestCase<XunitTestCollectionRunnerTests>("DisposesFixtures", collection);
        var runner = TestableXunitTestCollectionRunner.Create(testCase);

        await runner.RunAsync();

        Assert.IsType<CustomTestCaseOrderer>(runner.TestCaseOrderer);
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
        TestableXunitTestCollectionRunner(ITestCollection testCollection,
                                          IEnumerable<IXunitTestCase> testCases,
                                          IMessageSink diagnosticMessageSink,
                                          IMessageBus messageBus,
                                          ITestCaseOrderer testCaseOrderer,
                                          ExceptionAggregator aggregator,
                                          CancellationTokenSource cancellationTokenSource)
            : base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource) { }

        public static TestableXunitTestCollectionRunner Create(IXunitTestCase testCase)
        {
            return new TestableXunitTestCollectionRunner(
                testCase.TestMethod.TestClass.TestCollection,
                new[] { testCase },
                SpyMessageSink.Create(),
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
