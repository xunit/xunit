using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestCollectionRunnerTests
{
	public class Messages
	{
		[Fact]
		public static async ValueTask Passing()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageBus.Messages,
				msg =>
				{
					var starting = Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false);
					verifyTestCollectionMessage(starting);
					Assert.Equal(typeof(ClassUnderTestCollection).SafeName(), starting.TestCollectionClassName);
					Assert.Equal("ClassUnderTest Collection", starting.TestCollectionDisplayName);
					// Trait comes from an assembly-level trait attribute on this ITest assembly
					var trait = Assert.Single(starting.Traits);
					Assert.Equal("Assembly", trait.Key);
					var value = Assert.Single(trait.Value);
					Assert.Equal("Trait", value);
				},
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => verifyTestCollectionMessage(Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false))
			);

			static void verifyTestCollectionMessage(ITestCollectionMessage message)
			{
				Assert.Equal("assembly-id", message.AssemblyUniqueID);
				Assert.Equal("test-collection-id", message.TestCollectionUniqueID);
			}
		}

		[Fact]
		public static async ValueTask StaticPassing()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.StaticPassing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask Failed()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Failing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal(-1, failed.ExceptionParentIndices.Single());
					Assert.Equal("Xunit.Sdk.TrueException", failed.ExceptionTypes.Single());
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaAttribute()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaAttribute));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				// ...no invocation since it's skipped...
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("Don't run me", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaException()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaException));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("This isn't a good time", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaRegisteredException()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaRegisteredException));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("Dividing by zero is really tough", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask NotRun()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.ExplicitTest));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestNotRun>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
			);
		}

		[CollectionDefinition("ClassUnderTest Collection")]
		public class ClassUnderTestCollection
		{ }

		[Collection("ClassUnderTest Collection")]
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
		public static async ValueTask FixtureCreationFailure()
		{
			var testCase = TestData.XunitTestCase<TestClassForFixtureCreationgFailure>(nameof(TestClassForFixtureCreationgFailure.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				// ...no invocation because of the startup failure...
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal([-1, 0], failed.ExceptionParentIndices);
					Assert.Equal(new[] { typeof(TestPipelineException).SafeName(), typeof(DivideByZeroException).SafeName() }, failed.ExceptionTypes);
					Assert.Equal(new[] { $"Collection fixture type '{typeof(FixtureWithThrowingCtor).SafeName()}' threw in its constructor", "Attempted to divide by zero." }, failed.Messages);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
			);
		}

		class FixtureWithThrowingCtor
		{
			public FixtureWithThrowingCtor() => throw new DivideByZeroException();
		}

		[CollectionDefinition]
		public class TestCollectForFixtureCreationFailure : ICollectionFixture<FixtureWithThrowingCtor>
		{ }

		[Collection(typeof(TestCollectForFixtureCreationFailure))]
		class TestClassForFixtureCreationgFailure
		{
			[Fact]
			public void Passing() { }
		}

		[Fact]
		public static async ValueTask FixtureDisposeFailure()
		{
			var testCase = TestData.XunitTestCase<TestClassForFixtureCleanupFailure>(nameof(TestClassForFixtureCleanupFailure.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg =>
				{
					var failure = Assert.IsType<ITestCollectionCleanupFailure>(msg, exactMatch: false);
					Assert.Equal([-1, 0], failure.ExceptionParentIndices);
					Assert.Equal(new[] { typeof(TestPipelineException).SafeName(), typeof(DivideByZeroException).SafeName() }, failure.ExceptionTypes);
					Assert.Equal(new[] { $"Collection fixture type '{typeof(FixtureWithThrowingDispose).SafeName()}' threw in Dispose", "Attempted to divide by zero." }, failure.Messages);
				},
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
			);
		}

		class FixtureWithThrowingDispose : IDisposable
		{
			public void Dispose() => throw new DivideByZeroException();
		}

		[CollectionDefinition]
		public class TestCollectForFixtureCleanupFailure : ICollectionFixture<FixtureWithThrowingDispose>
		{ }

		[Collection(typeof(TestCollectForFixtureCleanupFailure))]
		class TestClassForFixtureCleanupFailure
		{
			[Fact]
			public void Passing() { }
		}
	}

	public class Fixtures
	{
		[Fact]
		public static async ValueTask CreatesFixtures()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.NotNull(runner.RunTestClasses_CollectionFixtures);
			var kvp = Assert.Single(runner.RunTestClasses_CollectionFixtures);
			Assert.Equal(typeof(FixtureUnderTest), kvp.Key);
		}

		[Fact]
		public static async ValueTask DisposesFixtures()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.NotNull(runner.RunTestClasses_CollectionFixtures);
			var fixture = Assert.Single(runner.RunTestClasses_CollectionFixtures.Select(kvp => kvp.Value).OfType<FixtureUnderTest>());
			Assert.True(fixture.Disposed);
		}

		[Fact]
		public static async ValueTask IAsyncDisposableIsPreferredOverIDisposable()
		{
			var testCase = TestData.XunitTestCase<TestClassForFixtureAsyncDisposableUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.NotNull(runner.RunTestClasses_CollectionFixtures);
			var fixture = Assert.Single(runner.RunTestClasses_CollectionFixtures.Select(kvp => kvp.Value).OfType<FixtureAsyncDisposableUnderTest>());
			Assert.True(fixture.AsyncDisposed);
			Assert.False(fixture.Disposed);
		}

		[CollectionDefinition]
		public class TestCollectionForFixtureAsyncDisposable : ICollectionFixture<FixtureAsyncDisposableUnderTest>
		{ }

		[Collection(typeof(TestCollectionForFixtureAsyncDisposable))]
		class TestClassForFixtureAsyncDisposableUnderTest
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
		public static async ValueTask MultiplePublicConstructorsOnCollectionFixture()
		{
			var testCase = TestData.XunitTestCase<TestClassWithMultiCtorClassFixture>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				// ...invocation happens here...
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal(typeof(TestPipelineException).SafeName(), Assert.Single(failed.ExceptionTypes));
					Assert.Equal($"Collection fixture type '{typeof(CollectionFixtureWithMultipleConstructors).SafeName()}' may only define a single public constructor.", Assert.Single(failed.Messages));
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
			);
		}

		class CollectionFixtureWithMultipleConstructors
		{
			public CollectionFixtureWithMultipleConstructors() { }
			public CollectionFixtureWithMultipleConstructors(int _) { }
		}

		[CollectionDefinition]
		public class TestCollectionWithMultiCtorFixture : ICollectionFixture<CollectionFixtureWithMultipleConstructors>
		{ }

		[Collection(typeof(TestCollectionWithMultiCtorFixture))]
		class TestClassWithMultiCtorClassFixture
		{
			[Fact]
			public void Passing() { }
		}

		[Fact]
		public static async ValueTask UnresolvedConstructorParameterOnCollectionFixture()
		{
			var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithDependency>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				// ...invocation happens here...
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal(typeof(TestPipelineException).SafeName(), Assert.Single(failed.ExceptionTypes));
					Assert.Equal($"Collection fixture type '{typeof(CollectionFixtureWithAssemblyFixtureDependency).SafeName()}' had one or more unresolved constructor arguments: {nameof(DependentAssemblyFixture)} assemblyFixture", Assert.Single(failed.Messages));
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask CanInjectAssemblyFixtureIntoCollectionFixture()
		{
			var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithDependency>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);
			await runner.AssemblyFixtureMappingManager.InitializeAsync(typeof(DependentAssemblyFixture));

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
			);
		}

		class DependentAssemblyFixture { }

		class CollectionFixtureWithAssemblyFixtureDependency(DependentAssemblyFixture assemblyFixture)
		{
			public DependentAssemblyFixture AssemblyFixture = assemblyFixture;
		}

		[CollectionDefinition]
		public class TestCollectionWithClassFixtureWithDependency : ICollectionFixture<CollectionFixtureWithAssemblyFixtureDependency>
		{ }

		[Collection(typeof(TestCollectionWithClassFixtureWithDependency))]
		class TestClassWithClassFixtureWithDependency
		{
			[Fact]
			public void Passing() { }
		}

		[Fact]
		public static async ValueTask CanInjectMessageSinkIntoCollectionFixture()
		{
			var spy = SpyMessageSink.Capture();
			TestContextInternal.Current.DiagnosticMessageSink = spy;
			var testCase = TestData.XunitTestCase<TestClassWithCollectionFixtureWithMessageSinkDependency>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			var diagnosticMessage = Assert.Single(spy.Messages.OfType<IDiagnosticMessage>());
			Assert.Equal("CollectionFixtureWithMessageSinkDependency constructor message", diagnosticMessage.Message);
		}

		class CollectionFixtureWithMessageSinkDependency
		{
			public IMessageSink MessageSink;

			public CollectionFixtureWithMessageSinkDependency(IMessageSink messageSink)
			{
				MessageSink = messageSink;
				MessageSink.OnMessage(new DiagnosticMessage("CollectionFixtureWithMessageSinkDependency constructor message"));
			}
		}

		[CollectionDefinition]
		public class TestCollectionWithCollectionFixtureWithMessageSinkDependency : ICollectionFixture<CollectionFixtureWithMessageSinkDependency>
		{ }

		[Collection(typeof(TestCollectionWithCollectionFixtureWithMessageSinkDependency))]
		class TestClassWithCollectionFixtureWithMessageSinkDependency
		{
			[Fact]
			public void Passing() { }
		}
	}

	public class TestClassOrderer
	{
		[Theory]
		[InlineData(typeof(AssemblyLevel))]
		[InlineData(typeof(CollectionLevel))]
		public static async ValueTask UsesCustomOrderer(Type testClassType)
		{
			var testAssembly =
				testClassType == typeof(AssemblyLevel)
					? Mocks.XunitTestAssembly(testClassOrderer: new CustomTestClassOrderer())
					: TestData.XunitTestAssembly(testClassType.Assembly);
			var testCollection = new CollectionPerClassTestCollectionFactory(testAssembly).Get(testClassType);
			var testClass = TestData.XunitTestClass(testClassType, testCollection);
			var testMethod = TestData.XunitTestMethod(testClass, testClassType.GetMethod("Passing") ?? throw new InvalidOperationException("Passing method not found"));
			var testCase = TestData.XunitTestCase(testMethod);
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.IsType<CustomTestClassOrderer>(runner.RunTestClasses_TestClassOrderer);
		}

		[Fact]
		public static async ValueTask OrdersTestClasses()
		{
			var testAssembly = Mocks.XunitTestAssembly(testClassOrderer: UnorderedTestClassOrderer.Instance);
			var testCollection = Mocks.XunitTestCollection(testAssembly: testAssembly);
			var testClass1 = Mocks.XunitTestClass(testCollection: testCollection, testClassName: "test-class-1");
			var testCase1 = testCaseForClass(testClass1, "test-case-1");
			var testClass2 = Mocks.XunitTestClass(testCollection: testCollection, testClassName: "test-class-2");
			var testCase2 = testCaseForClass(testClass2, "test-case-2");
			var testClass3 = Mocks.XunitTestClass(testCollection: testCollection, testClassName: "test-class-3");
			var testCase3 = testCaseForClass(testClass3, "test-case-3");
			var runner = new TestableXunitTestCollectionRunner(testCase3, testCase1, testCase2);

			await runner.RunAsync();

			Assert.IsType<UnorderedTestClassOrderer>(runner.RunTestClasses_TestClassOrderer);
			Assert.Collection(
				runner.RunTestClass__ClassesRun,
				tc =>
				{
					Assert.Equal("test-class-3", tc.TestClass?.TestClassName);
					Assert.Equal(["test-case-3"], tc.TestCases.Select(tc => tc.TestCaseDisplayName));
				},
				tc =>
				{
					Assert.Equal("test-class-1", tc.TestClass?.TestClassName);
					Assert.Equal(["test-case-1"], tc.TestCases.Select(tc => tc.TestCaseDisplayName));
				},
				tc =>
				{
					Assert.Equal("test-class-2", tc.TestClass?.TestClassName);
					Assert.Equal(["test-case-2"], tc.TestCases.Select(tc => tc.TestCaseDisplayName));
				}
			);

			static IXunitTestCase testCaseForClass(
				IXunitTestClass testClass,
				string testCaseDisplayName) =>
					Mocks.XunitTestCase(testMethod: Mocks.XunitTestMethod(testClass: testClass), testCaseDisplayName: testCaseDisplayName);
		}

		[Fact]
		public static async ValueTask SettingOrdererWithThrowingConstructorLogsDiagnosticMessage()
		{
			var spy = SpyMessageSink.Capture();
			TestContextInternal.Current.DiagnosticMessageSink = spy;
			var testCase = TestData.XunitTestCase<TestClassWithCtorThrowingTestClassOrderer>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.IsType<DefaultTestClassOrderer>(runner.RunTestClasses_TestClassOrderer);
			var diagnosticMessage = Assert.Single(spy.Messages.Cast<IDiagnosticMessage>());
			Assert.StartsWith($"Collection-level test class orderer '{typeof(MyCtorThrowingTestClassOrderer).SafeName()}' for test collection '{typeof(TestCollectionWithCtorThrowingTestClassOrderer).SafeName()}' threw 'System.DivideByZeroException' during construction: Attempted to divide by zero.", diagnosticMessage.Message);
		}

		class AssemblyLevel  // Attribute injected via mock assembly
		{
			[Fact]
			public void Passing() { }
		}

		[TestClassOrderer(typeof(CustomTestClassOrderer))]
		public class CollectionLevelCollection { }

		[Collection(typeof(CollectionLevelCollection))]
		class CollectionLevel
		{
			[Fact]
			public void Passing() { }
		}

		class CustomTestClassOrderer : ITestClassOrderer
		{
			public IReadOnlyCollection<TTestClass?> OrderTestClasses<TTestClass>(IReadOnlyCollection<TTestClass?> testClasses)
				where TTestClass : notnull, ITestClass =>
					testClasses;
		}

		[CollectionDefinition]
		[TestClassOrderer(typeof(MyCtorThrowingTestClassOrderer))]
		public class TestCollectionWithCtorThrowingTestClassOrderer
		{ }

		[Collection(typeof(TestCollectionWithCtorThrowingTestClassOrderer))]
		class TestClassWithCtorThrowingTestClassOrderer
		{
			[Fact]
			public void Passing() { }
		}

		class MyCtorThrowingTestClassOrderer : ITestClassOrderer
		{
			public MyCtorThrowingTestClassOrderer()
			{
				throw new DivideByZeroException();
			}

			public IReadOnlyCollection<TTestClass?> OrderTestClasses<TTestClass>(IReadOnlyCollection<TTestClass?> testClasses)
				where TTestClass : notnull, ITestClass =>
					[];
		}
	}

	class FixtureUnderTest : IDisposable
	{
		public bool Disposed;

		public void Dispose() => Disposed = true;
	}

	[CollectionDefinition]
	public class CollectionUnderTest : ICollectionFixture<FixtureUnderTest> { }

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources

	[Collection(typeof(CollectionUnderTest))]
	class ClassUnderTest(FixtureUnderTest _1, string _2, decimal _3)
	{
		[Fact]
		public void Passing() { }
	}

