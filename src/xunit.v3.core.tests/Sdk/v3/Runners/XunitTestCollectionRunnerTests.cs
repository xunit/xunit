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
	public static async void CreatesFixtures()
	{
		var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionUnderTest)), "Mock Test Collection");
		var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("CreatesFixtures", collection);
		var runner = TestableXunitTestCollectionRunner.Create(testCase);

		await runner.RunAsync();

		Assert.Collection(
			runner.CollectionFixtureMappings.OrderBy(mapping => mapping.Key.Name),
			mapping => Assert.IsType<FixtureUnderTest>(mapping.Value),
			mapping => Assert.IsType<object>(mapping.Value)
		);
	}

	[Fact]
	public static async void DisposesFixtures()
	{
		var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionUnderTest)), "Mock Test Collection");
		var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("DisposesFixtures", collection);
		var runner = TestableXunitTestCollectionRunner.Create(testCase);

		await runner.RunAsync();

		var fixtureUnderTest = runner.CollectionFixtureMappings.Values.OfType<FixtureUnderTest>().Single();
		Assert.True(fixtureUnderTest.Disposed);
	}

	[Fact]
	public static async void DisposeAndAsyncDisposableShouldBeCalledInTheRightOrder()
	{
		var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionForFixtureAsyncDisposableUnderTest)), "Mock Test Collection");
		var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("DisposeAndAsyncDisposableShouldBeCalledInTheRightOrder", collection);
		var runner = TestableXunitTestCollectionRunner.Create(testCase);

		var runnerSessionTask = runner.RunAsync();

		await Task.Delay(500);

		var fixtureUnderTest = runner.CollectionFixtureMappings.Values.OfType<FixtureAsyncDisposableUnderTest>().Single();

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
	public static async void MultiplePublicConstructorsOnCollectionFixture_ReturnsError()
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
	public static async void UnresolvedConstructorParameterOnCollectionFixture_ReturnsError()
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
	public static async void CanInjectMessageSinkIntoCollectionFixture()
	{
		var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionWithCollectionFixtureWithMessageSinkDependency)), "Mock Test Collection");
		var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("CreatesFixtures", collection);
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
		var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionWithCollectionFixtureWithMessageSinkDependency)), "Mock Test Collection");
		var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("CreatesFixtures", collection);
		var runner = TestableXunitTestCollectionRunner.Create(testCase);

		await runner.RunAsync();

		var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<_DiagnosticMessage>());
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
		public static async void UsesCustomTestOrderer()
		{
			var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionUnderTest)), "Mock Test Collection");
			var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("DisposesFixtures", collection);
			var runner = TestableXunitTestCollectionRunner.Create(testCase);

			await runner.RunAsync();

			Assert.IsType<CustomTestCaseOrderer>(runner.TestCaseOrderer);
		}

		[Fact]
		public static async void SettingUnknownTestCaseOrderLogsDiagnosticMessage()
		{
			var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionWithUnknownTestCaseOrderer)), "TestCollectionDisplayName");
			var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("DisposesFixtures", collection);
			var runner = TestableXunitTestCollectionRunner.Create(testCase);

			await runner.RunAsync();

			Assert.IsType<MockTestCaseOrderer>(runner.TestCaseOrderer);
			var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<_DiagnosticMessage>());
			Assert.Equal("Could not find type 'UnknownType' in UnknownAssembly for collection-level test case orderer on test collection 'TestCollectionDisplayName'", diagnosticMessage.Message);
		}

		[TestCaseOrderer("UnknownType", "UnknownAssembly")]
		class CollectionWithUnknownTestCaseOrderer { }

		[Fact]
		public static async void SettingTestCaseOrdererWithThrowingConstructorLogsDiagnosticMessage()
		{
			var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionWithCtorThrowingTestCaseOrderer)), "TestCollectionDisplayName");
			var testCase = TestData.XunitTestCase<XunitTestCollectionRunnerTests>("DisposesFixtures", collection);
			var runner = TestableXunitTestCollectionRunner.Create(testCase);

			await runner.RunAsync();

			Assert.IsType<MockTestCaseOrderer>(runner.TestCaseOrderer);
			var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<_DiagnosticMessage>());
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
				where TTestCase : _ITestCase
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
			where TTestCase : _ITestCase
		{
			return testCases;
		}
	}

	class TestableXunitTestCollectionRunner : XunitTestCollectionRunner
	{
		public List<_MessageSinkMessage> DiagnosticMessages;
		public Exception? RunTestClassAsync_AggregatorResult;

		TestableXunitTestCollectionRunner(
			_ITestCollection testCollection,
			IReadOnlyCollection<IXunitTestCase> testCases,
			List<_MessageSinkMessage> diagnosticMessages,
			IMessageBus messageBus,
			ITestCaseOrderer testCaseOrderer,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
				: base(testCollection, testCases, SpyMessageSink.Create(messages: diagnosticMessages), messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
		{
			DiagnosticMessages = diagnosticMessages;
		}

		public static TestableXunitTestCollectionRunner Create(IXunitTestCase testCase) =>
			new(
				testCase.TestMethod.TestClass.TestCollection,
				new[] { testCase },
				new List<_MessageSinkMessage>(),
				new SpyMessageBus(),
				new MockTestCaseOrderer(),
				new ExceptionAggregator(),
				new CancellationTokenSource()
			);

		public new Dictionary<Type, object> CollectionFixtureMappings => base.CollectionFixtureMappings;

		public new ITestCaseOrderer TestCaseOrderer => base.TestCaseOrderer;

		public new _IMessageSink DiagnosticMessageSink => base.DiagnosticMessageSink;

		protected override Task<RunSummary> RunTestClassAsync(
			_ITestClass testClass,
			_IReflectionTypeInfo @class,
			IReadOnlyCollection<IXunitTestCase> testCases)
		{
			RunTestClassAsync_AggregatorResult = Aggregator.ToException();

			return Task.FromResult(new RunSummary());
		}
	}
}
