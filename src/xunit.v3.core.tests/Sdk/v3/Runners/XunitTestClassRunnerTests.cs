using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestClassRunnerTests
{
	[Fact]
	public static async ValueTask ClassCannotBeDecoratedWithICollectionFixture()
	{
		var testCase = TestData.XunitTestCase<ClassWithCollectionFixture>("Passing");
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
	public static async ValueTask TestClassCannotHaveMoreThanOneConstructor()
	{
		var testCase = TestData.XunitTestCase<ClassWithTwoConstructors>("Passing");
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
	public static async ValueTask TestClassCanHavePublicAndPrivateConstructor()
	{
		var testCase = TestData.XunitTestCase<ClassWithMixedConstructors>("Passing");
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
	public static async ValueTask TestClassCanHaveStaticConstructor()
	{
		var testCase = TestData.XunitTestCase<ClassWithStaticConstructor>("Passing");
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
	public static async ValueTask CreatesFixturesFromClassAndCollection()
	{
		var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionUnderTest)), "Mock Test Collection");
		var testCase = TestData.XunitTestCase<ClassUnderTest>("Passing", collection);
		var runner = TestableXunitTestClassRunner.Create(testCase);

		await runner.RunAsync();

		Assert.NotNull(runner.RunTestMethodsAsync_ClassFixtureMappings);
		Assert.Collection(
			runner.RunTestMethodsAsync_ClassFixtureMappings.OrderBy(mapping => mapping.Key.Name),
			mapping => Assert.IsType<FixtureUnderTest>(mapping.Value),
			mapping => Assert.IsType<object>(mapping.Value)
		);
	}

	[Fact]
	public static async ValueTask DisposesFixtures()
	{
		var testCase = TestData.XunitTestCase<ClassUnderTest>("Passing");
		var runner = TestableXunitTestClassRunner.Create(testCase);

		await runner.RunAsync();

		Assert.NotNull(runner.RunTestMethodsAsync_ClassFixtureMappings);
		var fixtureUnderTest = runner.RunTestMethodsAsync_ClassFixtureMappings.Values.OfType<FixtureUnderTest>().Single();
		Assert.True(fixtureUnderTest.Disposed);
	}

	[Fact]
	public static async ValueTask DisposeAndAsyncDisposableShouldBeCalledInTheRightOrder()
	{
		var testCase = TestData.XunitTestCase<TestClassForFixtureAsyncDisposableUnderTest>("Passing");
		var runner = TestableXunitTestClassRunner.Create(testCase);

		var runnerSessionTask = runner.RunAsync();

		await Task.Delay(500);

		Assert.NotNull(runner.RunTestMethodsAsync_ClassFixtureMappings);
		var fixtureUnderTest = runner.RunTestMethodsAsync_ClassFixtureMappings.Values.OfType<FixtureAsyncDisposableUnderTest>().Single();

		Assert.True(fixtureUnderTest.DisposeAsyncCalled);
		Assert.False(fixtureUnderTest.Disposed);

		fixtureUnderTest.DisposeAsyncSignaler.SetResult(true);

		await runnerSessionTask;

		Assert.True(fixtureUnderTest.Disposed);
	}

	class TestClassForFixtureAsyncDisposableUnderTest : IClassFixture<FixtureAsyncDisposableUnderTest>
	{
		[Fact]
		public void Passing() { }
	}

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
	public static async ValueTask MultiplePublicConstructorsOnClassFixture_ReturnsError()
	{
		var testCase = TestData.XunitTestCase<TestClassWithMultiCtorClassFixture>("Passing");
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
	public static async ValueTask UnresolvedConstructorParameterOnClassFixture_ReturnsError()
	{
		var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithDependency>("Passing");
		var runner = TestableXunitTestClassRunner.Create(testCase);

		await runner.RunAsync();

		var ex = Assert.IsType<TestClassException>(runner.RunTestMethodAsync_AggregatorResult);
		Assert.Equal("Class fixture type 'XunitTestClassRunnerTests+ClassFixtureWithCollectionFixtureDependency' had one or more unresolved constructor arguments: DependentCollectionFixture collectionFixture", ex.Message);
	}

	[Fact]
	public static async ValueTask CanInjectCollectionFixtureIntoClassFixture()
	{
		var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithDependency>("Passing");
		var collectionFixture = new DependentCollectionFixture();
		var runner = TestableXunitTestClassRunner.Create(testCase, collectionFixture);

		await runner.RunAsync();

		Assert.Null(runner.RunTestMethodAsync_AggregatorResult);
		Assert.NotNull(runner.RunTestMethodsAsync_ClassFixtureMappings);
		var classFixture = runner.RunTestMethodsAsync_ClassFixtureMappings.Values.OfType<ClassFixtureWithCollectionFixtureDependency>().Single();
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
	public static async ValueTask CanInjectMessageSinkIntoClassFixture()
	{
		var spy = SpyMessageSink.Capture();
		TestContext.Current!.DiagnosticMessageSink = spy;
		var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithMessageSinkDependency>("Passing");
		var runner = TestableXunitTestClassRunner.Create(testCase);

		await runner.RunAsync();

		Assert.Null(runner.RunTestMethodAsync_AggregatorResult);
		Assert.NotNull(runner.RunTestMethodsAsync_ClassFixtureMappings);
		var classFixture = runner.RunTestMethodsAsync_ClassFixtureMappings.Values.OfType<ClassFixtureWithMessageSinkDependency>().Single();
		Assert.NotNull(classFixture.MessageSink);
		Assert.Same(spy, classFixture.MessageSink);
	}

	[Fact]
	public static async ValueTask CanLogSinkMessageFromClassFixture()
	{
		var spy = SpyMessageSink.Capture();
		TestContext.Current!.DiagnosticMessageSink = spy;
		var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithMessageSinkDependency>("Passing");
		var runner = TestableXunitTestClassRunner.Create(testCase);

		await runner.RunAsync();

		var diagnosticMessage = Assert.Single(spy.Messages.Cast<_DiagnosticMessage>());
		Assert.Equal("ClassFixtureWithMessageSinkDependency constructor message", diagnosticMessage.Message);
	}

	class ClassFixtureWithMessageSinkDependency
	{
		public _IMessageSink MessageSink;

		public ClassFixtureWithMessageSinkDependency(_IMessageSink messageSink)
		{
			MessageSink = messageSink;
			MessageSink.OnMessage(new _DiagnosticMessage { Message = "ClassFixtureWithMessageSinkDependency constructor message" });
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
		public static async ValueTask UsesCustomTestOrderer()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>("Passing");
			var runner = TestableXunitTestClassRunner.Create(testCase);

			await runner.RunAsync();

			Assert.IsType<CustomTestCaseOrderer>(runner.RunTestMethodsAsync_TestCaseOrderer);
		}

		[Fact]
		public static async ValueTask SettingUnknownTestCaseOrderLogsDiagnosticMessage()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;
			var testCase = TestData.XunitTestCase<TestClassWithUnknownTestCaseOrderer>("Passing");
			var runner = TestableXunitTestClassRunner.Create(testCase);

			await runner.RunAsync();

			Assert.IsType<MockTestCaseOrderer>(runner.RunTestMethodsAsync_TestCaseOrderer);
			var diagnosticMessage = Assert.Single(spy.Messages.Cast<_DiagnosticMessage>());
			Assert.Equal("Could not find type 'UnknownType' in UnknownAssembly for class-level test case orderer on test class 'XunitTestClassRunnerTests+TestCaseOrderer+TestClassWithUnknownTestCaseOrderer'", diagnosticMessage.Message);
		}

		[TestCaseOrderer("UnknownType", "UnknownAssembly")]
		class TestClassWithUnknownTestCaseOrderer
		{
			[Fact]
			public void Passing() { }
		}

		[Fact]
		public static async ValueTask SettingTestCaseOrdererWithThrowingConstructorLogsDiagnosticMessage()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;
			var testCase = TestData.XunitTestCase<TestClassWithCtorThrowingTestCaseOrder>("Passing");
			var runner = TestableXunitTestClassRunner.Create(testCase);

			await runner.RunAsync();

			Assert.IsType<MockTestCaseOrderer>(runner.RunTestMethodsAsync_TestCaseOrderer);
			var diagnosticMessage = Assert.Single(spy.Messages.Cast<_DiagnosticMessage>());
			Assert.StartsWith("Class-level test case orderer 'XunitTestClassRunnerTests+TestCaseOrderer+MyCtorThrowingTestCaseOrderer' for test class 'XunitTestClassRunnerTests+TestCaseOrderer+TestClassWithCtorThrowingTestCaseOrder' threw 'System.DivideByZeroException' during construction: Attempted to divide by zero.", diagnosticMessage.Message);
		}

		[TestCaseOrderer(typeof(MyCtorThrowingTestCaseOrderer))]
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

			public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
				where TTestCase : notnull, _ITestCase
					=> Array.Empty<TTestCase>();
		}
	}

	[Fact]
	public static async ValueTask PassesFixtureValuesToConstructor()
	{
		var testCase = TestData.XunitTestCase<ClassUnderTest>("Passing");
		var runner = TestableXunitTestClassRunner.Create(testCase, 42, "Hello, world!", 21.12m);

		await runner.RunAsync();

		var args = Assert.Single(runner.ConstructorArguments);
		Assert.Collection(
			args,
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

	[TestCaseOrderer(typeof(CustomTestCaseOrderer))]
	class ClassUnderTest : IClassFixture<FixtureUnderTest>
	{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
		public ClassUnderTest(FixtureUnderTest x, string y, decimal z) { }
#pragma warning restore xUnit1041

		[Fact]
		public void Passing() { }
	}

	class CustomTestCaseOrderer : ITestCaseOrderer
	{
		public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
			where TTestCase : notnull, _ITestCase
				=> testCases;
	}

	class TestableXunitTestClassRunner : XunitTestClassRunner
	{
		readonly ExceptionAggregator aggregator;
		readonly IReadOnlyDictionary<Type, object> assemblyFixtureMappings;
		readonly CancellationTokenSource cancellationTokenSource;
		readonly _IReflectionTypeInfo @class;
		readonly IReadOnlyDictionary<Type, object> collectionFixtureMappings;
		readonly IMessageBus messageBus;
		readonly ITestCaseOrderer testCaseOrderer;
		readonly IReadOnlyCollection<IXunitTestCase> testCases;
		readonly _ITestClass testClass;

		public List<object?[]> ConstructorArguments = new();
		public Exception? RunTestMethodAsync_AggregatorResult;
		public Dictionary<Type, object>? RunTestMethodsAsync_ClassFixtureMappings;
		public ITestCaseOrderer? RunTestMethodsAsync_TestCaseOrderer;

		TestableXunitTestClassRunner(
			_ITestClass testClass,
			_IReflectionTypeInfo @class,
			IReadOnlyCollection<IXunitTestCase> testCases,
			IMessageBus messageBus,
			ITestCaseOrderer testCaseOrderer,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource,
			IReadOnlyDictionary<Type, object> assemblyFixtureMappings,
			IReadOnlyDictionary<Type, object> collectionFixtureMappings)
		{
			this.testClass = testClass;
			this.@class = @class;
			this.testCases = testCases;
			this.messageBus = messageBus;
			this.testCaseOrderer = testCaseOrderer;
			this.aggregator = aggregator;
			this.cancellationTokenSource = cancellationTokenSource;
			this.assemblyFixtureMappings = assemblyFixtureMappings;
			this.collectionFixtureMappings = collectionFixtureMappings;
		}

		public static TestableXunitTestClassRunner Create(
			IXunitTestCase testCase,
			params object[] collectionFixtures) =>
				new(
					testCase.TestClass ?? throw new InvalidOperationException("testCase.TestClass cannot be null"),
					testCase.TestClass.Class as _IReflectionTypeInfo ?? throw new InvalidOperationException("testCase.TestClass.Class must be based on reflection"),
					new[] { testCase },
					new SpyMessageBus(),
					new MockTestCaseOrderer(),
					new ExceptionAggregator(),
					new CancellationTokenSource(),
					new Dictionary<Type, object>(),
					collectionFixtures.ToDictionary(fixture => fixture.GetType())
				);

		protected override ValueTask<RunSummary> RunTestMethodsAsync(XunitTestClassRunnerContext ctxt)
		{
			var result = base.RunTestMethodsAsync(ctxt);

			RunTestMethodsAsync_ClassFixtureMappings = ctxt.ClassFixtureMappings;
			RunTestMethodsAsync_TestCaseOrderer = ctxt.TestCaseOrderer;

			return result;
		}

		public ValueTask<RunSummary> RunAsync() =>
			RunAsync(testClass, @class, testCases, ExplicitOption.Off, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, assemblyFixtureMappings, collectionFixtureMappings);

		protected override ValueTask<RunSummary> RunTestMethodAsync(
			XunitTestClassRunnerContext ctxt,
			_ITestMethod? testMethod,
			_IReflectionMethodInfo? method,
			IReadOnlyCollection<IXunitTestCase> testCases,
			object?[] constructorArguments)
		{
			ConstructorArguments.Add(constructorArguments);
			RunTestMethodAsync_AggregatorResult = ctxt.Aggregator.ToException();

			return new(new RunSummary());
		}
	}
}
