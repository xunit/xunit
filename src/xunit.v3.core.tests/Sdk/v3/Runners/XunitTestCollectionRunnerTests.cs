using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestCollectionRunnerTests
{
	[Fact]
	public static async ValueTask CreatesFixtures()
	{
		var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionUnderTest)), "Mock Test Collection");
		var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("CreatesFixtures", collection);
		var runner = TestableXunitTestCollectionRunner.Create(testCase);

		await runner.RunAsync();

		Assert.NotNull(runner.RunTestClassesAsync_CollectionFixtureMappings);
		Assert.Collection(
			runner.RunTestClassesAsync_CollectionFixtureMappings.OrderBy(mapping => mapping.Key.Name),
			mapping => Assert.IsType<FixtureUnderTest>(mapping.Value),
			mapping => Assert.IsType<object>(mapping.Value)
		);
	}

	[Fact]
	public static async ValueTask DisposesFixtures()
	{
		var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionUnderTest)), "Mock Test Collection");
		var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("DisposesFixtures", collection);
		var runner = TestableXunitTestCollectionRunner.Create(testCase);

		await runner.RunAsync();

		Assert.NotNull(runner.RunTestClassesAsync_CollectionFixtureMappings);
		var fixtureUnderTest = runner.RunTestClassesAsync_CollectionFixtureMappings.Values.OfType<FixtureUnderTest>().Single();
		Assert.True(fixtureUnderTest.Disposed);
	}

	[Fact]
	public static async ValueTask DisposeAndAsyncDisposableShouldBeCalledInTheRightOrder()
	{
		var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionForFixtureAsyncDisposableUnderTest)), "Mock Test Collection");
		var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("DisposeAndAsyncDisposableShouldBeCalledInTheRightOrder", collection);
		var runner = TestableXunitTestCollectionRunner.Create(testCase);

		var runnerSessionTask = runner.RunAsync();

		await Task.Delay(500);

		Assert.NotNull(runner.RunTestClassesAsync_CollectionFixtureMappings);
		var fixtureUnderTest = runner.RunTestClassesAsync_CollectionFixtureMappings.Values.OfType<FixtureAsyncDisposableUnderTest>().Single();

		Assert.True(fixtureUnderTest.DisposeAsyncCalled);
		Assert.False(fixtureUnderTest.Disposed);

		fixtureUnderTest.DisposeAsyncSignaler.SetResult(true);

		await runnerSessionTask;

		Assert.True(fixtureUnderTest.Disposed);
	}

	class CollectionForFixtureAsyncDisposableUnderTest : ICollectionFixture<FixtureAsyncDisposableUnderTest> { }

	class FixtureAsyncDisposableUnderTest : IAsyncDisposable, IDisposable
	{
		public bool Disposed;

		public bool DisposeAsyncCalled;

		public TaskCompletionSource<bool> DisposeAsyncSignaler = new();

		public void Dispose()
		{
			Disposed = true;
		}

		public async ValueTask DisposeAsync()
		{
			DisposeAsyncCalled = true;

			await DisposeAsyncSignaler.Task;
		}
	}


	[Fact]
	public static async ValueTask MultiplePublicConstructorsOnCollectionFixture_ReturnsError()
	{
		var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionsWithMultiCtorCollectionFixture)), "Mock Test Collection");
		var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("CreatesFixtures", collection);
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
	public static async ValueTask UnresolvedConstructorParameterOnCollectionFixture_ReturnsError()
	{
		var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionWithCollectionFixtureWithDependency)), "Mock Test Collection");
		var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("CreatesFixtures", collection);
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
	public static async ValueTask CanInjectMessageSinkIntoCollectionFixture()
	{
		var spy = SpyMessageSink.Capture();
		TestContext.Current!.DiagnosticMessageSink = spy;
		var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionWithCollectionFixtureWithMessageSinkDependency)), "Mock Test Collection");
		var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("CreatesFixtures", collection);
		var runner = TestableXunitTestCollectionRunner.Create(testCase);

		await runner.RunAsync();

		Assert.Null(runner.RunTestClassAsync_AggregatorResult);
		Assert.NotNull(runner.RunTestClassesAsync_CollectionFixtureMappings);
		var classFixture = runner.RunTestClassesAsync_CollectionFixtureMappings.Values.OfType<CollectionFixtureWithMessageSinkDependency>().Single();
		Assert.NotNull(classFixture.MessageSink);
		Assert.Same(spy, classFixture.MessageSink);
	}

	[Fact]
	public static async ValueTask CanLogSinkMessageFromCollectionFixture()
	{
		var spy = SpyMessageSink.Capture();
		TestContext.Current!.DiagnosticMessageSink = spy;
		var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionWithCollectionFixtureWithMessageSinkDependency)), "Mock Test Collection");
		var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("CreatesFixtures", collection);
		var runner = TestableXunitTestCollectionRunner.Create(testCase);

		await runner.RunAsync();

		var diagnosticMessage = Assert.Single(spy.Messages.Cast<_DiagnosticMessage>());
		Assert.Equal("CollectionFixtureWithMessageSinkDependency constructor message", diagnosticMessage.Message);
	}

	class CollectionFixtureWithMessageSinkDependency
	{
		public _IMessageSink MessageSink;

		public CollectionFixtureWithMessageSinkDependency(_IMessageSink messageSink)
		{
			MessageSink = messageSink;
			MessageSink.OnMessage(new _DiagnosticMessage { Message = "CollectionFixtureWithMessageSinkDependency constructor message" });
		}
	}

	class CollectionWithCollectionFixtureWithMessageSinkDependency : ICollectionFixture<CollectionFixtureWithMessageSinkDependency> { }

	public class TestCaseOrderer
	{
		[Fact]
		public static async ValueTask UsesCustomTestOrderer()
		{
			var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionUnderTest)), "Mock Test Collection");
			var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("DisposesFixtures", collection);
			var runner = TestableXunitTestCollectionRunner.Create(testCase);

			await runner.RunAsync();

			Assert.IsType<CustomTestCaseOrderer>(runner.RunTestClassesAsync_TestCaseOrderer);
		}

		[Fact]
		public static async ValueTask SettingUnknownTestCaseOrderLogsDiagnosticMessage()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;
			var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionWithUnknownTestCaseOrderer)), "TestCollectionDisplayName");
			var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("DisposesFixtures", collection);
			var runner = TestableXunitTestCollectionRunner.Create(testCase);

			await runner.RunAsync();

			Assert.IsType<MockTestCaseOrderer>(runner.RunTestClassesAsync_TestCaseOrderer);
			var diagnosticMessage = Assert.Single(spy.Messages.Cast<_DiagnosticMessage>());
			Assert.Equal("Could not find type 'UnknownType' in UnknownAssembly for collection-level test case orderer on test collection 'TestCollectionDisplayName'", diagnosticMessage.Message);
		}

		[TestCaseOrderer("UnknownType", "UnknownAssembly")]
		class CollectionWithUnknownTestCaseOrderer { }

		[Fact]
		public static async ValueTask SettingTestCaseOrdererWithThrowingConstructorLogsDiagnosticMessage()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;
			var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionWithCtorThrowingTestCaseOrderer)), "TestCollectionDisplayName");
			var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("DisposesFixtures", collection);
			var runner = TestableXunitTestCollectionRunner.Create(testCase);

			await runner.RunAsync();

			Assert.IsType<MockTestCaseOrderer>(runner.RunTestClassesAsync_TestCaseOrderer);
			var diagnosticMessage = Assert.Single(spy.Messages.Cast<_DiagnosticMessage>());
			Assert.StartsWith("Collection-level test case orderer 'XunitTestCollectionRunnerTests+TestCaseOrderer+MyCtorThrowingTestCaseOrderer' for test collection 'TestCollectionDisplayName' threw 'System.DivideByZeroException' during construction: Attempted to divide by zero.", diagnosticMessage.Message);
		}

		[TestCaseOrderer(typeof(MyCtorThrowingTestCaseOrderer))]
		class CollectionWithCtorThrowingTestCaseOrderer { }

		class MyCtorThrowingTestCaseOrderer : ITestCaseOrderer
		{
			public MyCtorThrowingTestCaseOrderer()
			{
				throw new DivideByZeroException();
			}

			public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
				where TTestCase : notnull, _ITestCase
			{
				return Array.Empty<TTestCase>();
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

	[TestCaseOrderer(typeof(CustomTestCaseOrderer))]
	class CollectionUnderTest : ICollectionFixture<FixtureUnderTest>, ICollectionFixture<object> { }

	class CustomTestCaseOrderer : ITestCaseOrderer
	{
		public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
			where TTestCase : notnull, _ITestCase
		{
			return testCases;
		}
	}

	class TestableXunitTestCollectionRunner : XunitTestCollectionRunner
	{
		readonly ExceptionAggregator aggregator;
		readonly IReadOnlyDictionary<Type, object> assemblyFixtureMappings;
		readonly CancellationTokenSource cancellationTokenSource;
		readonly IMessageBus messageBus;
		readonly ITestCaseOrderer testCaseOrderer;
		readonly IReadOnlyCollection<IXunitTestCase> testCases;
		readonly _ITestCollection testCollection;

		public Exception? RunTestClassAsync_AggregatorResult;
		public Dictionary<Type, object>? RunTestClassesAsync_CollectionFixtureMappings;
		public ITestCaseOrderer? RunTestClassesAsync_TestCaseOrderer;

		TestableXunitTestCollectionRunner(
			_ITestCollection testCollection,
			IReadOnlyCollection<IXunitTestCase> testCases,
			IMessageBus messageBus,
			ITestCaseOrderer testCaseOrderer,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource,
			IReadOnlyDictionary<Type, object> assemblyFixtureMappings)
		{
			this.testCollection = testCollection;
			this.testCases = testCases;
			this.messageBus = messageBus;
			this.testCaseOrderer = testCaseOrderer;
			this.aggregator = aggregator;
			this.cancellationTokenSource = cancellationTokenSource;
			this.assemblyFixtureMappings = assemblyFixtureMappings;
		}

		public static TestableXunitTestCollectionRunner Create(
			IXunitTestCase testCase,
			params object[] assemblyFixtures) =>
				new(
					testCase.TestCollection,
					new[] { testCase },
					new SpyMessageBus(),
					new MockTestCaseOrderer(),
					new ExceptionAggregator(),
					new CancellationTokenSource(),
					assemblyFixtures.ToDictionary(fixture => fixture.GetType())
				);

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new XunitTestCollectionRunnerContext(testCollection, testCases, ExplicitOption.Off, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, assemblyFixtureMappings);
			await ctxt.InitializeAsync();

			return await RunAsync(ctxt);
		}

		protected override ValueTask<RunSummary> RunTestClassesAsync(XunitTestCollectionRunnerContext ctxt)
		{
			var result = base.RunTestClassesAsync(ctxt);

			RunTestClassesAsync_CollectionFixtureMappings = ctxt.CollectionFixtureMappings;
			RunTestClassesAsync_TestCaseOrderer = ctxt.TestCaseOrderer;

			return result;
		}

		protected override ValueTask<RunSummary> RunTestClassAsync(
			XunitTestCollectionRunnerContext ctxt,
			_ITestClass? testClass,
			_IReflectionTypeInfo? @class,
			IReadOnlyCollection<IXunitTestCase> testCases)
		{
			RunTestClassAsync_AggregatorResult = ctxt.Aggregator.ToException();

			return new(new RunSummary());
		}
	}
}
