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
	public static async void ClassCannotBeDecoratedWithICollectionFixture()
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
	public static async void TestClassCannotHaveMoreThanOneConstructor()
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
	public static async void TestClassCanHavePublicAndPrivateConstructor()
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
	public static async void TestClassCanHaveStaticConstructor()
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
	public static async void CreatesFixturesFromClassAndCollection()
	{
		var collection = new TestCollection(Mocks.TestAssembly(), Reflector.Wrap(typeof(CollectionUnderTest)), "Mock Test Collection");
		var testCase = TestData.XunitTestCase<ClassUnderTest>("Passing", collection);
		var runner = TestableXunitTestClassRunner.Create(testCase);

		await runner.RunAsync();

		Assert.Collection(
			runner.ClassFixtureMappings.OrderBy(mapping => mapping.Key.Name),
			mapping => Assert.IsType<FixtureUnderTest>(mapping.Value),
			mapping => Assert.IsType<object>(mapping.Value)
		);
	}

	[Fact]
	public static async void DisposesFixtures()
	{
		var testCase = TestData.XunitTestCase<ClassUnderTest>("Passing");
		var runner = TestableXunitTestClassRunner.Create(testCase);

		await runner.RunAsync();

		var fixtureUnderTest = runner.ClassFixtureMappings.Values.OfType<FixtureUnderTest>().Single();
		Assert.True(fixtureUnderTest.Disposed);
	}

	[Fact]
	public static async void DisposeAndAsyncDisposableShouldBeCalledInTheRightOrder()
	{
		var testCase = TestData.XunitTestCase<TestClassForFixtureAsyncDisposableUnderTest>("Passing");
		var runner = TestableXunitTestClassRunner.Create(testCase);

		var runnerSessionTask = runner.RunAsync();

		await Task.Delay(500);

		var fixtureUnderTest = runner.ClassFixtureMappings.Values.OfType<FixtureAsyncDisposableUnderTest>().Single();

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

		public TaskCompletionSource<bool> DisposeAsyncSignaler = new TaskCompletionSource<bool>();

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
	public static async void MultiplePublicConstructorsOnClassFixture_ReturnsError()
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
	public static async void UnresolvedConstructorParameterOnClassFixture_ReturnsError()
	{
		var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithDependency>("Passing");
		var runner = TestableXunitTestClassRunner.Create(testCase);

		await runner.RunAsync();

		var ex = Assert.IsType<TestClassException>(runner.RunTestMethodAsync_AggregatorResult);
		Assert.Equal("Class fixture type 'XunitTestClassRunnerTests+ClassFixtureWithCollectionFixtureDependency' had one or more unresolved constructor arguments: DependentCollectionFixture collectionFixture", ex.Message);
	}

	[Fact]
	public static async void CanInjectCollectionFixtureIntoClassFixture()
	{
		var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithDependency>("Passing");
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
		var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithMessageSinkDependency>("Passing");
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
		var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithMessageSinkDependency>("Passing");
		var runner = TestableXunitTestClassRunner.Create(testCase);

		await runner.RunAsync();

		var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<_DiagnosticMessage>());
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
		public static async void UsesCustomTestOrderer()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>("Passing");
			var runner = TestableXunitTestClassRunner.Create(testCase);

			await runner.RunAsync();

			Assert.IsType<CustomTestCaseOrderer>(runner.TestCaseOrderer);
		}

		[Fact]
		public static async void SettingUnknownTestCaseOrderLogsDiagnosticMessage()
		{
			var testCase = TestData.XunitTestCase<TestClassWithUnknownTestCaseOrderer>("Passing");
			var runner = TestableXunitTestClassRunner.Create(testCase);

			await runner.RunAsync();

			Assert.IsType<MockTestCaseOrderer>(runner.TestCaseOrderer);
			var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<_DiagnosticMessage>());
			Assert.Equal("Could not find type 'UnknownType' in UnknownAssembly for class-level test case orderer on test class 'XunitTestClassRunnerTests+TestCaseOrderer+TestClassWithUnknownTestCaseOrderer'", diagnosticMessage.Message);
		}

		[TestCaseOrderer("UnknownType", "UnknownAssembly")]
		class TestClassWithUnknownTestCaseOrderer
		{
			[Fact]
			public void Passing() { }
		}

		[CulturedFact("en-US")]
		public static async void SettingTestCaseOrdererWithThrowingConstructorLogsDiagnosticMessage()
		{
			var testCase = TestData.XunitTestCase<TestClassWithCtorThrowingTestCaseOrder>("Passing");
			var runner = TestableXunitTestClassRunner.Create(testCase);

			await runner.RunAsync();

			Assert.IsType<MockTestCaseOrderer>(runner.TestCaseOrderer);
			var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<_DiagnosticMessage>());
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
				where TTestCase : _ITestCase
					=> Array.Empty<TTestCase>();
		}
	}

	[Fact]
	public static async void PassesFixtureValuesToConstructor()
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
		public ClassUnderTest(FixtureUnderTest x, string y, decimal z) { }

		[Fact]
		public void Passing() { }
	}

	class CustomTestCaseOrderer : ITestCaseOrderer
	{
		public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
			where TTestCase : _ITestCase
				=> testCases;
	}

	class TestableXunitTestClassRunner : XunitTestClassRunner
	{
		public List<object?[]> ConstructorArguments = new List<object?[]>();
		public List<_MessageSinkMessage> DiagnosticMessages;
		public Exception? RunTestMethodAsync_AggregatorResult;

		TestableXunitTestClassRunner(
			_ITestClass testClass,
			_IReflectionTypeInfo @class,
			IReadOnlyCollection<IXunitTestCase> testCases,
			List<_MessageSinkMessage> diagnosticMessages,
			IMessageBus messageBus,
			ITestCaseOrderer testCaseOrderer,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource,
			IDictionary<Type, object> collectionFixtureMappings)
				: base(testClass, @class, testCases, SpyMessageSink.Create(messages: diagnosticMessages), messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings)
		{
			DiagnosticMessages = diagnosticMessages;
		}

		public new Dictionary<Type, object> ClassFixtureMappings => base.ClassFixtureMappings;

		public new ITestCaseOrderer TestCaseOrderer => base.TestCaseOrderer;

		public new _IMessageSink DiagnosticMessageSink => base.DiagnosticMessageSink;

		public static TestableXunitTestClassRunner Create(IXunitTestCase testCase, params object[] collectionFixtures) =>
			new TestableXunitTestClassRunner(
				testCase.TestMethod.TestClass,
				(_IReflectionTypeInfo)testCase.TestMethod.TestClass.Class,
				new[] { testCase },
				new List<_MessageSinkMessage>(),
				new SpyMessageBus(),
				new MockTestCaseOrderer(),
				new ExceptionAggregator(),
				new CancellationTokenSource(),
				collectionFixtures.ToDictionary(fixture => fixture.GetType())
			);

		protected override Task<RunSummary> RunTestMethodAsync(
			_ITestMethod testMethod,
			_IReflectionMethodInfo method,
			IReadOnlyCollection<IXunitTestCase> testCases,
			object?[] constructorArguments)
		{
			ConstructorArguments.Add(constructorArguments);
			RunTestMethodAsync_AggregatorResult = Aggregator.ToException();

			return Task.FromResult(new RunSummary());
		}
	}
}
