using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Internal;
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
					var starting = Assert.IsType<ITestClassStarting>(msg, exactMatch: false);
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
				msg => verifyTestClassMessage(Assert.IsType<ITestClassFinished>(msg, exactMatch: false))
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
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
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
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
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
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
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
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
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
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
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
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestNotRun>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
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
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal(typeof(TestPipelineException).SafeName(), Assert.Single(failed.ExceptionTypes));
					Assert.Equal("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead).", Assert.Single(failed.Messages));
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
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
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal(typeof(TestPipelineException).SafeName(), Assert.Single(failed.ExceptionTypes));
					Assert.Equal("A test class may only define a single public constructor.", Assert.Single(failed.Messages));
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
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

			Assert.NotNull(runner.RunTestMethods_ClassFixtures);
			Assert.Collection(
				runner.RunTestMethods_ClassFixtures.OrderBy(kvp => kvp.Key.SafeName()),
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

			Assert.NotNull(runner.RunTestMethods_ClassFixtures);
			var fixture = Assert.Single(runner.RunTestMethods_ClassFixtures.Select(kvp => kvp.Value).OfType<FixtureUnderTest>());
			Assert.True(fixture.Disposed);
		}

		[Fact]
		public static async ValueTask IAsyncDisposableIsPreferredOverIDisposable()
		{
			var testCase = TestData.XunitTestCase<TestClassForFixtureAsyncDisposableUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.NotNull(runner.RunTestMethods_ClassFixtures);
			var fixture = Assert.Single(runner.RunTestMethods_ClassFixtures.Select(kvp => kvp.Value).OfType<FixtureAsyncDisposableUnderTest>());
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
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal(typeof(TestPipelineException).SafeName(), Assert.Single(failed.ExceptionTypes));
					Assert.Equal($"Class fixture type '{typeof(ClassFixtureWithMultipleConstructors).SafeName()}' may only define a single public constructor.", Assert.Single(failed.Messages));
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
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
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal(typeof(TestPipelineException).SafeName(), Assert.Single(failed.ExceptionTypes));
					Assert.Equal($"Class fixture type '{typeof(ClassFixtureWithCollectionFixtureDependency).SafeName()}' had one or more unresolved constructor arguments: {nameof(DependentCollectionFixture)} collectionFixture", Assert.Single(failed.Messages));
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
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
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
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
			TestContextInternal.Current.DiagnosticMessageSink = spy;
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

	public class TestMethodOrderer
	{
		[Theory]
		[InlineData(typeof(AssemblyLevel))]
		[InlineData(typeof(CollectionLevel))]
		[InlineData(typeof(ClassLevel))]
		public static async ValueTask UsesCustomTestOrderer(Type testClassType)
		{
			var testAssembly =
				testClassType == typeof(AssemblyLevel)
					? Mocks.XunitTestAssembly(testMethodOrderer: new CustomTestMethodOrderer())
					: TestData.XunitTestAssembly(testClassType.Assembly);
			var testCollection = new CollectionPerClassTestCollectionFactory(testAssembly).Get(testClassType);
			var testClass = TestData.XunitTestClass(testClassType, testCollection);
			var testMethod = TestData.XunitTestMethod(testClass, testClassType.GetMethod("Passing") ?? throw new InvalidOperationException("Passing method not found"));
			var testCase = TestData.XunitTestCase(testMethod);
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.IsType<CustomTestMethodOrderer>(runner.RunTestMethods_TestMethodOrderer);
		}

		[Fact]
		public static async ValueTask OrdersTestMethods()
		{
			var testAssembly = Mocks.XunitTestAssembly(testMethodOrderer: UnorderedTestMethodOrderer.Instance);
			var testCollection = TestData.XunitTestCollection(testAssembly);
			var testClass = TestData.XunitTestClass(typeof(AssemblyLevel), testCollection);
			var testMethod1 = TestData.XunitTestMethod(testClass, typeof(AssemblyLevel).GetMethod("Passing")!, uniqueID: "test-method-1");
			var testCase1 = TestData.XunitTestCase(testMethod1, uniqueID: "test-case-1");
			var testMethod2 = TestData.XunitTestMethod(testClass, typeof(AssemblyLevel).GetMethod("Passing")!, uniqueID: "test-method-2");
			var testCase2 = TestData.XunitTestCase(testMethod2, uniqueID: "test-case-2");
			var testMethod3 = TestData.XunitTestMethod(testClass, typeof(AssemblyLevel).GetMethod("Passing")!, uniqueID: "test-method-3");
			var testCase3 = TestData.XunitTestCase(testMethod3, uniqueID: "test-case-3");
			var runner = new TestableXunitTestClassRunner(testCase3, testCase1, testCase2);

			await runner.RunAsync();

			Assert.IsType<UnorderedTestMethodOrderer>(runner.RunTestMethods_TestMethodOrderer);
			Assert.Collection(
				runner.RunTestMethod__MethodsRun,
				tm =>
				{
					Assert.Equal("test-method-3", tm.TestMethod!.UniqueID);
					Assert.Equal(["test-case-3"], tm.TestCases.Select(tc => tc.UniqueID));
				},
				tm =>
				{
					Assert.Equal("test-method-1", tm.TestMethod!.UniqueID);
					Assert.Equal(["test-case-1"], tm.TestCases.Select(tc => tc.UniqueID));
				},
				tm =>
				{
					Assert.Equal("test-method-2", tm.TestMethod!.UniqueID);
					Assert.Equal(["test-case-2"], tm.TestCases.Select(tc => tc.UniqueID));
				}
			);
		}

		[Fact]
		public static async ValueTask SettingTestCaseOrdererWithThrowingConstructorLogsDiagnosticMessage()
		{
			var spy = SpyMessageSink.Capture();
			TestContextInternal.Current.DiagnosticMessageSink = spy;
			var testCase = TestData.XunitTestCase<TestClassWithCtorThrowingTestMethodOrder>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			var diagnosticMessage = Assert.Single(spy.Messages.Cast<IDiagnosticMessage>());
			Assert.StartsWith($"Class-level test method orderer '{typeof(MyCtorThrowingTestMethodOrderer).SafeName()}' for test class '{typeof(TestClassWithCtorThrowingTestMethodOrder).SafeName()}' threw 'System.DivideByZeroException' during construction: Attempted to divide by zero.", diagnosticMessage.Message);
		}

		class AssemblyLevel  // Attribute injected via mock assembly
		{
			[Fact]
			public void Passing() { }
		}

		[TestMethodOrderer(typeof(CustomTestMethodOrderer))]
		public class CollectionLevelCollection { }

		[Collection(typeof(CollectionLevelCollection))]
		class CollectionLevel
		{
			[Fact]
			public void Passing() { }
		}

		[TestMethodOrderer(typeof(CustomTestMethodOrderer))]
		class ClassLevel
		{
			[Fact]
			public void Passing() { }
		}

		class CustomTestMethodOrderer : ITestMethodOrderer
		{
			public IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(IReadOnlyCollection<TTestMethod?> testMethods)
				where TTestMethod : notnull, ITestMethod =>
					testMethods;
		}

		[TestMethodOrderer(typeof(MyCtorThrowingTestMethodOrderer))]
		class TestClassWithCtorThrowingTestMethodOrder
		{
			[Fact]
			public void Passing() { }
		}

		class MyCtorThrowingTestMethodOrderer : ITestMethodOrderer
		{
			public MyCtorThrowingTestMethodOrderer()
			{
				throw new DivideByZeroException();
			}

			public IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(IReadOnlyCollection<TTestMethod?> testMethods)
				where TTestMethod : notnull, ITestMethod =>
					[];
		}
	}

	class FixtureUnderTest : IDisposable
	{
		public bool Disposed;

		public void Dispose() => Disposed = true;
	}

	class CollectionUnderTest : IClassFixture<object> { }

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources

	class ClassUnderTest(FixtureUnderTest _1, string _2, decimal _3) :
		IClassFixture<FixtureUnderTest>
	{
		[Fact]
		public void Passing() { }
	}

#pragma warning restore xUnit1041

	class TestableXunitTestClassRunner(params IXunitTestCase[] testCases) :
		XunitTestClassRunner
	{
		public readonly ExceptionAggregator Aggregator = new();
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public FixtureMappingManager CollectionFixtureMappingManager = new("[Unit Test] Test Collection");
		public readonly SpyMessageBus MessageBus = new();

		public ValueTask<RunSummary> RunAsync() =>
			Run(
				testCases[0].TestClass,
				testCases,
				ExplicitOption.Off,
				MessageBus,
				DefaultTestMethodOrderer.Instance,
				DefaultTestCaseOrderer.Instance,
				Aggregator,
				CancellationTokenSource,
				CollectionFixtureMappingManager
			);

		public object?[]? CreateTestClassConstructorArguments_ConstructorArguments;

		protected override async ValueTask<object?[]> CreateTestClassConstructorArguments(XunitTestClassRunnerContext ctxt)
		{
			CreateTestClassConstructorArguments_ConstructorArguments = await base.CreateTestClassConstructorArguments(ctxt);

			return CreateTestClassConstructorArguments_ConstructorArguments;
		}

		//// Override this because it needs a real test class, and we're using mocks. We'll just remove
		//// the exception that's thrown here (because we have no way to call the grandparent method).
		//protected override async ValueTask<bool> OnTestClassStarting(XunitTestClassRunnerContext ctxt)
		//{
		//	try
		//	{
		//		return await base.OnTestClassStarting(ctxt);
		//	}
		//	finally
		//	{
		//		ctxt.Aggregator.Clear();
		//	}
		//}

		public List<(IXunitTestMethod? TestMethod, IReadOnlyCollection<IXunitTestCase> TestCases)> RunTestMethod__MethodsRun = [];

		protected override ValueTask<RunSummary> RunTestMethod(
			XunitTestClassRunnerContext ctxt,
			IXunitTestMethod? testMethod,
			IReadOnlyCollection<IXunitTestCase> testCases,
			object?[] constructorArguments)
		{
			RunTestMethod__MethodsRun.Add((testMethod, testCases));

			return base.RunTestMethod(ctxt, testMethod, testCases, constructorArguments);
		}

		public IReadOnlyDictionary<Type, object>? RunTestMethods_ClassFixtures;
		public ITestMethodOrderer? RunTestMethods_TestMethodOrderer;

		protected override ValueTask<RunSummary> RunTestMethods(
			XunitTestClassRunnerContext ctxt,
			Exception? exception)
		{
			RunTestMethods_ClassFixtures = ctxt.ClassFixtureMappings.GetFixtureCache();
			RunTestMethods_TestMethodOrderer = ctxt.TestMethodOrderer;

			return base.RunTestMethods(ctxt, exception);
		}
	}
}