#pragma warning restore xUnit1041

	class TestableXunitTestCollectionRunner(params IXunitTestCase[] testCases) :
		XunitTestCollectionRunner
	{
		public readonly ExceptionAggregator Aggregator = new();
		public FixtureMappingManager AssemblyFixtureMappingManager = new("[Unit Test] Test Assembly");
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public readonly SpyMessageBus MessageBus = new();

		public ValueTask<RunSummary> RunAsync() =>
			Run(
				testCases[0].TestCollection,
				testCases,
				ExplicitOption.Off,
				MessageBus,
				DefaultTestClassOrderer.Instance,
				DefaultTestMethodOrderer.Instance,
				DefaultTestCaseOrderer.Instance,
				Aggregator,
				CancellationTokenSource,
				AssemblyFixtureMappingManager
			);

		public List<(IXunitTestClass? TestClass, IReadOnlyCollection<IXunitTestCase> TestCases)> RunTestClass__ClassesRun = [];

		protected override ValueTask<RunSummary> RunTestClass(
			XunitTestCollectionRunnerContext ctxt,
			IXunitTestClass? testClass,
			IReadOnlyCollection<IXunitTestCase> testCases)
		{
			RunTestClass__ClassesRun.Add((testClass, testCases));

			return base.RunTestClass(ctxt, testClass, testCases);
		}

		public IReadOnlyDictionary<Type, object>? RunTestClasses_CollectionFixtures = null;
		public ITestClassOrderer? RunTestClasses_TestClassOrderer = null;

		protected override ValueTask<RunSummary> RunTestClasses(
			XunitTestCollectionRunnerContext ctxt,
			Exception? exception)
		{
			RunTestClasses_CollectionFixtures = ctxt.CollectionFixtureMappings.GetFixtureCache();
			RunTestClasses_TestClassOrderer = ctxt.TestClassOrderer;

			return base.RunTestClasses(ctxt, exception);
		}
	}
}
