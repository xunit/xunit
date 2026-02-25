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
			var testCase = TestData.XunitTestCase<ClassWithFixtures>(nameof(ClassWithFixtures.Passing), testCollection: collection);
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
			var testCase = TestData.XunitTestCase<ClassWithFixtures>(nameof(ClassWithFixtures.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.NotNull(runner.RunTestMethods_ClassFixtures);
			var fixture = Assert.Single(runner.RunTestMethods_ClassFixtures.Select(kvp => kvp.Value).OfType<FixtureUnderTest>());
			Assert.True(fixture.Disposed);
		}

		[Fact]
		public static async ValueTask IAsyncDisposableIsPreferredOverIDisposable()
		{
			var testCase = TestData.XunitTestCase<TestClassForFixtureAsyncDisposableUnderTest>(nameof(TestClassForFixtureAsyncDisposableUnderTest.Passing));
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
			var testCase = TestData.XunitTestCase<TestClassWithMultiCtorClassFixture>(nameof(TestClassWithMultiCtorClassFixture.Passing));
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
			var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithDependency>(nameof(TestClassWithClassFixtureWithDependency.Passing));
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
			var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithDependency>(nameof(TestClassWithClassFixtureWithDependency.Passing));
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
			var testCase = TestData.XunitTestCase<TestClassWithClassFixtureWithMessageSinkDependency>(nameof(TestClassWithClassFixtureWithMessageSinkDependency.Passing));
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
		public static async ValueTask PassesFixtureValuesToTestClassConstructor()
		{
			var testCase = TestData.XunitTestCase<ClassWithFixtures>(nameof(ClassWithFixtures.Passing));
			var runner = new TestableXunitTestClassRunner(testCase) { CollectionFixtureMappingManager = new TestableFixtureMappingManager(42, "Hello, world!", 21.12m) };

			await runner.RunAsync();

			Assert.NotNull(runner.CreateTestClassConstructorArguments__ReturnValue);
			Assert.Collection(
				runner.CreateTestClassConstructorArguments__ReturnValue,
				arg => Assert.IsType<FixtureUnderTest>(arg),
				arg => Assert.Equal("Hello, world!", arg),
				arg => Assert.Equal(21.12m, arg),
				arg => Assert.Same(TestContextAccessor.Instance, arg),
				arg =>
				{
					var func = Assert.IsType<Func<ITestOutputHelper>>(arg);
					Assert.Same(TestContext.Current.TestOutputHelper, func());
				}
			);
		}

		[Fact]
		public static async ValueTask DefaultValuesInTestClassConstructor()
		{
			var testCase = TestData.XunitTestCase<ClassWithDefaultValues>(nameof(ClassWithDefaultValues.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.NotNull(runner.CreateTestClassConstructorArguments__ReturnValue);
			Assert.Collection(
				runner.CreateTestClassConstructorArguments__ReturnValue,
				arg => Assert.Equal(42, arg),
				arg => Assert.Equal("Hello", arg)
			);
		}

		[Fact]
		public static async ValueTask ParamsValueInTestClassConstructor()
		{
			var testCase = TestData.XunitTestCase<ClassWithParams>(nameof(ClassWithParams.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.NotNull(runner.CreateTestClassConstructorArguments__ReturnValue);
			var arg = Assert.Single(runner.CreateTestClassConstructorArguments__ReturnValue);
			var array = Assert.IsType<string[]>(arg);
			Assert.Empty(array);
		}

		class FixtureUnderTest : IDisposable
		{
			public bool Disposed;

			public void Dispose() => Disposed = true;
		}

		class CollectionUnderTest : IClassFixture<object> { }

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources

		class ClassWithFixtures(FixtureUnderTest _1, string _2, decimal _3, ITestContextAccessor _4, ITestOutputHelper _5) :
			IClassFixture<FixtureUnderTest>
		{
			[Fact]
			public void Passing() { }
		}

		class ClassWithDefaultValues(int x = 42, string y = "Hello")
		{
			[Fact]
			public void Passing() { }
		}

		class ClassWithParams(params string[] values)
		{
			[Fact]
			public void Passing() { }
		}

#pragma warning restore xUnit1041
	}

	public class Run
	{
		[Fact]
		public static async ValueTask OrdererWithThrowingConstructor()
		{
			var testCase = TestData.XunitTestCase<TestClassWithCtorThrowingOrderer>(nameof(TestClassWithCtorThrowingOrderer.Passing));
			var runner = new TestableXunitTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg =>
				{
					var failure = Assert.IsType<ITestClassCleanupFailure>(msg, exactMatch: false);
					Assert.Collection(
						failure.ExceptionTypes,
						type => Assert.Equal(typeof(TestPipelineException).SafeName(), type),
						type => Assert.Equal(typeof(DivideByZeroException).SafeName(), type)
					);
					Assert.Collection(
						failure.Messages,
						msg => Assert.Equal($"Class-level test method orderer '{typeof(MyCtorThrowingOrderer).FullName}' for test class '{typeof(TestClassWithCtorThrowingOrderer).FullName}' threw during construction", msg),
						msg => Assert.Equal("Attempted to divide by zero.", msg)
					);
				},
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
			);
		}

		[TestMethodOrderer(typeof(MyCtorThrowingOrderer))]
		class TestClassWithCtorThrowingOrderer
		{
			[Fact]
			public void Passing() { }
		}

		class MyCtorThrowingOrderer : ITestMethodOrderer
		{
			public MyCtorThrowingOrderer() =>
				throw new DivideByZeroException();

			public IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(IReadOnlyCollection<TTestMethod?> testMethods)
				where TTestMethod : notnull, ITestMethod =>
					[];
		}

		[Fact]
		public static async ValueTask CollectionFixtureOnTestClass()
		{
			var testCase = TestData.XunitTestCase<TestClassWithCollectionFixture>(nameof(TestClassWithCollectionFixture.Passing));
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
					Assert.Equal(typeof(TestPipelineException).SafeName(), failed.ExceptionTypes.Single());
					Assert.Equal("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead).", failed.Messages.Single());
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
			);
		}

		class TestClassWithCollectionFixture : ICollectionFixture<object>
		{
			[Fact]
			public void Passing() { }
		}

		[Fact]
		public static async ValueTask TooManyConstructors()
		{
			var testCase = TestData.XunitTestCase<TestClassWithTooManyConstructors>(nameof(TestClassWithTooManyConstructors.Passing));
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
					Assert.Equal(typeof(TestPipelineException).SafeName(), failed.ExceptionTypes.Single());
					Assert.Equal("A test class may only define a single public constructor.", failed.Messages.Single());
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
			);
		}

		class TestClassWithTooManyConstructors
		{
			public TestClassWithTooManyConstructors() { }
			public TestClassWithTooManyConstructors(int _) { }

			[Fact]
			public void Passing() { }
		}
	}

	class TestableXunitTestClassRunner(params IXunitTestCase[] testCases) :
		XunitTestClassRunner
	{
		public readonly ExceptionAggregator Aggregator = new();
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public FixtureMappingManager CollectionFixtureMappingManager = new("[Unit Test] Test Collection");
		public readonly SpyMessageBus MessageBus = new();

		public object?[]? CreateTestClassConstructorArguments__ReturnValue;

		protected override async ValueTask<object?[]> CreateTestClassConstructorArguments(XunitTestClassRunnerContext ctxt)
		{
			CreateTestClassConstructorArguments__ReturnValue = await base.CreateTestClassConstructorArguments(ctxt);

			return CreateTestClassConstructorArguments__ReturnValue;
		}

		public ValueTask<RunSummary> RunAsync() =>
			Run(
				testCases[0].TestClass,
				testCases,
				ExplicitOption.Off,
				MessageBus,
				Aggregator,
				CancellationTokenSource,
				CollectionFixtureMappingManager
			);

		public IReadOnlyDictionary<Type, object>? RunTestMethods_ClassFixtures;

		protected override ValueTask<RunSummary> RunTestMethods(
			XunitTestClassRunnerContext ctxt,
			Exception? exception)
		{
			RunTestMethods_ClassFixtures = ctxt.ClassFixtureMappings.GetFixtureCache();

			return base.RunTestMethods(ctxt, exception);
		}
	}
}
