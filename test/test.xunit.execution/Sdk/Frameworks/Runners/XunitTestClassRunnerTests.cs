using System;
using System.Collections.Generic;
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

        var ex = runner.Aggregator.ToException();
        Assert.IsType<TestClassException>(ex);
        Assert.Equal("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead).", ex.Message);
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

        var ex = runner.Aggregator.ToException();
        Assert.IsType<TestClassException>(ex);
        Assert.Equal("A test class may only define a single public constructor.", ex.Message);
    }

    class ClassWithTwoConstructors
    {
        public ClassWithTwoConstructors() { }
        public ClassWithTwoConstructors(int x) { }

        [Fact]
        public void Passing() { }
    }

    [Fact]
    public static async void CreatesFixturesFromClassAndCollection()
    {
        var collection = new XunitTestCollection { CollectionDefinition = Reflector.Wrap(typeof(CollectionUnderTest)) };
        var testCase = Mocks.XunitTestCase<ClassUnderTest>("Passing", collection);
        var runner = TestableXunitTestClassRunner.Create(testCase);

        await runner.RunAsync();

        Assert.Collection(runner.ClassFixtureMappings.OrderBy(mapping => mapping.Key.Name),
            mapping => Assert.IsType<FixtureUnderTest>(mapping.Value),
            mapping => Assert.IsType<Object>(mapping.Value)
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
    public static async void UsesCustomTestOrderer()
    {
        var testCase = Mocks.XunitTestCase<ClassUnderTest>("Passing");
        var runner = TestableXunitTestClassRunner.Create(testCase);

        await runner.RunAsync();

        Assert.IsType<CustomTestCaseOrderer>(runner.TestCaseOrderer);
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
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            return testCases;
        }
    }

    class TestableXunitTestClassRunner : XunitTestClassRunner
    {
        public new readonly ExceptionAggregator Aggregator;
        public List<object[]> ConstructorArguments = new List<object[]>();

        TestableXunitTestClassRunner(ITestCollection testCollection,
                                     IReflectionTypeInfo testClass,
                                     IEnumerable<IXunitTestCase> testCases,
                                     IMessageBus messageBus,
                                     ITestCaseOrderer testCaseOrderer,
                                     ExceptionAggregator aggregator,
                                     CancellationTokenSource cancellationTokenSource,
                                     IDictionary<Type, object> collectionFixtureMappings)
            : base(testCollection, testClass, testCases, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings)
        {
            Aggregator = aggregator;
        }

        public new Dictionary<Type, object> ClassFixtureMappings
        {
            get { return base.ClassFixtureMappings; }
        }

        public new ITestCaseOrderer TestCaseOrderer
        {
            get { return base.TestCaseOrderer; }
        }

        public static TestableXunitTestClassRunner Create(IXunitTestCase testCase, params object[] collectionFixtures)
        {
            return new TestableXunitTestClassRunner(
                testCase.TestCollection,
                (IReflectionTypeInfo)testCase.Class,
                new[] { testCase },
                new SpyMessageBus(),
                new MockTestCaseOrderer(),
                new ExceptionAggregator(),
                new CancellationTokenSource(),
                collectionFixtures.ToDictionary(fixture => fixture.GetType())
            );
        }

        protected override Task<RunSummary> RunTestMethodAsync(IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, object[] constructorArguments)
        {
            ConstructorArguments.Add(constructorArguments);

            return Task.FromResult(new RunSummary());
        }
    }
}
