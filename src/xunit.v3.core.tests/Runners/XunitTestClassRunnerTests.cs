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
	public class Messages
	{
		[Fact]
		public static async ValueTask Passing()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageBus.Messages,
				msg =>
				{
					var starting = Assert.IsAssignableFrom<ITestClassStarting>(msg);
					verifyTestClassMessage(starting);
					Assert.Equal(typeof(ClassUnderTest).SafeName(), starting.TestClassName);
					Assert.Null(starting.TestClassNamespace);
					Assert.Equal(typeof(ClassUnderTest).ToSimpleName(), starting.TestClassSimpleName);
					// Trait comes from an assembly-level trait attribute on this ITest assembly
					var trait = Assert.Single(starting.Traits);
					Assert.Equal("Assembly", trait.Key);
					var value = Assert.Single(trait.Value);
					Assert.Equal("Trait", value);
				},
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg),
				// ...invocation happens here...
				msg => Assert.IsAssignableFrom<ITestClassDisposeStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassDisposeFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestPassed>(msg),
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => verifyTestClassMessage(Assert.IsAssignableFrom<ITestClassFinished>(msg))
			);

			static void verifyTestClassMessage(ITestClassMessage message)
			{
				Assert.Equal("assembly-id", message.AssemblyUniqueID);
				Assert.Equal("test-class-id", message.TestClassUniqueID);
				Assert.Equal("test-collection-id", message.TestCollectionUniqueID);
			}
		}

		[Fact]
		public static async ValueTask StaticPassing()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.StaticPassing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				// ...invocation happens here...
				msg => Assert.IsAssignableFrom<ITestPassed>(msg),
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask Failed()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Failing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg),
				// ...invocation happens here...
				msg => Assert.IsAssignableFrom<ITestClassDisposeStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassDisposeFinished>(msg),
				msg =>
				{
					var failed = Assert.IsAssignableFrom<ITestFailed>(msg);
					Assert.Equal(-1, failed.ExceptionParentIndices.Single());
					Assert.Equal("Xunit.Sdk.TrueException", failed.ExceptionTypes.Single());
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaAttribute()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaAttribute));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				// ...no invocation since it's skipped...
				msg =>
				{
					var skipped = Assert.IsAssignableFrom<ITestSkipped>(msg);
					Assert.Equal("Don't run me", skipped.Reason);
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaException()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaException));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg),
				// ...invocation happens here...
				msg => Assert.IsAssignableFrom<ITestClassDisposeStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassDisposeFinished>(msg),
				msg =>
				{
					var skipped = Assert.IsAssignableFrom<ITestSkipped>(msg);
					Assert.Equal("This isn't a good time", skipped.Reason);
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaRegisteredException()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaRegisteredException));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg),
				// ...invocation happens here...
				msg => Assert.IsAssignableFrom<ITestClassDisposeStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassDisposeFinished>(msg),
				msg =>
				{
					var skipped = Assert.IsAssignableFrom<ITestSkipped>(msg);
					Assert.Equal("Dividing by zero is really tough", skipped.Reason);
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask NotRun()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.ExplicitTest));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestNotRun>(msg),
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg)
			);
		}

		class ClassUnderTest : IDisposable
		{
			public void Dispose() { }

			[Fact]
			public async Task Passing() => await Task.Yield();

			[Fact]
			public static void StaticPassing() { }

			[Fact]
			public void Failing() => Assert.True(false);

			[Fact(Skip = "Don't run me")]
			public void SkippedViaAttribute() { }

			[Fact]
			public void SkippedViaException() => Assert.Skip("This isn't a good time");

			[Fact(SkipExceptions = [typeof(DivideByZeroException)])]
			public void SkippedViaRegisteredException() => throw new DivideByZeroException("Dividing by zero is really tough");

			[Fact(Explicit = true)]
			public void ExplicitTest() => Assert.Fail("Should not run");
		}

		[Fact]
		public static async ValueTask ClassWithCollectionFixture()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTestWithCollectionFixture>(nameof(ClassUnderTestWithCollectionFixture.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg =>
				{
					var failed = Assert.IsAssignableFrom<ITestFailed>(msg);
					Assert.Equal(typeof(TestPipelineException).SafeName(), Assert.Single(failed.ExceptionTypes));
					Assert.Equal("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead).", Assert.Single(failed.Messages));
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg)
			);
		}

		class ClassUnderTestWithCollectionFixture : ICollectionFixture<object>
		{
			[Fact]
			public void Passing() { }
		}

		[Fact]
		public static async ValueTask ClassWithMultiplePublicConstructors()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTestWithMultiplePublicConstructors>(nameof(ClassUnderTestWithMultiplePublicConstructors.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg =>
				{
					var failed = Assert.IsAssignableFrom<ITestFailed>(msg);
					Assert.Equal(typeof(TestPipelineException).SafeName(), Assert.Single(failed.ExceptionTypes));
					Assert.Equal("A test class may only define a single public constructor.", Assert.Single(failed.Messages));
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg)
			);
		}

		class ClassUnderTestWithMultiplePublicConstructors
		{
			public ClassUnderTestWithMultiplePublicConstructors() { }
			public ClassUnderTestWithMultiplePublicConstructors(int _) { }


			[Fact]
			public void Passing() { }
		}

		[Fact]
		public static async ValueTask ClassWithMixedConstructors()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTestWithMixedConstructors>(nameof(ClassUnderTestWithMixedConstructors.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Contains(runner.MessageBus.Messages, m => m is ITestPassed);
			Assert.DoesNotContain(runner.MessageBus.Messages, m => m is ITestClassCleanupFailure);
		}

		class ClassUnderTestWithMixedConstructors
		{
			static ClassUnderTestWithMixedConstructors() { }
			public ClassUnderTestWithMixedConstructors() { }
			protected ClassUnderTestWithMixedConstructors(int _) { }

			[Fact]
			public void Passing() { }
		}
	}

	public class Fixtures
	{
		[Fact]
		public static async ValueTask CreatesFixturesFromClassAndCollection()
		{
			var assembly = Mocks.XunitTestAssembly();
			var collection = TestData.XunitTestCollection(assembly, typeof(CollectionUnderTest));
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing), testCollection: collection);
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.NotNull(runner.RunTestMethodsAsync_ClassFixtures);
			Assert.Collection(
				runner.RunTestMethodsAsync_ClassFixtures.OrderBy(kvp => kvp.Key.SafeName()),
				kvp => Assert.Equal(typeof(object), kvp.Key),
				kvp => Assert.Equal(typeof(FixtureUnderTest), kvp.Key)
			);
		}

		[Fact]
		public static async ValueTask DisposesFixtures()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.NotNull(runner.RunTestMethodsAsync_ClassFixtures);
			var fixture = Assert.Single(runner.RunTestMethodsAsync_ClassFixtures.Select(kvp => kvp.Value).OfType<FixtureUnderTest>());
			Assert.True(fixture.Disposed);
		}

		[Fact]
		public static async ValueTask IAsyncDisposableIsPreferredOverIDisposable()
		{
			var testCase = TestData.XunitTestCase<TestClassForFixtureAsyncDisposableUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.NotNull(runner.RunTestMethodsAsync_ClassFixtures);
			var fixture = Assert.Single(runner.RunTestMethodsAsync_ClassFixtures.Select(kvp => kvp.Value).OfType<FixtureAsyncDisposableUnderTest>());
			Assert.True(fixture.AsyncDisposed);
			Assert.False(fixture.Disposed);
		}

		class TestClassForFixtureAsyncDisposableUnderTest : IClassFixture<FixtureAsyncDisposableUnderTest>
		{
			[Fact]
			public void Passing() { }
		}

		class FixtureAsyncDisposableUnderTest : IAsyncDisposable, IDisposable
		{
			public bool AsyncDisposed;
			public bool Disposed;

			public void Dispose() => Disposed = true;

			public ValueTask DisposeAsync()
			{
				AsyncDisposed = true;
				return default;
			}
		}

		[Fact]
		public static async ValueTask MultiplePublicConstructorsOnClassFixture()
		{
			var testCase = TestData.XunitTestCase<TestClassWithMultiCtorClassFixture>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg =>
				{
					var failed = Assert.IsAssignableFrom<ITestFailed>(msg);
					Assert.Equal(typeof(TestPipelineException).SafeName(), Assert.Single(failed.ExceptionTypes));
					Assert.Equal($"Class fixture type '{typeof(ClassFixtureWithMultipleConstructors).SafeName()}' may only define a single public constructor.", Assert.Single(failed.Messages));
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg)
			);
		}

		class ClassFixtureWithMultipleConstructors
		{
			public ClassFixtureWithMultipleConstructors() { }
			public ClassFixtureWithMultipleConstructors(int _) { }
		}

		class TestClassWithMultiCtorClassFixture : IClassFixture<ClassFixtureWithMultipleConstructors>
		{
			[Fact]
			public void Passing() { }
		}

		[Fact]
		public static async ValueTask UnresolvedConstructorParameterOnClassFixture()
		{
			var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithDependency>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg =>
				{
					var failed = Assert.IsAssignableFrom<ITestFailed>(msg);
					Assert.Equal(typeof(TestPipelineException).SafeName(), Assert.Single(failed.ExceptionTypes));
					Assert.Equal($"Class fixture type '{typeof(ClassFixtureWithCollectionFixtureDependency).SafeName()}' had one or more unresolved constructor arguments: {nameof(DependentCollectionFixture)} collectionFixture", Assert.Single(failed.Messages));
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask CanInjectCollectionFixtureIntoClassFixture()
		{
			var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithDependency>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);
			await runner.CollectionFixtureMappingManager.InitializeAsync(typeof(DependentCollectionFixture));

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg),
				// ...invocation happens here...
				msg => Assert.IsAssignableFrom<ITestPassed>(msg),
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg)
			);
		}

		class DependentCollectionFixture { }

		class ClassFixtureWithCollectionFixtureDependency(DependentCollectionFixture collectionFixture)
		{
			public DependentCollectionFixture CollectionFixture = collectionFixture;
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
			TestContext.CurrentInternal.DiagnosticMessageSink = spy;
			var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithMessageSinkDependency>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			var diagnosticMessage = Assert.Single(spy.Messages.OfType<IDiagnosticMessage>());
			Assert.Equal("ClassFixtureWithMessageSinkDependency constructor message", diagnosticMessage.Message);
		}

		class ClassFixtureWithMessageSinkDependency
		{
			public IMessageSink MessageSink;

			public ClassFixtureWithMessageSinkDependency(IMessageSink messageSink)
			{
				MessageSink = messageSink;
				MessageSink.OnMessage(new DiagnosticMessage("ClassFixtureWithMessageSinkDependency constructor message"));
			}
		}

		class TestClassWithClassFixtureWithMessageSinkDependency : IClassFixture<ClassFixtureWithMessageSinkDependency>
		{
			[Fact]
			public void Passing() { }
		}

		[Fact]
		public static async ValueTask PassesFixtureValuesToConstructor()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestClassRunner(testCase) { CollectionFixtureMappingManager = new TestableFixtureMappingManager(42, "Hello, world!", 21.12m) };

			await runner.RunAsync();

			Assert.NotNull(runner.CreateTestClassConstructorArguments_ConstructorArguments);
			Assert.Collection(
				runner.CreateTestClassConstructorArguments_ConstructorArguments,
				arg => Assert.IsType<FixtureUnderTest>(arg),
				arg => Assert.Equal("Hello, world!", arg),
				arg => Assert.Equal(21.12m, arg)
			);
		}
	}

	public class TestCaseOrderer
	{
		[Fact]
		public static async ValueTask UsesCustomTestOrderer()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.IsType<CustomTestCaseOrderer>(runner.RunTestMethodsAsync_TestCaseOrderer);
		}

		[Fact]
		public static async ValueTask SettingTestCaseOrdererWithThrowingConstructorLogsDiagnosticMessage()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.CurrentInternal.DiagnosticMessageSink = spy;
			var testCase = TestData.XunitTestCase<TestClassWithCtorThrowingTestCaseOrder>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			var diagnosticMessage = Assert.Single(spy.Messages.Cast<IDiagnosticMessage>());
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
				where TTestCase : notnull, ITestCase
					=> [];
		}
	}

	class FixtureUnderTest : IDisposable
	{
		public bool Disposed;

		public void Dispose() => Disposed = true;
	}

	class CollectionUnderTest : IClassFixture<object> { }

	[TestCaseOrderer(typeof(CustomTestCaseOrderer))]
	class ClassUnderTest : IClassFixture<FixtureUnderTest>
	{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
		public ClassUnderTest(FixtureUnderTest _1, string _2, decimal _3) { }
#pragma warning restore xUnit1041

		[Fact]
		public void Passing() { }
	}

	class CustomTestCaseOrderer : ITestCaseOrderer
	{
		public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
			where TTestCase : notnull, ITestCase
				=> testCases;
	}

	class TestableXunitTestClassRunner(IXunitTestCase testCase) :
		XunitTestClassRunner
	{
		public readonly ExceptionAggregator Aggregator = new();
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public FixtureMappingManager CollectionFixtureMappingManager = new("[Unit Test] Test Collection");
		public readonly SpyMessageBus MessageBus = new();

		public ValueTask<RunSummary> RunAsync() =>
			Run(testCase.TestClass, [testCase], ExplicitOption.Off, MessageBus, DefaultTestCaseOrderer.Instance, Aggregator, CancellationTokenSource, CollectionFixtureMappingManager);

		public object?[]? CreateTestClassConstructorArguments_ConstructorArguments;

		protected override async ValueTask<object?[]> CreateTestClassConstructorArguments(XunitTestClassRunnerContext ctxt)
		{
			CreateTestClassConstructorArguments_ConstructorArguments = await base.CreateTestClassConstructorArguments(ctxt);
			return CreateTestClassConstructorArguments_ConstructorArguments;
		}

		public IReadOnlyDictionary<Type, object>? RunTestMethodsAsync_ClassFixtures;
		public ITestCaseOrderer? RunTestMethodsAsync_TestCaseOrderer;

		protected override ValueTask<RunSummary> RunTestMethods(
			XunitTestClassRunnerContext ctxt,
			Exception? exception)
		{
			RunTestMethodsAsync_ClassFixtures = ctxt.ClassFixtureMappings.FixtureCache;
			RunTestMethodsAsync_TestCaseOrderer = ctxt.TestCaseOrderer;

			return base.RunTestMethods(ctxt, exception);
		}
	}
}
