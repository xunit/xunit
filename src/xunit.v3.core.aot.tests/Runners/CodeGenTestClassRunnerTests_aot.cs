using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class CodeGenTestClassRunnerTests
{
	[Collection("Shared state in FixtureWithEvents")]
	public static class Fixtures
	{
		[Fact]
		public static async ValueTask FixtureCreationFailure()
		{
			var testClass = Mocks.CodeGenTestClass<MyTestClass>(classFixtureFactories: new Dictionary<Type, FixtureFactory>()
			{
				[typeof(ThrowingCtorFixture)] = async (_, _) => new ThrowingCtorFixture()
			});
			var testMethod = Mocks.CodeGenTestMethod(testClass: testClass);
			var testCase = Mocks.CodeGenTestCase(testMethod: testMethod);
			var runner = new TestableCodeGenTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				// ...no invocation because of the startup failure...
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal([-1, 0], failed.ExceptionParentIndices);
					Assert.Equal(new string[] { typeof(TestPipelineException).SafeName(), typeof(DivideByZeroException).SafeName() }, failed.ExceptionTypes);
					Assert.Equal(new string[] { $"Class fixture type '{typeof(ThrowingCtorFixture).SafeName()}' threw in its constructor", "Attempted to divide by zero." }, failed.Messages);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
			);
		}

		class MyTestClass { }

		class ThrowingCtorFixture
		{
			public ThrowingCtorFixture() => throw new DivideByZeroException();
		}

		[Fact]
		public static async ValueTask FixtureDisposeFailure()
		{
			var testClass = Mocks.CodeGenTestClass<MyTestClass>(classFixtureFactories: new Dictionary<Type, FixtureFactory>()
			{
				[typeof(ThrowingDisposeFixture)] = async (_, _) => new ThrowingDisposeFixture()
			});
			var testMethod = Mocks.CodeGenTestMethod(testClass: testClass);
			var testCase = Mocks.CodeGenTestCase(testMethod: testMethod);
			var runner = new TestableCodeGenTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
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
				msg =>
				{
					var failure = Assert.IsType<ITestClassCleanupFailure>(msg, exactMatch: false);
					Assert.Equal([-1, 0], failure.ExceptionParentIndices);
					Assert.Equal(new[] { typeof(TestPipelineException).SafeName(), typeof(DivideByZeroException).SafeName() }, failure.ExceptionTypes);
					Assert.Equal(new[] { $"Class fixture type '{typeof(ThrowingDisposeFixture).SafeName()}' threw in Dispose", "Attempted to divide by zero." }, failure.Messages);
				},
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
			);
		}

		class ThrowingDisposeFixture : IDisposable
		{
			public void Dispose() => throw new DivideByZeroException();
		}

		[Fact]
		public static async ValueTask IAsyncDisposableIsPreferredOverIDisposable()
		{
			var testClass = Mocks.CodeGenTestClass<MyTestClass>(classFixtureFactories: new Dictionary<Type, FixtureFactory>()
			{
				[typeof(ThrowingDisposeWithAsyncDisposeFixture)] = async (_, _) => new ThrowingDisposeWithAsyncDisposeFixture()
			});
			var testMethod = Mocks.CodeGenTestMethod(testClass: testClass);
			var testCase = Mocks.CodeGenTestCase(testMethod: testMethod);
			var runner = new TestableCodeGenTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
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

		class ThrowingDisposeWithAsyncDisposeFixture : IAsyncDisposable, IDisposable
		{
			public void Dispose() => throw new DivideByZeroException();

			public ValueTask DisposeAsync() => default;
		}

		[Fact]
		public static async ValueTask FixtureEvents()
		{
			FixtureWithEvents.Events.Clear();

			var factories = new Dictionary<Type, FixtureFactory> { [typeof(FixtureWithEvents)] = (_, _) => new(new FixtureWithEvents()) };
			var testClass = Mocks.CodeGenTestClass(classFixtureFactories: factories);
			var testMethod = Mocks.CodeGenTestMethod(testClass: testClass);
			var testCase = Mocks.CodeGenTestCase(testMethod: testMethod);
			var runner = new TestableCodeGenTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				FixtureWithEvents.Events,
				e => Assert.Equal("OnTestClassStarting", e),
				e => Assert.Equal("OnTestClassStartingAsync", e),

				e => Assert.Equal("OnTestMethodStarting", e),
				e => Assert.Equal("OnTestMethodStartingAsync", e),

				e => Assert.Equal("OnTestCaseStarting", e),
				e => Assert.Equal("OnTestCaseStartingAsync", e),

				e => Assert.Equal("OnTestStarting", e),
				e => Assert.Equal("OnTestStartingAsync", e),
				e => Assert.Equal("OnTestFinishedAsync", e),
				e => Assert.Equal("OnTestFinished", e),

				e => Assert.Equal("OnTestCaseFinishedAsync", e),
				e => Assert.Equal("OnTestCaseFinished", e),

				e => Assert.Equal("OnTestMethodFinishedAsync", e),
				e => Assert.Equal("OnTestMethodFinished", e),

				e => Assert.Equal("OnTestClassFinishedAsync", e),
				e => Assert.Equal("OnTestClassFinished", e)
			);
		}
	}

	class TestableCodeGenTestClassRunner(params ICodeGenTestCase[] testCases) :
		CodeGenTestClassRunner
	{
		public readonly ExceptionAggregator Aggregator = new();
		public FixtureMappingManager CollectionFixtureMappingManager = new("[Unit Test] Test Collection", TestData.EmptyFixtureFactories);
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public readonly SpyMessageBus MessageBus = new();

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
	}
}
