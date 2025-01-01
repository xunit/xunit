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
					var starting = Assert.IsAssignableFrom<ITestCollectionStarting>(msg);
					verifyTestCollectionMessage(starting);
					Assert.Equal(typeof(ClassUnderTestCollection).SafeName(), starting.TestCollectionClassName);
					Assert.Equal("ClassUnderTest Collection", starting.TestCollectionDisplayName);
					// Trait comes from an assembly-level trait attribute on this ITest assembly
					var trait = Assert.Single(starting.Traits);
					Assert.Equal("Assembly", trait.Key);
					var value = Assert.Single(trait.Value);
					Assert.Equal("Trait", value);
				},
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
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
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg),
				msg => verifyTestCollectionMessage(Assert.IsAssignableFrom<ITestCollectionFinished>(msg))
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
				msg => Assert.IsAssignableFrom<ITestCollectionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				// ...invocation happens here...
				msg => Assert.IsAssignableFrom<ITestPassed>(msg),
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCollectionFinished>(msg)
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
				msg => Assert.IsAssignableFrom<ITestCollectionStarting>(msg),
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
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCollectionFinished>(msg)
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
				msg => Assert.IsAssignableFrom<ITestCollectionStarting>(msg),
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
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCollectionFinished>(msg)
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
				msg => Assert.IsAssignableFrom<ITestCollectionStarting>(msg),
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
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCollectionFinished>(msg)
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
				msg => Assert.IsAssignableFrom<ITestCollectionStarting>(msg),
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
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCollectionFinished>(msg)
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
				msg => Assert.IsAssignableFrom<ITestCollectionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestNotRun>(msg),
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCollectionFinished>(msg)
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
				msg => Assert.IsAssignableFrom<ITestCollectionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				// ...no invocation because of the startup failure...
				msg =>
				{
					var failed = Assert.IsAssignableFrom<ITestFailed>(msg);
					Assert.Equal(new[] { -1, 0 }, failed.ExceptionParentIndices);
					Assert.Equal(new[] { typeof(TestPipelineException).SafeName(), typeof(DivideByZeroException).SafeName() }, failed.ExceptionTypes);
					Assert.Equal(new[] { $"Collection fixture type '{typeof(FixtureWithThrowingCtor).SafeName()}' threw in its constructor", "Attempted to divide by zero." }, failed.Messages);
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCollectionFinished>(msg)
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
				msg => Assert.IsAssignableFrom<ITestCollectionStarting>(msg),
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
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCollectionFinished>(msg),
				msg =>
				{
					var failure = Assert.IsAssignableFrom<ITestCollectionCleanupFailure>(msg);
					Assert.Equal(new[] { -1, 0 }, failure.ExceptionParentIndices);
					Assert.Equal(new[] { typeof(TestPipelineException).SafeName(), typeof(DivideByZeroException).SafeName() }, failure.ExceptionTypes);
					Assert.Equal(new[] { $"Collection fixture type '{typeof(FixtureWithThrowingDispose).SafeName()}' threw in Dispose", "Attempted to divide by zero." }, failure.Messages);
				}
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

			Assert.NotNull(runner.RunTestClassesAsync_CollectionFixtures);
			var kvp = Assert.Single(runner.RunTestClassesAsync_CollectionFixtures);
			Assert.Equal(typeof(FixtureUnderTest), kvp.Key);
		}

		[Fact]
		public static async ValueTask DisposesFixtures()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.NotNull(runner.RunTestClassesAsync_CollectionFixtures);
			var fixture = Assert.Single(runner.RunTestClassesAsync_CollectionFixtures.Select(kvp => kvp.Value).OfType<FixtureUnderTest>());
			Assert.True(fixture.Disposed);
		}

		[Fact]
		public static async ValueTask IAsyncDisposableIsPreferredOverIDisposable()
		{
			var testCase = TestData.XunitTestCase<TestClassForFixtureAsyncDisposableUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.NotNull(runner.RunTestClassesAsync_CollectionFixtures);
			var fixture = Assert.Single(runner.RunTestClassesAsync_CollectionFixtures.Select(kvp => kvp.Value).OfType<FixtureAsyncDisposableUnderTest>());
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
				msg => Assert.IsAssignableFrom<ITestCollectionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				// ...invocation happens here...
				msg =>
				{
					var failed = Assert.IsAssignableFrom<ITestFailed>(msg);
					Assert.Equal(typeof(TestPipelineException).SafeName(), Assert.Single(failed.ExceptionTypes));
					Assert.Equal($"Collection fixture type '{typeof(CollectionFixtureWithMultipleConstructors).SafeName()}' may only define a single public constructor.", Assert.Single(failed.Messages));
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCollectionFinished>(msg)
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
				msg => Assert.IsAssignableFrom<ITestCollectionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				// ...invocation happens here...
				msg =>
				{
					var failed = Assert.IsAssignableFrom<ITestFailed>(msg);
					Assert.Equal(typeof(TestPipelineException).SafeName(), Assert.Single(failed.ExceptionTypes));
					Assert.Equal($"Collection fixture type '{typeof(CollectionFixtureWithAssemblyFixtureDependency).SafeName()}' had one or more unresolved constructor arguments: {nameof(DependentAssemblyFixture)} assemblyFixture", Assert.Single(failed.Messages));
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCollectionFinished>(msg)
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
				msg => Assert.IsAssignableFrom<ITestCollectionStarting>(msg),
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
				msg => Assert.IsAssignableFrom<ITestClassFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCollectionFinished>(msg)
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
			TestContext.CurrentInternal.DiagnosticMessageSink = spy;
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

	public class TestCaseOrderer
	{
		[Fact]
		public static async ValueTask UsesCustomTestOrderer()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.IsType<CustomTestCaseOrderer>(runner.RunTestClassesAsync_TestCaseOrderer);
		}

		[Fact]
		public static async ValueTask SettingTestCaseOrdererWithThrowingConstructorLogsDiagnosticMessage()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.CurrentInternal.DiagnosticMessageSink = spy;
			var testCase = TestData.XunitTestCase<TestClassWithCtorThrowingTestCaseOrderer>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.IsType<DefaultTestCaseOrderer>(runner.RunTestClassesAsync_TestCaseOrderer);
			var diagnosticMessage = Assert.Single(spy.Messages.Cast<IDiagnosticMessage>());
			Assert.StartsWith($"Collection-level test case orderer '{typeof(MyCtorThrowingTestCaseOrderer).SafeName()}' for test collection '{typeof(TestCollectionWithCtorThrowingTestCaseOrderer).SafeName()}' threw 'System.DivideByZeroException' during construction: Attempted to divide by zero.", diagnosticMessage.Message);
		}

		[CollectionDefinition]
		[TestCaseOrderer(typeof(MyCtorThrowingTestCaseOrderer))]
		public class TestCollectionWithCtorThrowingTestCaseOrderer
		{ }

		[Collection(typeof(TestCollectionWithCtorThrowingTestCaseOrderer))]
		class TestClassWithCtorThrowingTestCaseOrderer
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
				where TTestCase : notnull, ITestCase =>
					[];
		}
	}

	class FixtureUnderTest : IDisposable
	{
		public bool Disposed;

		public void Dispose() => Disposed = true;
	}

	[CollectionDefinition]
	[TestCaseOrderer(typeof(CustomTestCaseOrderer))]
	public class CollectionUnderTest : ICollectionFixture<FixtureUnderTest> { }

	[Collection(typeof(CollectionUnderTest))]
	class ClassUnderTest
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

	class TestableXunitTestCollectionRunner(IXunitTestCase testCase) :
		XunitTestCollectionRunner
	{
		public readonly ExceptionAggregator Aggregator = new();
		public FixtureMappingManager AssemblyFixtureMappingManager = new("[Unit Test] Test Assembly");
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public readonly SpyMessageBus MessageBus = new();
		public ITestCaseOrderer TestCaseOrderer = DefaultTestCaseOrderer.Instance;

		public ValueTask<RunSummary> RunAsync() =>
			Run(testCase.TestCollection, [testCase], ExplicitOption.Off, MessageBus, TestCaseOrderer, Aggregator, CancellationTokenSource, AssemblyFixtureMappingManager);

		public Exception? RunTestClassAsync_AggregatorResult = null;
		public IReadOnlyDictionary<Type, object>? RunTestClassesAsync_CollectionFixtures = null;
		public ITestCaseOrderer? RunTestClassesAsync_TestCaseOrderer = null;

		protected override ValueTask<RunSummary> RunTestClasses(
			XunitTestCollectionRunnerContext ctxt,
			Exception? exception)
		{
			RunTestClassesAsync_CollectionFixtures = ctxt.CollectionFixtureMappings.FixtureCache;
			RunTestClassesAsync_TestCaseOrderer = ctxt.TestCaseOrderer;

			return base.RunTestClasses(ctxt, exception);
		}
	}
}
